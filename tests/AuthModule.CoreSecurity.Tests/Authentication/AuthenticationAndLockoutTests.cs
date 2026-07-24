using AuthModule.CoreSecurity.Application.Contracts;
using AuthModule.CoreSecurity.Domain;
using AuthModule.CoreSecurity.Tests.Support;
using FluentAssertions;

namespace AuthModule.CoreSecurity.Tests.Authentication;

public sealed class AuthenticationAndLockoutTests
{
    [Fact]
    public async Task Login_ShouldIssueToken_ForValidCredentials()
    {
        var sut = await TestContextFactory.CreateAsync();
        var result = await sut.AuthenticationService.LoginAsync(
            new LoginRequest("alice@example.com", "Password!123"),
            sut.RequestContext);

        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Login_ShouldLockAccount_AfterThresholdFailures()
    {
        var sut = await TestContextFactory.CreateAsync();
        var request = new LoginRequest("alice@example.com", "wrong");

        await sut.AuthenticationService.LoginAsync(request, sut.RequestContext);
        await sut.AuthenticationService.LoginAsync(request, sut.RequestContext);
        var finalResult = await sut.AuthenticationService.LoginAsync(request, sut.RequestContext);

        finalResult.IsFailure.Should().BeTrue();
        var attempt = sut.StateStore.GetOrCreateAttempt(Normalization.NormalizeEmail("alice@example.com"));
        attempt.LockedUntil.Should().NotBeNull();
    }
}
