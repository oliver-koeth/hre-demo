using AuthModule.Foundation.Domain.Primitives;
using AuthModule.Integration.Domain;

namespace AuthModule.Integration.Application.Contracts;

public sealed record ExecuteGateRequest(IntegrationGateOptions Options);

public interface IIntegrationGateService
{
    Task<Result<GateExecutionResult, DomainError>> ExecuteAsync(ExecuteGateRequest request, RequestContext context);
    Task<Result<GateDecisionRecord, DomainError>> GetLatestDecisionAsync(RequestContext context);
}

public interface IContractConformanceChecker
{
    Task<Result<IReadOnlyList<ContractConformanceFinding>, DomainError>> CheckAsync(
        IReadOnlyList<EndpointRouteDescriptor> endpoints,
        RequestContext context);
}

public interface ITraceabilityChecker
{
    Task<Result<IReadOnlyList<TraceabilityCoverageEntry>, DomainError>> CheckAsync(
        IReadOnlyList<StoryTraceabilityInput> storyMappings,
        RequestContext context);
}

public interface IRuntimeArtifactChecker
{
    Task<Result<IReadOnlyList<BlockingFindingRecord>, DomainError>> CheckAsync(RequestContext context);
}

public interface IIntegrationAlertService
{
    Task EmitGateFailureAsync(string reason, RequestContext context);
}

public interface IIntegrationStateStore
{
    void SaveGateDecision(GateDecisionRecord decision);
    GateDecisionRecord? GetLatestDecision();
    void ReplaceOpenBlockingFindings(IReadOnlyList<BlockingFindingRecord> findings);
    IReadOnlyList<BlockingFindingRecord> GetOpenBlockingFindings();
}
