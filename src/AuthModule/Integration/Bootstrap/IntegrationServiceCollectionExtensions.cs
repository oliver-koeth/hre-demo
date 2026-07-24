using AuthModule.Foundation.Persistence.Contracts;
using AuthModule.Integration.Application.Alerts;
using AuthModule.Integration.Application.Conformance;
using AuthModule.Integration.Application.Contracts;
using AuthModule.Integration.Application.Gate;
using AuthModule.Integration.Application.Runtime;
using AuthModule.Integration.Application.Traceability;
using AuthModule.Integration.Configuration;
using AuthModule.Integration.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace AuthModule.Integration.Bootstrap;

public static class IntegrationServiceCollectionExtensions
{
    public static IServiceCollection AddIntegrationServices(this IServiceCollection services, IntegrationConfiguration configuration)
    {
        var issues = configuration.Validate();
        if (issues.Count > 0)
        {
            throw new InvalidOperationException($"Invalid integration configuration: {string.Join("; ", issues)}");
        }

        services.AddSingleton(configuration);
        services.AddSingleton<IIntegrationStateStore, InMemoryIntegrationStateStore>();
        services.AddSingleton<IContractConformanceChecker, ContractConformanceChecker>();
        services.AddSingleton<ITraceabilityChecker, TraceabilityChecker>();
        services.AddSingleton<IRuntimeArtifactChecker, RuntimeArtifactChecker>();
        services.AddSingleton<IIntegrationAlertService, IntegrationAlertService>();
        services.AddSingleton<IIntegrationGateService, IntegrationGateService>();

        return services;
    }
}
