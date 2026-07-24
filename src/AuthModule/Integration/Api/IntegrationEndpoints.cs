using AuthModule.Foundation.Domain.Primitives;
using AuthModule.Integration.Application.Contracts;
using AuthModule.Integration.Domain;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace AuthModule.Integration.Api;

public static class IntegrationEndpoints
{
    public static IEndpointRouteBuilder MapIntegrationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/integration");

        group.MapPost("/gate/execute", async (ExecuteGateRequest request, HttpContext httpContext, IIntegrationGateService service) =>
        {
            var context = BuildContext(httpContext);
            var result = await service.ExecuteAsync(request, context);
            return result.IsSuccess ? Results.Ok(result.Value) : result.Error.ToProblem();
        });

        group.MapGet("/gate/latest", async (HttpContext httpContext, IIntegrationGateService service) =>
        {
            var context = BuildContext(httpContext);
            var result = await service.GetLatestDecisionAsync(context);
            return result.IsSuccess ? Results.Ok(result.Value) : result.Error.ToProblem();
        });

        return app;
    }

    public static ExecuteGateRequest BuildDefaultRequest()
    {
        var endpoints = new List<EndpointRouteDescriptor>
        {
            new("core-login", "/api/core-security/auth/login", Path.Combine("src","AuthModule","CoreSecurity","Api","CoreSecurityEndpoints.cs"), Path.Combine("src","AuthModule","CoreSecurity","Api","CoreSecurityProblemDetails.cs")),
            new("core-validate", "/api/core-security/auth/validate", Path.Combine("src","AuthModule","CoreSecurity","Api","CoreSecurityEndpoints.cs"), Path.Combine("src","AuthModule","CoreSecurity","Api","CoreSecurityProblemDetails.cs")),
            new("governance-evidence", "/api/governance/evidence", Path.Combine("src","AuthModule","Governance","Api","GovernanceEndpoints.cs"), Path.Combine("src","AuthModule","Governance","Api","GovernanceProblemDetails.cs")),
            new("governance-retention", "/api/governance/retention/invoke", Path.Combine("src","AuthModule","Governance","Api","GovernanceEndpoints.cs"), Path.Combine("src","AuthModule","Governance","Api","GovernanceProblemDetails.cs")),
        };

        var storyMappings = IntegrationStories.RequiredStoryIds
            .Select(storyId => new StoryTraceabilityInput(
                storyId,
                storyId is "US-01" or "US-02" or "US-03" or "US-04" or "US-05" or "US-06" ? "UOW-02" : "UOW-03",
                storyId is "US-01" or "US-02" or "US-03" or "US-04" or "US-05" or "US-06"
                    ? [Path.Combine("src", "AuthModule", "CoreSecurity", "Api", "CoreSecurityEndpoints.cs")]
                    : [Path.Combine("src", "AuthModule", "Governance", "Api", "GovernanceEndpoints.cs")]))
            .ToList();

        var unitReadiness = new List<IntegrationReadinessRecord>
        {
            new() { UnitId = "UOW-01", IsImplemented = true, Note = "Foundation complete." },
            new() { UnitId = "UOW-02", IsImplemented = true, Note = "Core security complete." },
            new() { UnitId = "UOW-03", IsImplemented = true, Note = "Governance complete." },
        };

        return new ExecuteGateRequest(new IntegrationGateOptions(endpoints, storyMappings, unitReadiness));
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
