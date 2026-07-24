using AuthModule.Foundation.Domain.Entities;
using AuthModule.Foundation.Domain.Primitives;
using AuthModule.Foundation.Persistence.Contracts;
using AuthModule.Foundation.Persistence.Repositories;
using AuthModule.Foundation.Security;
using FluentAssertions;

namespace AuthModule.Foundation.Tests.Persistence;

public sealed class RepositoryBehaviorTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), $"foundation-tests-{Guid.NewGuid():N}");
    private readonly JsonStoreRepository<User> _repo;

    public RepositoryBehaviorTests()
    {
        var secrets = TestSecrets.Create(_root);
        var keyProvider = new KeyProvider(secrets.EncryptionKeyPath, secrets.HmacKeyPath);
        var encryption = new EncryptionService(keyProvider);
        var integrity = new IntegrityService(keyProvider);
        _repo = new JsonStoreRepository<User>(
            Path.Combine(_root, "auth-store", "users.json"),
            "AuthStore/Users",
            encryption,
            integrity);
    }

    [Fact]
    public async Task Save_Then_Get_RoundTrip_Should_Return_Entity()
    {
        var context = RequestContext.CreateAnonymous();
        var user = NewUser();

        var saved = await _repo.SaveAsync(user, expectedVersion: null, context);
        saved.IsSuccess.Should().BeTrue();

        var fetched = await _repo.GetAsync(new StoreQuery(user.UserId), context);
        fetched.IsSuccess.Should().BeTrue();
        fetched.Value.Should().NotBeNull();
        fetched.Value!.Username.Should().Be(user.Username);
    }

    [Fact]
    public async Task Save_With_Stale_Version_Should_Return_Conflict()
    {
        var context = RequestContext.CreateAnonymous();
        var user = NewUser();
        await _repo.SaveAsync(user, expectedVersion: null, context);

        user.DisplayName = "Updated";
        var firstUpdate = await _repo.SaveAsync(user, expectedVersion: 0, context);
        firstUpdate.IsSuccess.Should().BeTrue();

        user.DisplayName = "Stale update";
        var stale = await _repo.SaveAsync(user, expectedVersion: 0, context);
        stale.IsFailure.Should().BeTrue();
        stale.Error.Code.Should().Be(DomainErrorCode.Conflict);
    }

    [Fact]
    public async Task SoftDelete_Should_Mark_Record_And_Exclude_From_Default_Search()
    {
        var context = RequestContext.CreateAnonymous();
        var user = NewUser();
        await _repo.SaveAsync(user, expectedVersion: null, context);

        var deleted = await _repo.SoftDeleteAsync(user.UserId, expectedVersion: 0, context);
        deleted.IsSuccess.Should().BeTrue();
        deleted.Value.IsDeleted.Should().BeTrue();

        var normalSearch = await _repo.SearchAsync(new StoreSearchQuery<User>(_ => true), context);
        normalSearch.Value.Should().BeEmpty();

        var includeDeleted = await _repo.SearchAsync(new StoreSearchQuery<User>(_ => true, IncludeDeleted: true), context);
        includeDeleted.Value.Should().HaveCount(1);
    }

    [Fact]
    public async Task Missing_Signature_Should_Fail_Integrity_Check()
    {
        var context = RequestContext.CreateAnonymous();
        var user = NewUser();
        await _repo.SaveAsync(user, expectedVersion: null, context);
        File.Delete(Path.Combine(_root, "auth-store", "users.json.sig"));

        var fetched = await _repo.GetAsync(new StoreQuery(user.UserId), context);
        fetched.IsFailure.Should().BeTrue();
        fetched.Error.Code.Should().Be(DomainErrorCode.IntegrityViolation);
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, recursive: true);
        }
    }

    private static User NewUser() => new()
    {
        UserId = Guid.NewGuid(),
        Username = $"user-{Guid.NewGuid():N}",
        Email = $"user-{Guid.NewGuid():N}@example.com",
        DisplayName = "User",
        Status = UserStatus.Active,
        CreatedAt = DateTimeOffset.UtcNow,
        UpdatedAt = DateTimeOffset.UtcNow,
        CreatedBy = Guid.NewGuid(),
    };
}

