using AuthModule.CoreSecurity.Application.Contracts;
using AuthModule.CoreSecurity.Tests.Support;
using FluentAssertions;

namespace AuthModule.CoreSecurity.Tests.Tokens;

public sealed class TokenValidationTests
{
    [Fact]
    public async Task Validate_ShouldSucceed_ForFreshIssuedToken()
    {
        var sut = await TestContextFactory.CreateAsync();
        var login = await sut.AuthenticationService.LoginAsync(
            new LoginRequest("alice@example.com", "Password!123"),
            sut.RequestContext);

        var validation = await sut.TokenValidationService.ValidateAsync(
            new ValidateTokenRequest(login.Value.AccessToken),
            sut.RequestContext);

        validation.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenTokenVersionIsRevoked()
    {
        var sut = await TestContextFactory.CreateAsync();
        var login = await sut.AuthenticationService.LoginAsync(
            new LoginRequest("alice@example.com", "Password!123"),
            sut.RequestContext);

        var user = (await sut.Users.SearchAsync(new AuthModule.Foundation.Persistence.Contracts.StoreSearchQuery<AuthModule.Foundation.Domain.Entities.User>(_ => true), sut.RequestContext)).Value.Single();
        await sut.UserAdministrationService.DisableUserAsync(new DisableUserRequest(user.UserId, "admin action"), sut.RequestContext);

        var validation = await sut.TokenValidationService.ValidateAsync(
            new ValidateTokenRequest(login.Value.AccessToken),
            sut.RequestContext);

        validation.IsFailure.Should().BeTrue();
    }
}
