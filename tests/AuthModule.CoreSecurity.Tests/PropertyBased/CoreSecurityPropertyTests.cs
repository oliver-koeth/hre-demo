using AuthModule.CoreSecurity.Application.Contracts;
using AuthModule.CoreSecurity.Domain;
using AuthModule.CoreSecurity.Tests.Support;
using FsCheck;
using FsCheck.Xunit;

namespace AuthModule.CoreSecurity.Tests.PropertyBased;

public sealed class CoreSecurityPropertyTests
{
    [Property]
    public bool EmailNormalization_ShouldBeIdempotent(NonNull<string> raw)
    {
        var once = Normalization.NormalizeEmail(raw.Get);
        var twice = Normalization.NormalizeEmail(once);
        return once == twice;
    }

    [Fact]
    public async Task DisableUser_ShouldBeIdempotent()
    {
        var sut = await TestContextFactory.CreateAsync();
        var user = (await sut.Users.SearchAsync(new AuthModule.Foundation.Persistence.Contracts.StoreSearchQuery<AuthModule.Foundation.Domain.Entities.User>(_ => true), sut.RequestContext)).Value.Single();

        var first = await sut.UserAdministrationService.DisableUserAsync(new DisableUserRequest(user.UserId, "first"), sut.RequestContext);
        var second = await sut.UserAdministrationService.DisableUserAsync(new DisableUserRequest(user.UserId, "second"), sut.RequestContext);

        Assert.True(first.IsSuccess);
        Assert.True(second.IsSuccess);
    }

    [Property]
    public bool PermissionKeyNormalization_ShouldBeDeterministic(NonNull<string> resource, NonNull<string> action)
    {
        var k1 = Normalization.BuildPermissionKey(resource.Get, action.Get);
        var k2 = Normalization.BuildPermissionKey(resource.Get, action.Get);
        return k1 == k2;
    }
}
