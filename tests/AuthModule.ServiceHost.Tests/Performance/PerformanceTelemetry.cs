using System.Text.Json;

namespace AuthModule.ServiceHost.Tests.Performance;

internal sealed record PerformanceTelemetry(
    string RequirementId,
    string Scenario,
    string Endpoint,
    int TotalRequests,
    int FailedRequests,
    int Concurrency,
    double P50Ms,
    double P95Ms,
    double P99Ms,
    double MaxMs,
    double ThroughputPerSecond,
    int Gen0Collections,
    int Gen1Collections,
    int Gen2Collections,
    DateTimeOffset MeasuredAtUtc);

internal static class PerformanceTelemetryWriter
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
    };

    public static void Write(string scenarioKey, PerformanceTelemetry telemetry)
    {
        var outputDir = Environment.GetEnvironmentVariable("PERF_RESULTS_DIR");
        if (string.IsNullOrWhiteSpace(outputDir))
        {
            outputDir = Path.Combine(AppContext.BaseDirectory, "perf-artifacts");
        }

        Directory.CreateDirectory(outputDir);
        var file = Path.Combine(
            outputDir,
            $"{scenarioKey}-{DateTimeOffset.UtcNow:yyyyMMddHHmmssfff}-{Guid.NewGuid():N}.json");
        File.WriteAllText(file, JsonSerializer.Serialize(telemetry, JsonOptions));
    }

    public static PerformanceTelemetry Build(
        string requirementId,
        string scenario,
        string endpoint,
        IReadOnlyCollection<double> latenciesMs,
        int failedRequests,
        int concurrency,
        TimeSpan elapsed,
        int beforeGen0,
        int beforeGen1,
        int beforeGen2)
    {
        var ordered = latenciesMs.OrderBy(x => x).ToArray();
        var p50 = Percentile(ordered, 0.50);
        var p95 = Percentile(ordered, 0.95);
        var p99 = Percentile(ordered, 0.99);
        var max = ordered.Length == 0 ? 0 : ordered[^1];
        var throughput = elapsed.TotalSeconds <= 0
            ? 0
            : (latenciesMs.Count - failedRequests) / elapsed.TotalSeconds;

        return new PerformanceTelemetry(
            requirementId,
            scenario,
            endpoint,
            latenciesMs.Count,
            failedRequests,
            concurrency,
            p50,
            p95,
            p99,
            max,
            throughput,
            GC.CollectionCount(0) - beforeGen0,
            GC.CollectionCount(1) - beforeGen1,
            GC.CollectionCount(2) - beforeGen2,
            DateTimeOffset.UtcNow);
    }

    private static double Percentile(IReadOnlyList<double> ordered, double percentile)
    {
        if (ordered.Count == 0)
        {
            return 0;
        }

        var index = (int)Math.Ceiling(percentile * ordered.Count) - 1;
        index = Math.Clamp(index, 0, ordered.Count - 1);
        return ordered[index];
    }
}
