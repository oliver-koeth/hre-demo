using System.Text;
using System.Text.Json;
using AuthModule.Foundation.Domain.Entities;
using AuthModule.Foundation.Domain.Primitives;
using AuthModule.Foundation.Persistence.Contracts;
using AuthModule.Foundation.Persistence.Serialization;
using AuthModule.Foundation.Security;

namespace AuthModule.Foundation.Persistence.Repositories;

public sealed class JsonStoreRepository<T> : IStoreRepository<T> where T : class, IStoreEntity
{
    private readonly string _filePath;
    private readonly string _signaturePath;
    private readonly string _storeType;
    private readonly IEncryptionService _encryption;
    private readonly IIntegrityService _integrity;
    private readonly SemaphoreSlim _mutex = new(1, 1);

    public JsonStoreRepository(
        string filePath,
        string storeType,
        IEncryptionService encryption,
        IIntegrityService integrity)
    {
        _filePath = filePath;
        _signaturePath = $"{filePath}.sig";
        _storeType = storeType;
        _encryption = encryption;
        _integrity = integrity;
    }

    public async Task<Result<T?, DomainError>> GetAsync(StoreQuery query, RequestContext context)
    {
        var readResult = await ReadEnvelopeAsync(context);
        if (readResult.IsFailure) return Result<T?, DomainError>.Failure(readResult.Error);

        var records = readResult.Value.Records;
        var record = records.FirstOrDefault(x => x.Id == query.Id);
        if (record is null || (!query.IncludeDeleted && record.IsDeleted))
        {
            return Result<T?, DomainError>.Success(null);
        }

        return Result<T?, DomainError>.Success(record);
    }

    public async Task<Result<IReadOnlyList<T>, DomainError>> SearchAsync(StoreSearchQuery<T> query, RequestContext context)
    {
        var readResult = await ReadEnvelopeAsync(context);
        if (readResult.IsFailure) return Result<IReadOnlyList<T>, DomainError>.Failure(readResult.Error);

        var filtered = readResult.Value.Records
            .Where(query.Predicate)
            .Where(x => query.IncludeDeleted || !x.IsDeleted)
            .ToList();
        return Result<IReadOnlyList<T>, DomainError>.Success(filtered);
    }

    public async Task<Result<T, DomainError>> SaveAsync(T entity, int? expectedVersion, RequestContext context)
    {
        await _mutex.WaitAsync();
        try
        {
            var readResult = await ReadEnvelopeAsync(context);
            if (readResult.IsFailure) return Result<T, DomainError>.Failure(readResult.Error);

            var envelope = readResult.Value;
            var existing = envelope.Records.FirstOrDefault(x => x.Id == entity.Id);

            if (existing is null)
            {
                entity.Version = 0;
                envelope.Records.Add(entity);
            }
            else
            {
                if (expectedVersion is null || existing.Version != expectedVersion.Value)
                {
                    return Result<T, DomainError>.Failure(new DomainError(
                        DomainErrorCode.Conflict,
                        "Version mismatch.",
                        context.CorrelationId,
                        new Dictionary<string, string>
                        {
                            ["CurrentVersion"] = existing.Version.ToString(),
                            ["SuppliedVersion"] = expectedVersion?.ToString() ?? "null",
                        }));
                }

                entity.Version = existing.Version + 1;
                envelope.Records.Remove(existing);
                envelope.Records.Add(entity);
            }

            envelope.Header.RecordCount = envelope.Records.Count;
            await WriteEnvelopeAsync(envelope);
            return Result<T, DomainError>.Success(entity);
        }
        catch (Exception ex)
        {
            return Result<T, DomainError>.Failure(new DomainError(
                DomainErrorCode.Internal,
                ex.Message,
                context.CorrelationId));
        }
        finally
        {
            _mutex.Release();
        }
    }

