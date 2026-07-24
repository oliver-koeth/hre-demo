using AuthModule.Foundation.Domain.Entities;
using AuthModule.Foundation.Domain.Primitives;
using AuthModule.Foundation.Persistence.Contracts;

namespace AuthModule.Integration.Tests.Support;

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
