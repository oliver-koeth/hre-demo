using AuthModule.CoreSecurity.Application.Auth;
using AuthModule.CoreSecurity.Application.Authorization;
using AuthModule.CoreSecurity.Application.Contracts;
using AuthModule.CoreSecurity.Application.Governance;
using AuthModule.CoreSecurity.Application.Tokens;
using AuthModule.CoreSecurity.Application.Users;
using AuthModule.CoreSecurity.Configuration;
using AuthModule.CoreSecurity.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace AuthModule.CoreSecurity.Bootstrap;

public static class CoreSecurityServiceCollectionExtensions
{
    public static IServiceCollection AddCoreSecurityServices(this IServiceCollection services, CoreSecurityConfiguration configuration)
    {
        var issues = configuration.Validate();
        if (issues.Count > 0)
        {
            throw new InvalidOperationException($"Invalid core security configuration: {string.Join("; ", issues)}");
        }

        services.AddSingleton(configuration);
        services.AddSingleton<ICoreSecurityStateStore, InMemoryCoreSecurityStateStore>();
        services.AddSingleton<IAuditEventSink, AuditEventSink>();

        services.AddSingleton<IPasswordVerificationService, PasswordVerificationService>();
        services.AddSingleton<ISecurityAlertService, SecurityAlertService>();
        services.AddSingleton<IAuthenticationService, AuthenticationService>();
        services.AddSingleton<IAuthorizationService, AuthorizationService>();
        services.AddSingleton<TokenService>();
        services.AddSingleton<ITokenIssueService>(sp => sp.GetRequiredService<TokenService>());
        services.AddSingleton<ITokenValidationService>(sp => sp.GetRequiredService<TokenService>());
        services.AddSingleton<IApprovalWorkflowService, ApprovalWorkflowService>();
        services.AddSingleton<IMfaVerificationService, MfaVerificationService>();
        services.AddSingleton<IUserAdministrationService, UserAdministrationService>();

        return services;
    }
}
