using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AuthModule.CoreSecurity.Persistence;
using AuthModule.Foundation.Domain.Entities;
using AuthModule.Foundation.Domain.Primitives;
using AuthModule.Foundation.Persistence.Contracts;
using AuthModule.ServiceHost.Tests.Support;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace AuthModule.ServiceHost.Tests.Api;

public sealed class UserSearchTests(ServiceHostFactory factory) : IClassFixture<ServiceHostFactory>
{
    private static readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web);

    [Fact]
    [Trait("Security", "True")]
    public async Task SearchUsers_WithoutActorHeader_ShouldReturn401()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/core-security/users/search?q=test");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("Security", "True")]
    public async Task SearchUsers_WithInvalidActorHeader_ShouldReturn401()
    {
        var client = factory.CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/core-security/users/search?q=test");
        request.Headers.Add("X-Actor-UserId", "not-a-guid");

        var response = await client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("Security", "True")]
    public async Task SearchUsers_ByUnauthorizedActor_ShouldReturn403()
    {
        var client = factory.CreateClient();
        var actor = await CreateActorAsync(client);

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/core-security/users/search?q=test");
        request.Headers.Add("X-Actor-UserId", actor.UserId.ToString());

        var response = await client.SendAsync(request);
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task SearchUsers_WithShortQuery_ShouldReturn400()
    {
        var client = factory.CreateClient();
        var actor = await SeedAuthorizedActorAsync(client);

        var response = await SearchAsync(client, actor.UserId, "a");
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task SearchUsers_WithLongQuery_ShouldReturn400()
    {
        var client = factory.CreateClient();
        var actor = await SeedAuthorizedActorAsync(client);

        var response = await SearchAsync(client, actor.UserId, new string('x', 101));
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task SearchUsers_BySubstring_ShouldReturnMatchingUsersSorted()
    {
        var client = factory.CreateClient();
        var actor = await SeedAuthorizedActorAsync(client);

        await CreateUserAsync(client, "alpha-one", "Alpha One");
        await CreateUserAsync(client, "alpha-two", "Alpha Two");
        await CreateUserAsync(client, "beta-one", "Beta One");

        var response = await SearchAsync(client, actor.UserId, "alpha");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(body);
        var results = doc.RootElement.GetProperty("results");
        results.GetArrayLength().Should().Be(2);

        var names = results.EnumerateArray()
            .Select(r => r.GetProperty("displayName").GetString())
            .ToList();
        names.Should().Equal("Alpha One", "Alpha Two");
    }

    [Fact]
    public async Task SearchUsers_IsCaseInsensitive()
    {
        var client = factory.CreateClient();
        var actor = await SeedAuthorizedActorAsync(client);
        await CreateUserAsync(client, "ci-user", "MixedCase Name");

        var response = await SearchAsync(client, actor.UserId, "mixedcase");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(body);
        doc.RootElement.GetProperty("totalCount").GetInt32().Should().Be(1);
    }

    [Fact]
    public async Task SearchUsers_Pagination_ShouldReturnRequestedPage()
    {
        var client = factory.CreateClient();
        var actor = await SeedAuthorizedActorAsync(client);

        for (var i = 1; i <= 5; i++)
        {
            await CreateUserAsync(client, $"page-user-{i}", $"Page User {i}");
        }

        var response = await SearchAsync(client, actor.UserId, "Page", page: 2, pageSize: 2);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(body);
        doc.RootElement.GetProperty("totalCount").GetInt32().Should().Be(5);
        doc.RootElement.GetProperty("page").GetInt32().Should().Be(2);
        doc.RootElement.GetProperty("pageSize").GetInt32().Should().Be(2);
        doc.RootElement.GetProperty("results").GetArrayLength().Should().Be(2);
    }

    [Fact]
    public async Task SearchUsers_NoMatches_ShouldReturnEmptyResults()
    {
        var client = factory.CreateClient();
        var actor = await SeedAuthorizedActorAsync(client);
        await CreateUserAsync(client, "nomatch", "No Match");

        var response = await SearchAsync(client, actor.UserId, "zzzz");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(body);
        doc.RootElement.GetProperty("totalCount").GetInt32().Should().Be(0);
        doc.RootElement.GetProperty("results").GetArrayLength().Should().Be(0);
    }

    [Fact]
    public async Task SearchUsers_IncludesDisabledUsers_WithStatus()
    {
        var client = factory.CreateClient();
        var actor = await SeedAuthorizedActorAsync(client);
        var user = await CreateUserAsync(client, "disabled-user", "Disabled User Name");

        var disableResp = await client.PostAsJsonAsync($"/api/core-security/users/{user.UserId}/disable", new
        {
            userId = user.UserId,
            reason = "Test disable",
        });
        disableResp.StatusCode.Should().Be(HttpStatusCode.OK);

        var response = await SearchAsync(client, actor.UserId, "disabled");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(body);
        doc.RootElement.GetProperty("totalCount").GetInt32().Should().Be(1);
        var result = doc.RootElement.GetProperty("results")[0];
        result.GetProperty("status").GetString().Should().Be("Inactive");
    }

    private async Task<HttpResponseMessage> SearchAsync(HttpClient client, Guid actorId, string query, int? page = null, int? pageSize = null)
    {
        var url = $"/api/core-security/users/search?q={Uri.EscapeDataString(query)}";
        if (page.HasValue)
        {
            url += $"&page={page.Value}";
        }
        if (pageSize.HasValue)
        {
            url += $"&pageSize={pageSize.Value}";
        }
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("X-Actor-UserId", actorId.ToString());
        return await client.SendAsync(request);
    }

    private async Task<UserDto> CreateActorAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/core-security/users", new
        {
            username = $"actor-{Guid.NewGuid():N}",
            email = $"actor-{Guid.NewGuid():N}@example.com",
            displayName = "Test Actor",
            createdBy = Guid.NewGuid(),
        });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(body);
        return new UserDto(doc.RootElement.GetProperty("userId").GetGuid());
    }

    private async Task<UserDto> SeedAuthorizedActorAsync(HttpClient client)
    {
        var actor = await CreateActorAsync(client);

        var permissionRepo = factory.Services.GetRequiredService<IStoreRepository<Permission>>();
        var roleRepo = factory.Services.GetRequiredService<IStoreRepository<Role>>();
        var stateStore = factory.Services.GetRequiredService<ICoreSecurityStateStore>();
        var context = new RequestContext(Guid.NewGuid(), actor.UserId, null, DateTimeOffset.UtcNow, null);

        var permission = new Permission
        {
            PermissionId = Guid.NewGuid(),
            Name = "User Search",
            Resource = "users",
            Action = "search",
            Description = "Allows searching user records by display name.",
            IsSystem = true,
            CreatedAt = context.Timestamp,
            UpdatedAt = context.Timestamp,
            CreatedBy = actor.UserId,
        };
        var savedPermission = await permissionRepo.SaveAsync(permission, expectedVersion: null, context);
        savedPermission.IsSuccess.Should().BeTrue();

        var role = new Role
        {
            RoleId = Guid.NewGuid(),
            Name = "User Reader",
            Description = "Can search and read user records.",
            IsSystem = true,
            CreatedAt = context.Timestamp,
            UpdatedAt = context.Timestamp,
            CreatedBy = actor.UserId,
        };
        var savedRole = await roleRepo.SaveAsync(role, expectedVersion: null, context);
        savedRole.IsSuccess.Should().BeTrue();

        stateStore.UpsertUserRoleAssignment(new UserRoleAssignment
        {
            AssignmentId = Guid.NewGuid(),
            UserId = actor.UserId,
            RoleId = role.RoleId,
            CreatedAt = context.Timestamp,
            CreatedBy = actor.UserId,
        });

        stateStore.UpsertRolePermissionAssignment(new RolePermissionAssignment
        {
            AssignmentId = Guid.NewGuid(),
            RoleId = role.RoleId,
            PermissionId = permission.PermissionId,
            CreatedAt = context.Timestamp,
            CreatedBy = actor.UserId,
            ValidFrom = context.Timestamp,
        });

        return actor;
    }

    private async Task<UserDto> CreateUserAsync(HttpClient client, string usernamePrefix, string displayName)
    {
        var response = await client.PostAsJsonAsync("/api/core-security/users", new
        {
            username = $"{usernamePrefix}-{Guid.NewGuid():N}",
            email = $"{usernamePrefix}-{Guid.NewGuid():N}@example.com",
            displayName,
            createdBy = Guid.NewGuid(),
        });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(body);
        return new UserDto(doc.RootElement.GetProperty("userId").GetGuid());
    }

    private sealed record UserDto(Guid UserId);
}
