namespace AuthModule.Integration.Configuration;

public sealed class IntegrationConfiguration
{
    public string RepositoryRootPath { get; init; } = ".";
    public int GateDecisionTimeoutSeconds { get; init; } = 15;
    public int GateEvidenceRetentionDays { get; init; } = 90;
    public bool PreviewRuntimeApproved { get; init; } = true;

    public IReadOnlyList<string> Validate()
    {
        var issues = new List<string>();
        if (string.IsNullOrWhiteSpace(RepositoryRootPath)) issues.Add("RepositoryRootPath is required.");
        if (GateDecisionTimeoutSeconds < 1) issues.Add("GateDecisionTimeoutSeconds must be >= 1.");
        if (GateEvidenceRetentionDays < 1) issues.Add("GateEvidenceRetentionDays must be >= 1.");
        return issues;
    }
}
