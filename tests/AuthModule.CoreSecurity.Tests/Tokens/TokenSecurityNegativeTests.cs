using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuthModule.CoreSecurity.Application.Contracts;
using AuthModule.CoreSecurity.Tests.Support;
using FluentAssertions;
using Microsoft.IdentityModel.Tokens;

namespace AuthModule.CoreSecurity.Tests.Tokens;

public sealed class TokenSecurityNegativeTests
{
    private const string SigningKey = "local-test-signing-key-local-test-sign";
    private const string Issuer = "auth-module";
    private const string Audience = "auth-module-clients";

    [Fact]
    [Trait("Security", "True")]
    public async Task Validate_ShouldFail_ForTamperedToken()
    {
        var sut = await TestContextFactory.CreateAsync();
        var login = await sut.AuthenticationService.LoginAsync(
            new LoginRequest("alice@example.com", "Password!123"),
            sut.RequestContext);

        var token = login.Value.AccessToken;
        var tampered = token[..^1] + (token[^1] == 'a' ? "b" : "a");

        var validation = await sut.TokenValidationService.ValidateAsync(
            new ValidateTokenRequest(tampered),
            sut.RequestContext);

        validation.IsFailure.Should().BeTrue();
    }

    [Fact]
    [Trait("Security", "True")]
    public async Task Validate_ShouldFail_ForExpiredToken()
    {
        var sut = await TestContextFactory.CreateAsync();
        var expired = BuildToken(
            userId: Guid.NewGuid(),
            sessionId: Guid.NewGuid(),
            tokenVersion: 0,
            issuer: Issuer,
            audience: Audience,
            expiresAtUtc: DateTime.UtcNow.AddMinutes(-1));

        var validation = await sut.TokenValidationService.ValidateAsync(
            new ValidateTokenRequest(expired),
            sut.RequestContext);

        validation.IsFailure.Should().BeTrue();
    }

    [Fact]
    [Trait("Security", "True")]
    public async Task Validate_ShouldFail_ForWrongIssuer()
    {
        var sut = await TestContextFactory.CreateAsync();
        var token = BuildToken(
            userId: Guid.NewGuid(),
            sessionId: Guid.NewGuid(),
            tokenVersion: 0,
            issuer: "untrusted-issuer",
            audience: Audience,
            expiresAtUtc: DateTime.UtcNow.AddMinutes(10));

        var validation = await sut.TokenValidationService.ValidateAsync(
            new ValidateTokenRequest(token),
            sut.RequestContext);

        validation.IsFailure.Should().BeTrue();
    }

    [Fact]
    [Trait("Security", "True")]
    public async Task Validate_ShouldFail_ForWrongAudience()
    {
        var sut = await TestContextFactory.CreateAsync();
        var token = BuildToken(
            userId: Guid.NewGuid(),
            sessionId: Guid.NewGuid(),
            tokenVersion: 0,
            issuer: Issuer,
            audience: "wrong-audience",
            expiresAtUtc: DateTime.UtcNow.AddMinutes(10));

        var validation = await sut.TokenValidationService.ValidateAsync(
            new ValidateTokenRequest(token),
            sut.RequestContext);

        validation.IsFailure.Should().BeTrue();
    }

    [Fact]
    [Trait("Security", "True")]
    public async Task Validate_ShouldFail_WhenRequiredClaimsMissing()
    {
        var sut = await TestContextFactory.CreateAsync();
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SigningKey));
        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, Guid.NewGuid().ToString()),
            }),
            Issuer = Issuer,
            Audience = Audience,
            Expires = DateTime.UtcNow.AddMinutes(10),
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256),
        };
        var handler = new JwtSecurityTokenHandler();
        var token = handler.WriteToken(handler.CreateToken(descriptor));

        var validation = await sut.TokenValidationService.ValidateAsync(
            new ValidateTokenRequest(token),
            sut.RequestContext);

        validation.IsFailure.Should().BeTrue();
    }

    private static string BuildToken(
        Guid userId,
        Guid sessionId,
        int tokenVersion,
        string issuer,
        string audience,
        DateTime expiresAtUtc)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SigningKey));
        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim("sid", sessionId.ToString()),
                new Claim("tv", tokenVersion.ToString()),
                new Claim("privileged", "0"),
                new Claim("permissions", string.Empty),
            }),
            Issuer = issuer,
            Audience = audience,
            NotBefore = DateTime.UtcNow.AddHours(-1),
            IssuedAt = DateTime.UtcNow.AddHours(-1),
            Expires = expiresAtUtc,
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256),
        };

        var handler = new JwtSecurityTokenHandler();
        return handler.WriteToken(handler.CreateToken(descriptor));
    }
}
