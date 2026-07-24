using AuthModule.Foundation.Domain.Primitives;

namespace AuthModule.Integration.Domain;

public enum GateDecisionStatus
{
    Pass,
    Fail,
}

public sealed class IntegrationReadinessRecord
{
    public required string UnitId { get; init; }
    public required bool IsImplemented { get; init; }
    public string Note { get; init; } = string.Empty;
}

public sealed class ContractConformanceFinding
{
    public required Guid FindingId { get; init; }
    public required string EndpointKey { get; init; }
    public required string Route { get; init; }
    public required bool HasProblemDetailsShape { get; init; }
    public required bool HasErrorCodeExtension { get; init; }
    public required bool HasCorrelationIdExtension { get; init; }
    public required bool IsBlocking { get; init; }
    public required string Message { get; init; }
}

public sealed class TraceabilityCoverageEntry
{
    public required string StoryId { get; init; }
    public required string UnitId { get; init; }
    public required IReadOnlyList<string> FilePaths { get; init; }
    public required bool IsCovered { get; init; }
    public string Note { get; init; } = string.Empty;
}

public sealed class BlockingFindingRecord
{
    public required Guid BlockingFindingId { get; init; }
    public required string Category { get; init; }
    public required string Code { get; init; }
    public required string Description { get; init; }
    public required bool IsOpen { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public required DateTimeOffset? ResolvedAt { get; init; }
    public required Guid CorrelationId { get; init; }
}

public sealed class GateDecisionRecord
{
    public required Guid GateRunId { get; init; }
    public required GateDecisionStatus Status { get; init; }
    public required string DecisionFingerprint { get; init; }
    public required IReadOnlyList<BlockingFindingRecord> BlockingFindings { get; init; }
    public required string SummaryNote { get; init; }
    public required DateTimeOffset EvaluatedAt { get; init; }
    public required DateTimeOffset RetentionExpiresAt { get; init; }
    public required Guid CorrelationId { get; init; }
}

public sealed class GateExecutionResult
{
    public required GateDecisionRecord Decision { get; init; }
    public required IReadOnlyList<ContractConformanceFinding> ConformanceFindings { get; init; }
    public required IReadOnlyList<TraceabilityCoverageEntry> TraceabilityEntries { get; init; }
}

public sealed record EndpointRouteDescriptor(
    string EndpointKey,
    string Route,
    string EndpointFilePath,
    string ProblemDetailsFilePath);

public sealed record StoryTraceabilityInput(string StoryId, string UnitId, IReadOnlyList<string> FilePaths);

public sealed record IntegrationGateOptions(
    IReadOnlyList<EndpointRouteDescriptor> Endpoints,
    IReadOnlyList<StoryTraceabilityInput> StoryMappings,
    IReadOnlyList<IntegrationReadinessRecord> UnitReadiness);

public static class IntegrationStories
{
    public static readonly IReadOnlyList<string> RequiredStoryIds =
    [
        "US-01", "US-02", "US-03", "US-04", "US-05", "US-06",
        "US-07", "US-08", "US-09", "US-10", "US-11", "US-12"
    ];
}
