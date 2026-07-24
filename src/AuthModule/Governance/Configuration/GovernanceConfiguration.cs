namespace AuthModule.Governance.Configuration;

public sealed class GovernanceConfiguration
{
    public int DefaultQueryPageSize { get; init; } = 100;
    public int MaxQueryPageSize { get; init; } = 500;
    public int ExportChunkSize { get; init; } = 500;
    public int BackupMetadataFreshnessSeconds { get; init; } = 30;
    public int EvidenceRetentionMinimumDays { get; init; } = 180;
    public bool PreviewRuntimeApproved { get; init; } = true;

    public IReadOnlyList<string> Validate()
    {
        var issues = new List<string>();
        if (DefaultQueryPageSize < 1) issues.Add("DefaultQueryPageSize must be >= 1.");
        if (MaxQueryPageSize < DefaultQueryPageSize) issues.Add("MaxQueryPageSize must be >= DefaultQueryPageSize.");
        if (ExportChunkSize < 1) issues.Add("ExportChunkSize must be >= 1.");
        if (BackupMetadataFreshnessSeconds < 1) issues.Add("BackupMetadataFreshnessSeconds must be >= 1.");
        if (EvidenceRetentionMinimumDays < 1) issues.Add("EvidenceRetentionMinimumDays must be >= 1.");
        return issues;
    }
}
