using AuthModule.Foundation.Domain.Entities;
using AuthModule.Foundation.Domain.Primitives;

namespace AuthModule.Foundation.Persistence.Contracts;

public sealed record StoreQuery(Guid Id, bool IncludeDeleted = false);
public sealed record StoreSearchQuery<T>(Func<T, bool> Predicate, bool IncludeDeleted = false);
public sealed record StoreIntegrityCheckRequest(IReadOnlyCollection<string>? RelativePaths = null);

public enum IntegrityCheckOutcome
{
    Pass,
    Fail,
}

public sealed class StoreIntegrityResult
{
    public required bool AllPassed { get; init; }
    public required IReadOnlyDictionary<string, IntegrityCheckOutcome> FileResults { get; init; }
}

public interface IStoreRepository<T> where T : class, IStoreEntity
{
    Task<Result<T?, DomainError>> GetAsync(StoreQuery query, RequestContext context);
    Task<Result<IReadOnlyList<T>, DomainError>> SearchAsync(StoreSearchQuery<T> query, RequestContext context);
    Task<Result<T, DomainError>> SaveAsync(T entity, int? expectedVersion, RequestContext context);
    Task<Result<T, DomainError>> SoftDeleteAsync(Guid id, int expectedVersion, RequestContext context);
}

public interface IAuditStoreRepository
{
    Task<Result<Unit, DomainError>> AppendSecurityEventAsync(SecurityAuditEvent evt, RequestContext context);
    Task<Result<Unit, DomainError>> AppendAdminChangeEventAsync(AdminChangeEvent evt, RequestContext context);
    Task<Result<IReadOnlyList<SecurityAuditEvent>, DomainError>> QuerySecurityEventsAsync(RequestContext context);
    Task<Result<IReadOnlyList<AdminChangeEvent>, DomainError>> QueryAdminChangeEventsAsync(RequestContext context);
}

public interface IStoreIntegrityService
{
    Task<StoreIntegrityResult> VerifyAllStoresAsync(RequestContext context);
    Task<StoreIntegrityResult> VerifyStoreAsync(StoreIntegrityCheckRequest request, RequestContext context);
}

public readonly struct Unit;

