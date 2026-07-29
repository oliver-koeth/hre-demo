using System.Security.Cryptography;
using System.Text.Json;

namespace AuthModule.ServiceHost.Tests.Support;

/// <summary>
/// Creates an isolated temp directory with generated secrets and a policy config file for each test run.
/// </summary>
public sealed class TestPolicyConfig : IDisposable
{
    public string Root { get; }
    public string ConfigPath { get; }
    public string EncryptionKeyPath { get; }
    public string HmacKeyPath { get; }
    public string DataPath { get; }

    public TestPolicyConfig()
    {
        Root = Path.Combine(Path.GetTempPath(), $"svc-tests-{Guid.NewGuid():N}");
        var secretsDir = Path.Combine(Root, "secrets");
        DataPath = Path.Combine(Root, "data");
        Directory.CreateDirectory(secretsDir);
        Directory.CreateDirectory(DataPath);

        EncryptionKeyPath = Path.Combine(secretsDir, "encryption-key");
        HmacKeyPath = Path.Combine(secretsDir, "hmac-key");
        File.WriteAllText(EncryptionKeyPath, Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)));
        File.WriteAllText(HmacKeyPath, Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)));

        ConfigPath = Path.Combine(Root, "policy.json");
        var doc = new
        {
            tokenLifetimeSeconds = 3600,
            adminTokenLifetimeSeconds = 900,
            tokenIssuer = "auth-module",
            tokenAudience = "auth-module-clients",
            maxLoginAttempts = 3,
            lockoutDurationSeconds = 60,
            storeBasePath = DataPath,
            encryptionKeyPath = EncryptionKeyPath,
            hmacKeyPath = HmacKeyPath,
            auditEventRetentionDays = 365,
            userRecordRetentionDays = 3650,
            incidentRecordRetentionDays = 2555,
            sodApprovalRequiredForRoleChanges = true,
            coreSecurity = new
            {
                maxLoginAttempts = 3,
                lockoutDurationSeconds = 60,
                stepUpChallengeTtlSeconds = 300,
                tokenValidationCacheSeconds = 5,
                approvalRetryCount = 2,
                tokenSigningKey = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)),
            },
            governance = new
            {
                defaultQueryPageSize = 100,
                maxQueryPageSize = 500,
                exportChunkSize = 500,
                backupMetadataFreshnessSeconds = 30,
                evidenceRetentionMinimumDays = 1,
                previewRuntimeApproved = true,
            },
            integration = new
            {
                repositoryRootPath = Root,
                gateDecisionTimeoutSeconds = 15,
                gateEvidenceRetentionDays = 90,
                previewRuntimeApproved = true,
            },
        };
        File.WriteAllText(ConfigPath, JsonSerializer.Serialize(doc));
    }

    public void Dispose()
    {
        try { Directory.Delete(Root, recursive: true); }
        catch { /* best-effort cleanup */ }
    }
}
