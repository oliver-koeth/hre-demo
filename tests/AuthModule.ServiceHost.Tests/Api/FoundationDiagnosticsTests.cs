using AuthModule.ServiceHost.Tests.Support;
using FluentAssertions;

namespace AuthModule.ServiceHost.Tests.Api;

public sealed class FoundationDiagnosticsTests(ServiceHostFactory factory) : IClassFixture<ServiceHostFactory>
{
    [Fact]
    public async Task HealthEndpoint_ShouldReturn200_WithHealthyStatus()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/internal/foundation/health");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("Healthy");
    }

    [Fact]
    public async Task IntegrityEndpoint_ShouldReturn200()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/internal/foundation/integrity");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }
}
