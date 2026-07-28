using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using AuthModule.ServiceHost.Tests.Support;
using FluentAssertions;

namespace AuthModule.ServiceHost.Tests.Performance;

public sealed class ConcurrencyPerformanceTests(ServiceHostFactory factory) : IClassFixture<ServiceHostFactory>
{
    [Fact]
    [Trait("PerfType", "Concurrency")]
    public async Task NFR_PERF_003_HealthEndpoint_ShouldHandleLightConcurrencyWithinBudget()
    {
        const int totalRequests = 160;
        const int parallelism = 16;

        var client = factory.CreateClient();
        var beforeGen0 = GC.CollectionCount(0);
        var beforeGen1 = GC.CollectionCount(1);
        var beforeGen2 = GC.CollectionCount(2);

        var failed = 0;
        var latencies = new ConcurrentBag<double>();
        var wallClock = Stopwatch.StartNew();

        await Parallel.ForEachAsync(
            Enumerable.Range(0, totalRequests),
            new ParallelOptions { MaxDegreeOfParallelism = parallelism },
            async (_, _) =>
            {
                var sw = Stopwatch.StartNew();
                var response = await client.GetAsync("/internal/foundation/health");
                sw.Stop();

                latencies.Add(sw.Elapsed.TotalMilliseconds);
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    Interlocked.Increment(ref failed);
                }
            });

        wallClock.Stop();

        var telemetry = PerformanceTelemetryWriter.Build(
            requirementId: "NFR-PERF-003",
            scenario: "Foundation health light concurrency",
            endpoint: "GET /internal/foundation/health",
            latenciesMs: latencies.ToArray(),
            failedRequests: failed,
            concurrency: parallelism,
            elapsed: wallClock.Elapsed,
            beforeGen0: beforeGen0,
            beforeGen1: beforeGen1,
            beforeGen2: beforeGen2);
        PerformanceTelemetryWriter.Write("nfr-perf-003-light-concurrency", telemetry);

        telemetry.FailedRequests.Should().Be(0);
        telemetry.P95Ms.Should().BeLessThan(1500);
        telemetry.ThroughputPerSecond.Should().BeGreaterThan(10);
    }
}
