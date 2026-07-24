using AuthModule.Foundation.Domain.Entities;
using AuthModule.Foundation.Security;
using FsCheck;
using FsCheck.Xunit;

namespace AuthModule.Foundation.Tests.PropertyBased;

public sealed class FoundationPropertyTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), $"pbt-tests-{Guid.NewGuid():N}");
    private readonly IEncryptionService _encryption;
    private readonly IIntegrityService _integrity;

    public FoundationPropertyTests()
    {
        var secrets = Persistence.TestSecrets.Create(_root);
        var keyProvider = new KeyProvider(secrets.EncryptionKeyPath, secrets.HmacKeyPath);
        _encryption = new EncryptionService(keyProvider);
        _integrity = new IntegrityService(keyProvider);
    }

    [Property(MaxTest = 100)]
    public bool EncryptionRoundTrip_Returns_Original(byte[] input)
    {
        var payload = input ?? [];
        var encrypted = _encryption.Encrypt(payload);
        var decrypted = _encryption.Decrypt(encrypted);
        return payload.SequenceEqual(decrypted);
    }

    [Property(MaxTest = 100)]
    public bool IntegrityVerification_Is_Idempotent(byte[] input)
    {
        var payload = input ?? [];
        var signature = _integrity.ComputeSignature(payload);
        var first = _integrity.Verify(payload, signature);
        var second = _integrity.Verify(payload, signature);
        return first && second;
    }

    [Property(MaxTest = 100)]
    public bool RoleAssignmentWindow_Validity_Rule_Holds(DateTimeOffset a, DateTimeOffset b)
    {
        var assignment = new RolePermissionAssignment { ValidFrom = a, ValidUntil = b };
        var expected = a <= b;
        return assignment.HasValidWindow() == expected;
    }

    [Property(MaxTest = 100)]
    public bool MigrationOperation_Is_Idempotent(NonNull<string> content)
    {
        static string Migrate(string text) => text.Contains("\"schemaVersion\":\"1.0\"", StringComparison.Ordinal)
            ? text
            : text + "\"schemaVersion\":\"1.0\"";

        var once = Migrate(content.Get);
        var twice = Migrate(once);
        return once == twice;
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, true);
        }
    }
}
