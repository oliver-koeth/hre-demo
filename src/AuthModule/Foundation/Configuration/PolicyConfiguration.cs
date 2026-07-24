namespace AuthModule.Foundation.Configuration;

public sealed class PolicyConfiguration
{
    public int TokenLifetimeSeconds { get; init; }
    public int AdminTokenLifetimeSeconds { get; init; }
    public string TokenIssuer { get; init; } = string.Empty;
    public string TokenAudience { get; init; } = string.Empty;
    public int MaxLoginAttempts { get; init; }
    public int LockoutDurationSeconds { get; init; }
    public string StoreBasePath { get; init; } = string.Empty;
    public string EncryptionKeyPath { get; init; } = string.Empty;
    public string HmacKeyPath { get; init; } = string.Empty;
    public int AuditEventRetentionDays { get; init; }
    public int UserRecordRetentionDays { get; init; }
    public int IncidentRecordRetentionDays { get; init; }
    public bool SodApprovalRequiredForRoleChanges { get; init; } = true;

    public IReadOnlyList<string> Validate()
    {
        var issues = new List<string>();

        if (TokenLifetimeSeconds <= 0) issues.Add("TokenLifetimeSeconds must be > 0.");
        if (AdminTokenLifetimeSeconds <= 0 || AdminTokenLifetimeSeconds >= TokenLifetimeSeconds)
            issues.Add("AdminTokenLifetimeSeconds must be > 0 and < TokenLifetimeSeconds.");
        if (string.IsNullOrWhiteSpace(TokenIssuer)) issues.Add("TokenIssuer is required.");
        if (string.IsNullOrWhiteSpace(TokenAudience)) issues.Add("TokenAudience is required.");
        if (MaxLoginAttempts is < 3 or > 20) issues.Add("MaxLoginAttempts must be between 3 and 20.");
        if (LockoutDurationSeconds <= 0) issues.Add("LockoutDurationSeconds must be > 0.");
        if (string.IsNullOrWhiteSpace(StoreBasePath)) issues.Add("StoreBasePath is required.");
        if (string.IsNullOrWhiteSpace(EncryptionKeyPath)) issues.Add("EncryptionKeyPath is required.");
        if (string.IsNullOrWhiteSpace(HmacKeyPath)) issues.Add("HmacKeyPath is required.");
        if (AuditEventRetentionDays < 0) issues.Add("AuditEventRetentionDays must be >= 0.");
        if (UserRecordRetentionDays < 0) issues.Add("UserRecordRetentionDays must be >= 0.");
        if (IncidentRecordRetentionDays < 0) issues.Add("IncidentRecordRetentionDays must be >= 0.");

        return issues;
    }
}

public interface IPolicyConfigurationService
{
    PolicyConfiguration GetConfiguration();
}

public sealed class PolicyConfigurationService(PolicyConfiguration configuration) : IPolicyConfigurationService
{
    public PolicyConfiguration GetConfiguration() => configuration;
}

