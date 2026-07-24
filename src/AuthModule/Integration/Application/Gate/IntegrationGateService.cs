using System.Security.Cryptography;
using System.Text;
using AuthModule.Foundation.Domain.Primitives;
using AuthModule.Integration.Application.Common;
using AuthModule.Integration.Application.Contracts;
using AuthModule.Integration.Configuration;
using AuthModule.Integration.Domain;

namespace AuthModule.Integration.Application.Gate;

public sealed class IntegrationGateService(
    IContractConformanceChecker contractConformanceChecker,
    ITraceabilityChecker traceabilityChecker,
    IRuntimeArtifactChecker runtimeArtifactChecker,
    IIntegrationStateStore stateStore,
    IIntegrationAlertService alertService,
    IntegrationConfiguration configuration) : IIntegrationGateService
{
    public async Task<Result<GateExecutionResult, DomainError>> ExecuteAsync(ExecuteGateRequest request, RequestContext context)
    {
        if (request.Options.Endpoints.Count == 0)
        {
            return Result<GateExecutionResult, DomainError>.Failure(ErrorFactory.Validation("At least one endpoint descriptor is required.", context));
        }

        var readinessFailures = request.Options.UnitReadiness
            .Where(x => !x.IsImplemented)
            .Select(x => CreateBlocking("UnitReadiness", "UNIT_NOT_IMPLEMENTED", $"Unit {x.UnitId} is not implemented.", context))
            .ToList();
        if (readinessFailures.Count > 0)
        {
            return await BuildAndPersistFailureAsync(readinessFailures, [], [], "Unit readiness check failed.", context);
        }

        var conformanceResult = await contractConformanceChecker.CheckAsync(request.Options.Endpoints, context);
        if (conformanceResult.IsFailure)
        {
            return Result<GateExecutionResult, DomainError>.Failure(conformanceResult.Error);
        }

        var conformanceFailures = conformanceResult.Value
            .Where(x => x.IsBlocking)
            .Select(x => CreateBlocking("ContractConformance", "CONTRACT_CONFORMANCE_FAILED", $"{x.EndpointKey}: {x.Message}", context))
            .ToList();
        if (conformanceFailures.Count > 0)
        {
            return await BuildAndPersistFailureAsync(conformanceFailures, conformanceResult.Value, [], "Contract conformance check failed.", context);
        }

        var traceabilityResult = await traceabilityChecker.CheckAsync(request.Options.StoryMappings, context);
        if (traceabilityResult.IsFailure)
        {
            return Result<GateExecutionResult, DomainError>.Failure(traceabilityResult.Error);
        }

        var traceabilityFailures = traceabilityResult.Value
            .Where(x => !x.IsCovered)
            .Select(x => CreateBlocking("Traceability", "TRACEABILITY_GAP", $"{x.StoryId}: {x.Note}", context))
            .ToList();
        if (traceabilityFailures.Count > 0)
        {
            return await BuildAndPersistFailureAsync(traceabilityFailures, conformanceResult.Value, traceabilityResult.Value, "Traceability coverage check failed.", context);
        }

        var runtimeResult = await runtimeArtifactChecker.CheckAsync(context);
        if (runtimeResult.IsFailure)
        {
            return Result<GateExecutionResult, DomainError>.Failure(runtimeResult.Error);
        }

        if (runtimeResult.Value.Count > 0)
        {
            return await BuildAndPersistFailureAsync(runtimeResult.Value, conformanceResult.Value, traceabilityResult.Value, "Runtime artifact check failed.", context);
        }

        var passFingerprint = BuildFingerprint([], conformanceResult.Value, traceabilityResult.Value);
        var passDecision = new GateDecisionRecord
        {
            GateRunId = Guid.NewGuid(),
            Status = GateDecisionStatus.Pass,
            DecisionFingerprint = passFingerprint,
            BlockingFindings = [],
            SummaryNote = "Integration gate passed.",
            EvaluatedAt = context.Timestamp,
            RetentionExpiresAt = context.Timestamp.AddDays(configuration.GateEvidenceRetentionDays),
            CorrelationId = context.CorrelationId,
        };

        stateStore.ReplaceOpenBlockingFindings([]);
        stateStore.SaveGateDecision(passDecision);

        return Result<GateExecutionResult, DomainError>.Success(new GateExecutionResult
        {
            Decision = passDecision,
            ConformanceFindings = conformanceResult.Value,
            TraceabilityEntries = traceabilityResult.Value,
        });
    }

    public Task<Result<GateDecisionRecord, DomainError>> GetLatestDecisionAsync(RequestContext context)
    {
        var latest = stateStore.GetLatestDecision();
        if (latest is null)
        {
            return Task.FromResult(Result<GateDecisionRecord, DomainError>.Failure(ErrorFactory.NotFound("No gate decision found.", context)));
        }

        return Task.FromResult(Result<GateDecisionRecord, DomainError>.Success(latest));
    }

    private async Task<Result<GateExecutionResult, DomainError>> BuildAndPersistFailureAsync(
        IReadOnlyList<BlockingFindingRecord> failures,
        IReadOnlyList<ContractConformanceFinding> conformanceFindings,
        IReadOnlyList<TraceabilityCoverageEntry> traceabilityEntries,
        string summary,
        RequestContext context)
    {
        var fingerprint = BuildFingerprint(failures, conformanceFindings, traceabilityEntries);
        var failureDecision = new GateDecisionRecord
        {
            GateRunId = Guid.NewGuid(),
            Status = GateDecisionStatus.Fail,
            DecisionFingerprint = fingerprint,
            BlockingFindings = failures,
            SummaryNote = summary,
            EvaluatedAt = context.Timestamp,
            RetentionExpiresAt = context.Timestamp.AddDays(configuration.GateEvidenceRetentionDays),
            CorrelationId = context.CorrelationId,
        };

        stateStore.ReplaceOpenBlockingFindings(failures);
        stateStore.SaveGateDecision(failureDecision);
        await alertService.EmitGateFailureAsync(summary, context);

        return Result<GateExecutionResult, DomainError>.Success(new GateExecutionResult
        {
            Decision = failureDecision,
            ConformanceFindings = conformanceFindings,
            TraceabilityEntries = traceabilityEntries,
        });
    }

    private static string BuildFingerprint(
        IReadOnlyList<BlockingFindingRecord> failures,
        IReadOnlyList<ContractConformanceFinding> conformanceFindings,
        IReadOnlyList<TraceabilityCoverageEntry> traceabilityEntries)
    {
        var normalized = string.Join("|",
            failures.OrderBy(x => x.Code, StringComparer.Ordinal).ThenBy(x => x.Description, StringComparer.Ordinal)
                .Select(x => $"{x.Category}:{x.Code}:{x.Description}"));
        var conformance = string.Join("|",
            conformanceFindings.OrderBy(x => x.EndpointKey, StringComparer.Ordinal)
                .Select(x => $"{x.EndpointKey}:{x.HasProblemDetailsShape}:{x.HasErrorCodeExtension}:{x.HasCorrelationIdExtension}:{x.IsBlocking}"));
        var traceability = string.Join("|",
            traceabilityEntries.OrderBy(x => x.StoryId, StringComparer.Ordinal)
                .Select(x => $"{x.StoryId}:{x.IsCovered}:{x.FilePaths.Count}"));

        var raw = $"{normalized}||{conformance}||{traceability}";
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
        return Convert.ToHexString(hash);
    }

    private static BlockingFindingRecord CreateBlocking(
        string category,
        string code,
        string description,
        RequestContext context) =>
        new()
        {
            BlockingFindingId = Guid.NewGuid(),
            Category = category,
            Code = code,
            Description = description,
            IsOpen = true,
            CreatedAt = context.Timestamp,
            ResolvedAt = null,
            CorrelationId = context.CorrelationId,
        };
}
