using System.Diagnostics;
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

namespace AuthModule.ServiceHost.Tests.Performance;

public sealed class MicroLatencyPerformanceTests(ServiceHostFactory factory) : IClassFixture<ServiceHostFactory>
{
    [Fact]
    [Trait("PerfType", "Micro")]
    public async Task NFR_PERF_001_AuthorizationEvaluate_ShouldStayWithinLocalLatencyBudget()
    {
        var client = factory.CreateClient();
        var request = new
        {
            userId = Guid.NewGuid(),
            resource = "policy",
            action = "read",
        };

        for (var i = 0; i < 5; i++)
        {
            await client.PostAsJsonAsync("/api/core-security/authz/evaluate", request);
        }

        var beforeGen0 = GC.CollectionCount(0);
        var beforeGen1 = GC.CollectionCount(1);
        var beforeGen2 = GC.CollectionCount(2);

        var failed = 0;
        var latencies = new List<double>(capacity: 40);
        var runStopwatch = Stopwatch.StartNew();
        for (var i = 0; i < 40; i++)
        {
            var sw = Stopwatch.StartNew();
            var response = await client.PostAsJsonAsync("/api/core-security/authz/evaluate", request);
            sw.Stop();

            latencies.Add(sw.Elapsed.TotalMilliseconds);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                failed++;
            }
        }
        runStopwatch.Stop();

        var telemetry = PerformanceTelemetryWriter.Build(
            requirementId: "NFR-PERF-001",
            scenario: "Authorization evaluate micro-latency",
            endpoint: "POST /api/core-security/authz/evaluate",
            latenciesMs: latencies,
            failedRequests: failed,
            concurrency: 1,
            elapsed: runStopwatch.Elapsed,
            beforeGen0: beforeGen0,
            beforeGen1: beforeGen1,
            beforeGen2: beforeGen2);
        PerformanceTelemetryWriter.Write("nfr-perf-001-micro-latency", telemetry);

        telemetry.FailedRequests.Should().Be(0);
        telemetry.P95Ms.Should().BeLessThan(1000);
    }

    [Fact]
    [Trait("PerfType", "Micro")]
    public async Task NFR_U02_018_UserSearch_ShouldStayWithinLocalLatencyBudget()
    {
        var client = factory.CreateClient();
        var actor = await SeedAuthorizedSearcherAsync(client);

        for (var i = 0; i < 10; i++)
        {
            await CreateUserAsync(client, $"perf-user-{i}", $"Performance User {i}");
        }

        for (var i = 0; i < 5; i++)
        {
            await SearchAsync(client, actor, "Performance");
        }

        var beforeGen0 = GC.CollectionCount(0);
        var beforeGen1 = GC.CollectionCount(1);
        var beforeGen2 = GC.CollectionCount(2);

        var failed = 0;
        var latencies = new List<double>(capacity: 40);
        var runStopwatch = Stopwatch.StartNew();
        for (var i = 0; i < 40; i++)
        {
            var sw = Stopwatch.StartNew();
            var response = await SearchAsync(client, actor, "Performance");
            sw.Stop();

            latencies.Add(sw.Elapsed.TotalMilliseconds);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                failed++;
            }
        }
        runStopwatch.Stop();

        var telemetry = PerformanceTelemetryWriter.Build(
            requirementId: "NFR-U02-018",
            scenario: "User search micro-latency",
            endpoint: "GET /api/core-security/users/search",
            latenciesMs: latencies,
            failedRequests: failed,
            concurrency: 1,
            elapsed: runStopwatch.Elapsed,
            beforeGen0: beforeGen0,
            beforeGen1: beforeGen1,
            beforeGen2: beforeGen2);
        PerformanceTelemetryWriter.Write("nfr-u02-018-user-search-micro-latency", telemetry);

        telemetry.FailedRequests.Should().Be(0);
        telemetry.P99Ms.Should().BeLessThan(100);
    }

    private async Task<Guid> SeedAuthorizedSearcherAsync(HttpClient client)
    {
        var actorResponse = await client.PostAsJsonAsync("/api/core-security/users", new
        {
            username = $"perf-searcher-{Guid.NewGuid():N}",
            email = $"perf-searcher-{Guid.NewGuid():N}@example.com",
            displayName = "Perf Searcher",
            createdBy = Guid.NewGuid(),
        });
        actorResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var actorBody = await actorResponse.Content.ReadAsStringAsync();
        var actorId = JsonDocument.Parse(actorBody).RootElement.GetProperty("userId").GetGuid();

        var permissionRepo = factory.Services.GetRequiredService<IStoreRepository<Permission>>();
        var roleRepo = factory.Services.GetRequiredService<IStoreRepository<Role>>();
        var stateStore = factory.Services.GetRequiredService<ICoreSecurityStateStore>();
        var context = new RequestContext(Guid.NewGuid(), actorId, null, DateTimeOffset.UtcNow, null);

        var permission = new Permission
        {
            PermissionId = Guid.NewGuid(),
            Name = "users:search",
            Resource = "users",
            Action = "search",
            Description = "search users",
            IsSystem = true,
            CreatedAt = context.Timestamp,
            UpdatedAt = context.Timestamp,
            CreatedBy = actorId,
        };
        var savedPermission = await permissionRepo.SaveAsync(permission, expectedVersion: null, context);
        savedPermission.IsSuccess.Should().BeTrue();

        var role = new Role
        {
            RoleId = Guid.NewGuid(),
            Name = "User Searcher",
            Description = "Can search users",
            IsSystem = true,
            CreatedAt = context.Timestamp,
            UpdatedAt = context.Timestamp,
            CreatedBy = actorId,
        };
        var savedRole = await roleRepo.SaveAsync(role, expectedVersion: null, context);
        savedRole.IsSuccess.Should().BeTrue();

        stateStore.UpsertUserRoleAssignment(new UserRoleAssignment
        {
            AssignmentId = Guid.NewGuid(),
            UserId = actorId,
            RoleId = role.RoleId,
            CreatedAt = context.Timestamp,
            CreatedBy = actorId,
        });

        stateStore.UpsertRolePermissionAssignment(new RolePermissionAssignment
        {
            AssignmentId = Guid.NewGuid(),
            RoleId = role.RoleId,
            PermissionId = permission.PermissionId,
            CreatedAt = context.Timestamp,
            CreatedBy = actorId,
            ValidFrom = context.Timestamp,
        });

        return actorId;
    }

    private static async Task<HttpResponseMessage> SearchAsync(HttpClient client, Guid actorId, string query)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/core-security/users/search?q={Uri.EscapeDataString(query)}");
        request.Headers.Add("X-Actor-UserId", actorId.ToString());
        return await client.SendAsync(request);
    }

    private static async Task CreateUserAsync(HttpClient client, string usernamePrefix, string displayName)
    {
        var response = await client.PostAsJsonAsync("/api/core-security/users", new
        {
            username = $"{usernamePrefix}-{Guid.NewGuid():N}",
            email = $"{usernamePrefix}-{Guid.NewGuid():N}@example.com",
            displayName,
            createdBy = Guid.NewGuid(),
        });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