    public async Task<Result<T, DomainError>> SoftDeleteAsync(Guid id, int expectedVersion, RequestContext context)
    {
        await _mutex.WaitAsync();
        try
        {
            var readResult = await ReadEnvelopeAsync(context);
            if (readResult.IsFailure) return Result<T, DomainError>.Failure(readResult.Error);

            var envelope = readResult.Value;
            var existing = envelope.Records.FirstOrDefault(x => x.Id == id);
            if (existing is null)
            {
                return Result<T, DomainError>.Failure(new DomainError(
                    DomainErrorCode.NotFound,
                    "Record not found.",
                    context.CorrelationId));
            }

            if (existing.Version != expectedVersion)
            {
                return Result<T, DomainError>.Failure(new DomainError(
                    DomainErrorCode.Conflict,
                    "Version mismatch.",
                    context.CorrelationId));
            }

            existing.IsDeleted = true;
            existing.DeletedAt = context.Timestamp;
            existing.DeletedBy = context.UserId;
            existing.Version += 1;
            envelope.Header.RecordCount = envelope.Records.Count;

            await WriteEnvelopeAsync(envelope);
            return Result<T, DomainError>.Success(existing);
        }
        finally
        {
            _mutex.Release();
        }
    }

    private async Task<Result<StoreEnvelope<T>, DomainError>> ReadEnvelopeAsync(RequestContext context)
    {
        EnsureParentDirectory();
        if (!File.Exists(_filePath))
        {
            var empty = new StoreEnvelope<T>
            {
                Header = new StoreFileHeader
                {
                    SchemaVersion = StoreSchema.CurrentVersion,
                    StoreType = _storeType,
                    RecordCount = 0,
                },
                Records = [],
            };
            await WriteEnvelopeAsync(empty);
            return Result<StoreEnvelope<T>, DomainError>.Success(empty);
        }

        if (!File.Exists(_signaturePath))
        {
            return Result<StoreEnvelope<T>, DomainError>.Failure(new DomainError(
                DomainErrorCode.IntegrityViolation,
                "Signature file missing.",
                context.CorrelationId));
        }

        var encrypted = await File.ReadAllBytesAsync(_filePath);
        var signature = await File.ReadAllBytesAsync(_signaturePath);
        if (!_integrity.Verify(encrypted, signature))
        {
            return Result<StoreEnvelope<T>, DomainError>.Failure(new DomainError(
                DomainErrorCode.IntegrityViolation,
                "HMAC signature mismatch.",
                context.CorrelationId));
        }

        try
        {
            var plaintext = _encryption.Decrypt(encrypted);
            var envelope = JsonSerializer.Deserialize<StoreEnvelope<T>>(plaintext, JsonStoreSerializerOptions.Default)
                           ?? new StoreEnvelope<T>
                           {
                               Header = new StoreFileHeader
                               {
                                   SchemaVersion = StoreSchema.CurrentVersion,
                                   StoreType = _storeType,
                               },
                               Records = [],
                           };

            if (envelope.Header.SchemaVersion != StoreSchema.CurrentVersion)
            {
                envelope = MigrateToCurrent(envelope);
            }

            return Result<StoreEnvelope<T>, DomainError>.Success(envelope);
        }
        catch (Exception ex)
        {
            return Result<StoreEnvelope<T>, DomainError>.Failure(new DomainError(
                DomainErrorCode.IntegrityViolation,
                $"Decryption/deserialize failure: {ex.Message}",
                context.CorrelationId));
        }
    }

    private async Task WriteEnvelopeAsync(StoreEnvelope<T> envelope)
    {
        envelope.Header.SchemaVersion = StoreSchema.CurrentVersion;
        envelope.Header.StoreType = _storeType;
        envelope.Header.RecordCount = envelope.Records.Count;

        var plaintext = JsonSerializer.SerializeToUtf8Bytes(envelope, JsonStoreSerializerOptions.Default);
        var encrypted = _encryption.Encrypt(plaintext);
        var signature = _integrity.ComputeSignature(encrypted);

        EnsureParentDirectory();

        var tmpFile = $"{_filePath}.tmp";
        var tmpSig = $"{_signaturePath}.tmp";
        await File.WriteAllBytesAsync(tmpFile, encrypted);
        await File.WriteAllBytesAsync(tmpSig, signature);

        File.Move(tmpFile, _filePath, overwrite: true);
        File.Move(tmpSig, _signaturePath, overwrite: true);
    }

    private StoreEnvelope<T> MigrateToCurrent(StoreEnvelope<T> source)
    {
        source.Header.SchemaVersion = StoreSchema.CurrentVersion;
        source.Header.StoreType = _storeType;
        source.Header.RecordCount = source.Records.Count;
        return source;
    }

    private void EnsureParentDirectory()
    {
        var dir = Path.GetDirectoryName(_filePath)!;
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
    }
}

