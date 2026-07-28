using AuthModule.ServiceHost.Tests.Support;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace AuthModule.ServiceHost.Tests.Api;

public sealed class UserAdminTests(ServiceHostFactory factory) : IClassFixture<ServiceHostFactory>
{
    private static readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task CreateUser_ShouldReturn200_WithUserId()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/core-security/users", new
        {
            username = $"user-{Guid.NewGuid():N}",
            email = $"user-{Guid.NewGuid():N}@example.com",
            displayName = "Test User",
            createdBy = Guid.NewGuid(),
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(body);
        doc.RootElement.TryGetProperty("userId", out _).Should().BeTrue();
    }

    [Fact]
    public async Task UpdateUser_ShouldReturn200()
    {
        var client = factory.CreateClient();

        // First create the user
        var createResp = await client.PostAsJsonAsync("/api/core-security/users", new
        {
            username = $"user-{Guid.NewGuid():N}",
            email = $"user-{Guid.NewGuid():N}@example.com",
            displayName = "Original Name",
            createdBy = Guid.NewGuid(),
        });
        createResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await createResp.Content.ReadAsStringAsync();
        var userId = JsonDocument.Parse(body).RootElement.GetProperty("userId").GetGuid();

        var updateResp = await client.PutAsJsonAsync($"/api/core-security/users/{userId}", new
        {
            userId,
            displayName = "Updated Name",
        });

        updateResp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task DisableUser_ShouldReturn200()
    {
        var client = factory.CreateClient();

        // Create
        var createResp = await client.PostAsJsonAsync("/api/core-security/users", new
        {
            username = $"user-{Guid.NewGuid():N}",
            email = $"user-{Guid.NewGuid():N}@example.com",
            displayName = "To Be Disabled",
            createdBy = Guid.NewGuid(),
        });
        var userId = JsonDocument.Parse(await createResp.Content.ReadAsStringAsync())
            .RootElement.GetProperty("userId").GetGuid();

        var disableResp = await client.PostAsJsonAsync(
            $"/api/core-security/users/{userId}/disable",
            new { userId, reason = "Integration test teardown" });

        disableResp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task DisableUser_TwiceOnSameUser_ShouldBeIdempotent()
    {
        var client = factory.CreateClient();

        var createResp = await client.PostAsJsonAsync("/api/core-security/users", new
        {
            username = $"user-{Guid.NewGuid():N}",
            email = $"user-{Guid.NewGuid():N}@example.com",
            displayName = "Idempotent Disable",
            createdBy = Guid.NewGuid(),
        });
        var userId = JsonDocument.Parse(await createResp.Content.ReadAsStringAsync())
            .RootElement.GetProperty("userId").GetGuid();

        var first = await client.PostAsJsonAsync(
            $"/api/core-security/users/{userId}/disable",
            new { userId, reason = "first" });
        var second = await client.PostAsJsonAsync(
            $"/api/core-security/users/{userId}/disable",
            new { userId, reason = "second" });

        first.StatusCode.Should().Be(HttpStatusCode.OK);
        second.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
