using AuthModule.ServiceHost.Tests.Support;
using FluentAssertions;
using System.Net.Http.Json;
using System.Text.Json;

namespace AuthModule.ServiceHost.Tests.Api;

public sealed class OpenApiTests(ServiceHostFactory factory) : IClassFixture<ServiceHostFactory>
{
    [Fact]
    public async Task OpenApiJson_ShouldReturn200_WithJsonContentType()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/openapi/v1.json");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
        var body = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(body);
        doc.RootElement.GetProperty("info").GetProperty("title").GetString()
            .Should().Contain("Authentication Module");
    }

    [Fact]
    public async Task DocsEndpoint_ShouldReturn200_WithHtmlContentType()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/docs");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("text/html");
    }

    [Fact]
    public async Task PreflightRequest_ShouldInclude_CorsHeader()
    {
        var client = factory.CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Options, "/api/core-security/auth/login");
        request.Headers.Add("Origin", "http://localhost:3000");
        request.Headers.Add("Access-Control-Request-Method", "POST");

        var response = await client.SendAsync(request);

        response.Headers.Contains("Access-Control-Allow-Origin").Should().BeTrue();
    }
}
