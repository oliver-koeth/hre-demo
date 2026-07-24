using AuthModule.Foundation.Configuration;
using AuthModule.Foundation.Domain.Entities;
using AuthModule.Foundation.Observability;
using AuthModule.Foundation.Persistence.Contracts;
using AuthModule.Foundation.Persistence.Repositories;
using AuthModule.Foundation.Runtime;
using AuthModule.Foundation.Security;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AuthModule.Foundation.Bootstrap;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFoundationServices(this IServiceCollection services, PolicyConfiguration configuration)
    {
        var issues = configuration.Validate();
        if (issues.Count > 0)
        {
            throw new InvalidOperationException($"Invalid policy configuration: {string.Join("; ", issues)}");
        }

        var logger = ObservabilityExtensions.CreateBootstrapLogger();
        services.AddSingleton(logger);
        services.AddLogging(builder => builder.AddConsole());

        services.AddSingleton(configuration);
        services.AddSingleton<IPolicyConfigurationService, PolicyConfigurationService>();

        services.AddSingleton<IKeyProvider>(_ =>
            new KeyProvider(configuration.EncryptionKeyPath, configuration.HmacKeyPath));
        services.AddSingleton<IEncryptionService, EncryptionService>();
        services.AddSingleton<IIntegrityService, IntegrityService>();
        services.AddSingleton<IStoreWriteCoordinator, StoreWriteCoordinator>();
        services.AddSingleton<SecurityEventLogger>();

        services.AddFoundationTelemetry("AuthModule.Foundation");

        var basePath = configuration.StoreBasePath;
        Directory.CreateDirectory(basePath);

        services.AddSingleton<IStoreRepository<User>>(sp =>
            new JsonStoreRepository<User>(
                Path.Combine(basePath, "auth-store", "users.json"),
                "AuthStore/Users",
                sp.GetRequiredService<IEncryptionService>(),
                sp.GetRequiredService<IIntegrityService>()));

        services.AddSingleton<IStoreRepository<Role>>(sp =>
            new JsonStoreRepository<Role>(
                Path.Combine(basePath, "authz-store", "roles.json"),
                "AuthzStore/Roles",
                sp.GetRequiredService<IEncryptionService>(),
                sp.GetRequiredService<IIntegrityService>()));

        services.AddSingleton<IStoreRepository<Permission>>(sp =>
            new JsonStoreRepository<Permission>(
                Path.Combine(basePath, "authz-store", "permissions.json"),
                "AuthzStore/Permissions",
                sp.GetRequiredService<IEncryptionService>(),
                sp.GetRequiredService<IIntegrityService>()));

        services.AddSingleton<IAuditStoreRepository>(sp =>
            new JsonAuditStoreRepository(
                new JsonStoreRepository<AuditEventEnvelope<SecurityAuditEvent>>(
                    Path.Combine(basePath, "audit-store", "security-events.json"),
                    "AuditStore/SecurityEvents",
                    sp.GetRequiredService<IEncryptionService>(),
                    sp.GetRequiredService<IIntegrityService>()),
                new JsonStoreRepository<AuditEventEnvelope<AdminChangeEvent>>(
                    Path.Combine(basePath, "audit-store", "admin-change-events.json"),
                    "AuditStore/AdminChangeEvents",
                    sp.GetRequiredService<IEncryptionService>(),
                    sp.GetRequiredService<IIntegrityService>())));

        services.AddSingleton<IStoreIntegrityService>(sp =>
            new StoreIntegrityService(
                sp.GetRequiredService<IIntegrityService>(),
                new[]
                {
                    Path.Combine(basePath, "auth-store", "users.json"),
                    Path.Combine(basePath, "authz-store", "roles.json"),
                    Path.Combine(basePath, "authz-store", "permissions.json"),
                    Path.Combine(basePath, "audit-store", "security-events.json"),
                }));

        return services;
    }
}