public sealed class JsonAuditStoreRepository(
    JsonStoreRepository<AuditEventEnvelope<SecurityAuditEvent>> securityRepo,
    JsonStoreRepository<AuditEventEnvelope<AdminChangeEvent>> adminRepo) : IAuditStoreRepository
{
    public async Task<Result<Unit, DomainError>> AppendSecurityEventAsync(SecurityAuditEvent evt, RequestContext context)
    {
        var wrapper = new AuditEventEnvelope<SecurityAuditEvent> { EventId = evt.EventId, EventData = evt };
        var saved = await securityRepo.SaveAsync(wrapper, expectedVersion: null, context);
        return saved.IsSuccess
            ? Result<Unit, DomainError>.Success(new Unit())
            : Result<Unit, DomainError>.Failure(saved.Error);
    }

    public async Task<Result<Unit, DomainError>> AppendAdminChangeEventAsync(AdminChangeEvent evt, RequestContext context)
    {
        var wrapper = new AuditEventEnvelope<AdminChangeEvent> { EventId = evt.EventId, EventData = evt };
        var saved = await adminRepo.SaveAsync(wrapper, expectedVersion: null, context);
        return saved.IsSuccess
            ? Result<Unit, DomainError>.Success(new Unit())
            : Result<Unit, DomainError>.Failure(saved.Error);
    }

    public async Task<Result<IReadOnlyList<SecurityAuditEvent>, DomainError>> QuerySecurityEventsAsync(RequestContext context)
    {
        var found = await securityRepo.SearchAsync(new StoreSearchQuery<AuditEventEnvelope<SecurityAuditEvent>>(_ => true, true), context);
        return found.IsFailure
            ? Result<IReadOnlyList<SecurityAuditEvent>, DomainError>.Failure(found.Error)
            : Result<IReadOnlyList<SecurityAuditEvent>, DomainError>.Success(found.Value.Select(x => x.EventData).ToList());
    }

    public async Task<Result<IReadOnlyList<AdminChangeEvent>, DomainError>> QueryAdminChangeEventsAsync(RequestContext context)
    {
        var found = await adminRepo.SearchAsync(new StoreSearchQuery<AuditEventEnvelope<AdminChangeEvent>>(_ => true, true), context);
        return found.IsFailure
            ? Result<IReadOnlyList<AdminChangeEvent>, DomainError>.Failure(found.Error)
            : Result<IReadOnlyList<AdminChangeEvent>, DomainError>.Success(found.Value.Select(x => x.EventData).ToList());
    }
}

public sealed class AuditEventEnvelope<TEvent> : BaseStoreEntity
{
    public Guid EventId { get; set; }
    public required TEvent EventData { get; set; }
    public override Guid Id => EventId;
}

public sealed class StoreIntegrityService(
    IIntegrityService integrity,
    IEnumerable<string> storeFiles) : IStoreIntegrityService
{
    private readonly string[] _storeFiles = storeFiles.ToArray();

    public Task<StoreIntegrityResult> VerifyAllStoresAsync(RequestContext context) =>
        VerifyInternal(_storeFiles);

    public Task<StoreIntegrityResult> VerifyStoreAsync(StoreIntegrityCheckRequest request, RequestContext context)
    {
        if (request.RelativePaths is null || request.RelativePaths.Count == 0)
        {
            return VerifyInternal(_storeFiles);
        }

        var selected = _storeFiles.Where(path => request.RelativePaths.Contains(Path.GetFileName(path))).ToArray();
        return VerifyInternal(selected);
    }

    private Task<StoreIntegrityResult> VerifyInternal(IEnumerable<string> files)
    {
        var map = new Dictionary<string, IntegrityCheckOutcome>(StringComparer.OrdinalIgnoreCase);
        foreach (var file in files)
        {
            var sigPath = $"{file}.sig";
            if (!File.Exists(file) || !File.Exists(sigPath))
            {
                map[file] = IntegrityCheckOutcome.Fail;
                continue;
            }

            var encrypted = File.ReadAllBytes(file);
            var sig = File.ReadAllBytes(sigPath);
            map[file] = integrity.Verify(encrypted, sig) ? IntegrityCheckOutcome.Pass : IntegrityCheckOutcome.Fail;
        }

        return Task.FromResult(new StoreIntegrityResult
        {
            FileResults = map,
            AllPassed = map.Count > 0 && map.Values.All(v => v == IntegrityCheckOutcome.Pass),
        });
    }
}
