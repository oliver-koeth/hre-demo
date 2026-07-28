using AuthModule.ServiceHost.Tests.Support;
using FluentAssertions;
using System.Net.Http.Json;

namespace AuthModule.ServiceHost.Tests.Api;

public sealed class AuthorizationTests(ServiceHostFactory factory) : IClassFixture<ServiceHostFactory>
{
    [Fact]
    public async Task Evaluate_WithValidRequest_ShouldReturn200_NotServerError()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/core-security/authz/evaluate", new
        {
            userId = Guid.NewGuid(),
            resource = "ledger.entry",
            action = "read",
        });

        // Without a matching permission the service returns a domain decision (200 deny),
        // or a structured 4xx — must never be 500
        ((int)response.StatusCode).Should().NotBe(500);
        response.Content.Headers.ContentType?.MediaType
            .Should().BeOneOf("application/json", "application/problem+json");
    }
}
