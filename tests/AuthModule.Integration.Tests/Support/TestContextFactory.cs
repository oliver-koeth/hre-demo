using AuthModule.Foundation.Domain.Primitives;
using AuthModule.Integration.Application.Alerts;
using AuthModule.Integration.Application.Conformance;
using AuthModule.Integration.Application.Contracts;
using AuthModule.Integration.Application.Gate;
using AuthModule.Integration.Application.Runtime;
using AuthModule.Integration.Application.Traceability;
using AuthModule.Integration.Configuration;
using AuthModule.Integration.Domain;
using AuthModule.Integration.Persistence;
using Microsoft.Extensions.Logging.Abstractions;

namespace AuthModule.Integration.Tests.Support;

public sealed class IntegrationTestContext
{
    public required string RepositoryRootPath { get; init; }
    public required RequestContext RequestContext { get; init; }
    public required IIntegrationGateService GateService { get; init; }
}

public static class TestContextFactory
{
    public static IntegrationTestContext Create(Action<string>? seed = null)
    {
        var root = Path.Combine(Path.GetTempPath(), $"integration-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        Directory.CreateDirectory(Path.Combine(root, "config"));
        Directory.CreateDirectory(Path.Combine(root, "src", "AuthModule", "CoreSecurity", "Api"));
        Directory.CreateDirectory(Path.Combine(root, "src", "AuthModule", "Governance", "Api"));
        File.WriteAllText(Path.Combine(root, "docker-compose.yml"), "services: {}\n");
        File.WriteAllText(Path.Combine(root, "config", "policy.template.json"), "{}\n");
        File.WriteAllText(Path.Combine(root, "src", "AuthModule", "CoreSecurity", "Api", "CoreSecurityEndpoints.cs"), "return result.IsSuccess ? Results.Ok(result.Value) : result.Error.ToProblem();");
        File.WriteAllText(Path.Combine(root, "src", "AuthModule", "CoreSecurity", "Api", "CoreSecurityProblemDetails.cs"), "var details = new ProblemDetails(); details.Extensions[\"errorCode\"] = \"X\"; details.Extensions[\"correlationId\"] = \"Y\";");
        File.WriteAllText(Path.Combine(root, "src", "AuthModule", "Governance", "Api", "GovernanceEndpoints.cs"), "return result.IsSuccess ? Results.Ok(result.Value) : result.Error.ToProblem();");
        File.WriteAllText(Path.Combine(root, "src", "AuthModule", "Governance", "Api", "GovernanceProblemDetails.cs"), "var details = new ProblemDetails(); details.Extensions[\"errorCode\"] = \"X\"; details.Extensions[\"correlationId\"] = \"Y\";");

        seed?.Invoke(root);

        var config = new IntegrationConfiguration
        {
            RepositoryRootPath = root,
            GateDecisionTimeoutSeconds = 15,
            GateEvidenceRetentionDays = 90,
            PreviewRuntimeApproved = true,
        };

        var auditStore = new InMemoryAuditStoreRepository();
        var stateStore = new InMemoryIntegrationStateStore();
        var conformanceChecker = new ContractConformanceChecker(config);
        var traceabilityChecker = new TraceabilityChecker(config);
        var runtimeChecker = new RuntimeArtifactChecker(config);
        var alerts = new IntegrationAlertService(auditStore, new NullLogger<IntegrationAlertService>());
        var gate = new IntegrationGateService(conformanceChecker, traceabilityChecker, runtimeChecker, stateStore, alerts, config);

        var context = new RequestContext(Guid.NewGuid(), Guid.NewGuid(), "127.0.0.1", DateTimeOffset.UtcNow, Guid.NewGuid());
        return new IntegrationTestContext
        {
            RepositoryRootPath = root,
            RequestContext = context,
            GateService = gate,
        };
    }

    public static ExecuteGateRequest BuildRequest()
    {
        var endpoints = new List<EndpointRouteDescriptor>
        {
            new("core-login", "/api/core-security/auth/login", Path.Combine("src","AuthModule","CoreSecurity","Api","CoreSecurityEndpoints.cs"), Path.Combine("src","AuthModule","CoreSecurity","Api","CoreSecurityProblemDetails.cs")),
            new("governance-evidence", "/api/governance/evidence", Path.Combine("src","AuthModule","Governance","Api","GovernanceEndpoints.cs"), Path.Combine("src","AuthModule","Governance","Api","GovernanceProblemDetails.cs")),
        };

        var storyMappings = IntegrationStories.RequiredStoryIds
            .Select(storyId =>
            {
                var filePath = storyId is "US-01" or "US-02" or "US-03" or "US-04" or "US-05" or "US-06"
                    ? Path.Combine("src", "AuthModule", "CoreSecurity", "Api", "CoreSecurityEndpoints.cs")
                    : Path.Combine("src", "AuthModule", "Governance", "Api", "GovernanceEndpoints.cs");
                var unitId = storyId is "US-01" or "US-02" or "US-03" or "US-04" or "US-05" or "US-06" ? "UOW-02" : "UOW-03";
                return new StoryTraceabilityInput(storyId, unitId, [filePath]);
            })
            .ToList();

        var readiness = new List<IntegrationReadinessRecord>
        {
            new() { UnitId = "UOW-01", IsImplemented = true, Note = "Ready" },
            new() { UnitId = "UOW-02", IsImplemented = true, Note = "Ready" },
            new() { UnitId = "UOW-03", IsImplemented = true, Note = "Ready" },
        };

        return new ExecuteGateRequest(new IntegrationGateOptions(endpoints, storyMappings, readiness));
    }
}
