namespace AuthModule.CoreSecurity.Configuration;

public sealed class CoreSecurityConfiguration
{
    public int MaxLoginAttempts { get; init; } = 5;
    public int LockoutDurationSeconds { get; init; } = 900;
    public int StepUpChallengeTtlSeconds { get; init; } = 300;
    public int TokenValidationCacheSeconds { get; init; } = 15;
    public int ApprovalRetryCount { get; init; } = 2;
    public string TokenSigningKey { get; init; } = "dev-signing-key-change-me";

    public IReadOnlyList<string> Validate()
    {
        var issues = new List<string>();
        if (MaxLoginAttempts < 1) issues.Add("MaxLoginAttempts must be >= 1.");
        if (LockoutDurationSeconds < 1) issues.Add("LockoutDurationSeconds must be >= 1.");
        if (StepUpChallengeTtlSeconds < 1) issues.Add("StepUpChallengeTtlSeconds must be >= 1.");
        if (TokenValidationCacheSeconds < 1) issues.Add("TokenValidationCacheSeconds must be >= 1.");
        if (ApprovalRetryCount < 0) issues.Add("ApprovalRetryCount must be >= 0.");
        if (string.IsNullOrWhiteSpace(TokenSigningKey)) issues.Add("TokenSigningKey is required.");
        return issues;
    }
}
