using AuthModule.Foundation.Domain.Primitives;
using AuthModule.Foundation.Persistence.Contracts;
using AuthModule.Foundation.Persistence.Repositories;
using AuthModule.Foundation.Security;
using FluentAssertions;

namespace AuthModule.Foundation.Tests.Persistence;

public sealed class StoreIntegrityServiceTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), $"integrity-tests-{Guid.NewGuid():N}");
    private readonly string _filePath;
    private readonly IEncryptionService _encryption;
    private readonly IIntegrityService _integrity;

    public StoreIntegrityServiceTests()
    {
        var secrets = TestSecrets.Create(_root);
        var keyProvider = new KeyProvider(secrets.EncryptionKeyPath, secrets.HmacKeyPath);
        _encryption = new EncryptionService(keyProvider);
        _integrity = new IntegrityService(keyProvider);
        _filePath = Path.Combine(_root, "auth-store", "users.json");
        Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
    }

    [Fact]
    public async Task VerifyAllStores_Should_Pass_For_Valid_File_And_Signature()
    {
        await File.WriteAllBytesAsync(_filePath, _encryption.Encrypt("{}".Select(c => (byte)c).ToArray()));
        await File.WriteAllBytesAsync($"{_filePath}.sig", _integrity.ComputeSignature(await File.ReadAllBytesAsync(_filePath)));

        var service = new StoreIntegrityService(_integrity, [_filePath]);
        var result = await service.VerifyAllStoresAsync(RequestContext.CreateAnonymous());

        result.AllPassed.Should().BeTrue();
        result.FileResults[_filePath].Should().Be(IntegrityCheckOutcome.Pass);
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, true);
        }
    }
}
