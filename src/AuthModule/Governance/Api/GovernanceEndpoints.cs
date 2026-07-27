using AuthModule.Foundation.Domain.Entities;
using AuthModule.Foundation.Domain.Primitives;
using AuthModule.Governance.Application.Contracts;
using AuthModule.Governance.Domain;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace AuthModule.Governance.Api;

public static class GovernanceEndpoints
{
    public static IEndpointRouteBuilder MapGovernanceEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/governance")
            .WithTags("Governance");

        group.MapGet("/audit/security-events", async (
            DateTimeOffset? dateFrom,
            DateTimeOffset? dateTo,
            SecurityEventType? eventType,
            Guid? actorId,
            int? page,
            int? pageSize,
            HttpContext httpContext,
            IAuditQueryService service) =>
        {
            var context = BuildContext(httpContext);
            var result = await service.QuerySecurityEventsAsync(
                new AuditQueryRequest(dateFrom, dateTo, eventType, actorId, page ?? 1, pageSize ?? 100),
                context);
            return result.IsSuccess ? Results.Ok(result.Value) : result.Error.ToProblem();
        })
        .WithSummary("Queries security audit events.")
        .WithDescription("Returns filtered and paginated security audit events for compliance and forensic analysis.");

        group.MapPost("/evidence", async (EvidenceCaptureRequest request, HttpContext httpContext, IEvidenceService service) =>
        {
            var context = BuildContext(httpContext);
            var result = await service.CaptureAsync(request, context);
            return result.IsSuccess ? Results.Ok(result.Value) : result.Error.ToProblem();
        })
        .WithSummary("Captures governance evidence.")
        .WithDescription("Stores immutable evidence metadata for governance controls and audit traceability.");

        group.MapPost("/evidence/export", async (EvidenceExportRequest request, HttpContext httpContext, IEvidenceService service) =>
        {
            var context = BuildContext(httpContext);
            var result = await service.ExportAsync(request, context);
            return result.IsSuccess ? Results.Ok(result.Value) : result.Error.ToProblem();
        })
        .WithSummary("Exports evidence records.")
        .WithDescription("Builds an export package for evidence records in the requested scope and format.");

        group.MapPost("/data-subject/requests", async (DataSubjectRequestCommand request, HttpContext httpContext, IDataSubjectService service) =>
        {
            var context = BuildContext(httpContext);
            var result = await service.SubmitAsync(request, context);
            return result.IsSuccess ? Results.Ok(result.Value) : result.Error.ToProblem();
        })
        .WithSummary("Submits a data-subject request.")
        .WithDescription("Creates a request for data-subject rights workflows such as access, correction, or erasure.");

        group.MapPost("/retention/invoke", async (RetentionInvocationRequest request, HttpContext httpContext, IRetentionService service) =>
        {
            var context = BuildContext(httpContext);
            var result = await service.InvokeAsync(request, context);
            return result.IsSuccess ? Results.Ok(result.Value) : result.Error.ToProblem();
        })
        .WithSummary("Invokes retention processing.")
        .WithDescription("Executes retention policy evaluation and returns the retention action results.");

        group.MapPost("/incidents", async (CreateIncidentRequest request, HttpContext httpContext, IIncidentService service) =>
        {
            var context = BuildContext(httpContext);
            var result = await service.CreateAsync(request, context);
            return result.IsSuccess ? Results.Ok(result.Value) : result.Error.ToProblem();
        })
        .WithSummary("Creates a governance incident.")
        .WithDescription("Registers a new governance or security incident for investigation and tracking.");

        group.MapPost("/incidents/status", async (AdvanceIncidentStatusRequest request, HttpContext httpContext, IIncidentService service) =>
        {
            var context = BuildContext(httpContext);
            var result = await service.AdvanceStatusAsync(request, context);
            return result.IsSuccess ? Results.Ok(result.Value) : result.Error.ToProblem();
        })
        .WithSummary("Advances incident status.")
        .WithDescription("Transitions an existing incident to its next lifecycle state with audit context.");

        group.MapPost("/backups", async (BackupMetadataAppendRequest request, HttpContext httpContext, IBackupEvidenceService service) =>
        {
            var context = BuildContext(httpContext);
            var result = await service.AppendAsync(request, context);
            return result.IsSuccess ? Results.Ok(result.Value) : result.Error.ToProblem();
        })
        .WithSummary("Appends backup evidence metadata.")
        .WithDescription("Records backup execution metadata to preserve compliance evidence for recovery operations.");

        group.MapPost("/backups/status", async (BackupMetadataStatusRequest request, HttpContext httpContext, IBackupEvidenceService service) =>
        {
            var context = BuildContext(httpContext);
            var result = await service.TransitionStatusAsync(request, context);
            return result.IsSuccess ? Results.Ok(result.Value) : result.Error.ToProblem();
        })
        .WithSummary("Updates backup evidence status.")
        .WithDescription("Changes lifecycle status for an existing backup evidence record.");

        return app;
    }

    private static RequestContext BuildContext(HttpContext context)
    {
        Guid? userId = Guid.TryParse(context.Request.Headers["X-Actor-UserId"], out var parsedUserId) ? parsedUserId : null;
        Guid? sessionId = Guid.TryParse(context.Request.Headers["X-Session-Id"], out var parsedSessionId) ? parsedSessionId : null;
        return new RequestContext(
            CorrelationId: Guid.NewGuid(),
            UserId: userId,
            SourceIp: context.Connection.RemoteIpAddress?.ToString(),
            Timestamp: DateTimeOffset.UtcNow,
            SessionId: sessionId);
    }
}
