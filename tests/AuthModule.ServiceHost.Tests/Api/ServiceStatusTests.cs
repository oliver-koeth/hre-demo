using AuthModule.ServiceHost.Tests.Support;
using FluentAssertions;

namespace AuthModule.ServiceHost.Tests.Api;

public sealed class ServiceStatusTests(ServiceHostFactory factory) : IClassFixture<ServiceHostFactory>
{
    [Fact]
    public async Task Root_ShouldReturn200_WithRunningStatus()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("auth-module");
        body.Should().Contain("Running");
    }
}
