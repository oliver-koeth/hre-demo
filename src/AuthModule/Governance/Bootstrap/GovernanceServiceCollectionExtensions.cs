using AuthModule.Foundation.Persistence.Contracts;
using AuthModule.Governance.Application.AuditEvidence;
using AuthModule.Governance.Application.Contracts;
using AuthModule.Governance.Application.DataSubject;
using AuthModule.Governance.Application.Operations;
using AuthModule.Governance.Application.Retention;
using AuthModule.Governance.Application.Security;
using AuthModule.Governance.Configuration;
using AuthModule.Governance.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace AuthModule.Governance.Bootstrap;

public static class GovernanceServiceCollectionExtensions
{
    public static IServiceCollection AddGovernanceServices(this IServiceCollection services, GovernanceConfiguration configuration)
    {
        var issues = configuration.Validate();
        if (issues.Count > 0)
        {
            throw new InvalidOperationException($"Invalid governance configuration: {string.Join("; ", issues)}");
        }

        services.AddSingleton(configuration);
        services.AddSingleton<IGovernanceStateStore, InMemoryGovernanceStateStore>();
        services.AddSingleton<ILegalHoldPolicyGuard, LegalHoldPolicyGuard>();
        services.AddSingleton<IAlertService, AlertService>();

        services.AddSingleton<IAuditQueryService, AuditQueryService>();
        services.AddSingleton<IEvidenceService, EvidenceService>();
        services.AddSingleton<IDataSubjectService, DataSubjectService>();
        services.AddSingleton<IRetentionService, RetentionService>();
        services.AddSingleton<IIncidentService, IncidentService>();
        services.AddSingleton<IBackupEvidenceService, BackupEvidenceService>();

        return services;
    }
}
