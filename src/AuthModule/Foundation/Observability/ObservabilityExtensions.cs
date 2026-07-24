using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Events;

namespace AuthModule.Foundation.Observability;

public static class ObservabilityExtensions
{
    public static IServiceCollection AddFoundationTelemetry(this IServiceCollection services, string serviceName)
    {
        services
            .AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(serviceName))
            .WithMetrics(metrics =>
            {
                metrics
                    .AddRuntimeInstrumentation()
                    .AddMeter(serviceName)
                    .AddConsoleExporter();
            })
            .WithTracing(tracing =>
            {
                tracing
                    .AddSource(serviceName)
                    .AddConsoleExporter();
            });

        return services;
    }

    public static Serilog.ILogger CreateBootstrapLogger()
    {
        return new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .WriteTo.Console(
                outputTemplate:
                "{Timestamp:O} [{Level:u3}] CorrelationId={CorrelationId} SourceContext={SourceContext} {Message:lj}{NewLine}{Exception}")
            .CreateLogger();
    }
}

public sealed class SecurityEventLogger(ILogger<SecurityEventLogger> logger)
{
    public void IntegrityFailure(string storePath, Guid correlationId) =>
        logger.LogError("Integrity verification failed for {StorePath}. CorrelationId={CorrelationId}", storePath, correlationId);

    public void MigrationApplied(string storePath, string fromVersion, string toVersion, Guid correlationId) =>
        logger.LogWarning(
            "Schema migration applied for {StorePath}. From={FromVersion} To={ToVersion} CorrelationId={CorrelationId}",
            storePath,
            fromVersion,
            toVersion,
            correlationId);

    public void ConcurrencyConflict(string storePath, Guid entityId, Guid correlationId) =>
        logger.LogWarning(
            "Optimistic concurrency conflict on {StorePath} EntityId={EntityId} CorrelationId={CorrelationId}",
            storePath,
            entityId,
            correlationId);

    public void KeyLoadingFailure(string keyPath, Exception ex, Guid correlationId) =>
        logger.LogCritical(ex, "Failed to load key material from {KeyPath}. CorrelationId={CorrelationId}", keyPath, correlationId);
}
