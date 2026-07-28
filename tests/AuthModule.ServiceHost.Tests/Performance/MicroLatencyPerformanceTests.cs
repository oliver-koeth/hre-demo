using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using AuthModule.ServiceHost.Tests.Support;
using FluentAssertions;

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
}
