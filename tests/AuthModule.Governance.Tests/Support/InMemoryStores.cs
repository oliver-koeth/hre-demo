using System.Collections.Concurrent;
using AuthModule.Foundation.Domain.Entities;
using AuthModule.Foundation.Domain.Primitives;
using AuthModule.Foundation.Persistence.Contracts;

namespace AuthModule.Governance.Tests.Support;

public sealed class InMemoryStoreRepository<T> : IStoreRepository<T> where T : class, IStoreEntity
{
    private readonly ConcurrentDictionary<Guid, T> _store = new();

    public Task<Result<T?, DomainError>> GetAsync(StoreQuery query, RequestContext context)
    {
        _store.TryGetValue(query.Id, out var entity);
        if (entity is not null && !query.IncludeDeleted && entity.IsDeleted)
        {
            entity = null;
        }

        return Task.FromResult(Result<T?, DomainError>.Success(entity));
    }

    public Task<Result<IReadOnlyList<T>, DomainError>> SearchAsync(StoreSearchQuery<T> query, RequestContext context)
    {
        IEnumerable<T> records = _store.Values.Where(query.Predicate);
        if (!query.IncludeDeleted)
        {
            records = records.Where(x => !x.IsDeleted);
        }

        return Task.FromResult(Result<IReadOnlyList<T>, DomainError>.Success(records.ToList()));
    }

    public Task<Result<T, DomainError>> SaveAsync(T entity, int? expectedVersion, RequestContext context)
    {
        if (_store.TryGetValue(entity.Id, out var existing))
        {
            if (expectedVersion is null || existing.Version != expectedVersion.Value)
            {
                return Task.FromResult(Result<T, DomainError>.Failure(new DomainError(DomainErrorCode.Conflict, "Version mismatch.", context.CorrelationId)));
            }

            entity.Version = existing.Version + 1;
        }
        else
        {
            entity.Version = 0;
        }

        _store[entity.Id] = entity;
        return Task.FromResult(Result<T, DomainError>.Success(entity));
    }

    public Task<Result<T, DomainError>> SoftDeleteAsync(Guid id, int expectedVersion, RequestContext context)
    {
        if (!_store.TryGetValue(id, out var existing))
        {
            return Task.FromResult(Result<T, DomainError>.Failure(new DomainError(DomainErrorCode.NotFound, "Not found.", context.CorrelationId)));
        }

        if (existing.Version != expectedVersion)
        {
            return Task.FromResult(Result<T, DomainError>.Failure(new DomainError(DomainErrorCode.Conflict, "Version mismatch.", context.CorrelationId)));
        }

        existing.IsDeleted = true;
        existing.DeletedAt = context.Timestamp;
        existing.DeletedBy = context.UserId;
        existing.Version += 1;
        _store[id] = existing;
        return Task.FromResult(Result<T, DomainError>.Success(existing));
    }
}

public sealed class InMemoryAuditStoreRepository : IAuditStoreRepository
{
    public List<SecurityAuditEvent> SecurityEvents { get; } = [];
    public List<AdminChangeEvent> AdminEvents { get; } = [];

    public Task<Result<Unit, DomainError>> AppendSecurityEventAsync(SecurityAuditEvent evt, RequestContext context)
    {
        SecurityEvents.Add(evt);
        return Task.FromResult(Result<Unit, DomainError>.Success(new Unit()));
    }

    public Task<Result<Unit, DomainError>> AppendAdminChangeEventAsync(AdminChangeEvent evt, RequestContext context)
    {
        AdminEvents.Add(evt);
        return Task.FromResult(Result<Unit, DomainError>.Success(new Unit()));
    }

    public Task<Result<IReadOnlyList<SecurityAuditEvent>, DomainError>> QuerySecurityEventsAsync(RequestContext context) =>
        Task.FromResult(Result<IReadOnlyList<SecurityAuditEvent>, DomainError>.Success(SecurityEvents));

    public Task<Result<IReadOnlyList<AdminChangeEvent>, DomainError>> QueryAdminChangeEventsAsync(RequestContext context) =>
        Task.FromResult(Result<IReadOnlyList<AdminChangeEvent>, DomainError>.Success(AdminEvents));
}
