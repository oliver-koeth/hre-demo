using AuthModule.ServiceHost.Tests.Support;
using FluentAssertions;
using System.Net.Http.Json;
using System.Text.Json;

namespace AuthModule.ServiceHost.Tests.Api;

public sealed class MfaTests(ServiceHostFactory factory) : IClassFixture<ServiceHostFactory>
{
    [Fact]
    public async Task CreateChallenge_ShouldReturn200_WithChallengeId()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/core-security/mfa/challenges", new
        {
            userId = Guid.NewGuid(),
            sessionId = Guid.NewGuid(),
            operationKey = "high-value-transfer",
        });

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(body);
        doc.RootElement.TryGetProperty("challengeId", out _).Should().BeTrue();
    }

    [Fact]
    public async Task VerifyChallenge_WithWrongCode_ShouldReturn4xx_NotServerError()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/core-security/mfa/verify", new
        {
            challengeId = Guid.NewGuid(),
            verificationCode = "000000",
        });

        ((int)response.StatusCode).Should().BeInRange(400, 499);
    }
}
