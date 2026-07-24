using AuthModule.CoreSecurity.Application.Auth;
using AuthModule.CoreSecurity.Application.Authorization;
using AuthModule.CoreSecurity.Application.Contracts;
using AuthModule.CoreSecurity.Application.Governance;
using AuthModule.CoreSecurity.Application.Tokens;
using AuthModule.CoreSecurity.Application.Users;
using AuthModule.CoreSecurity.Configuration;
using AuthModule.CoreSecurity.Persistence;
using AuthModule.Foundation.Configuration;
using AuthModule.Foundation.Domain.Entities;
using AuthModule.Foundation.Domain.Primitives;

namespace AuthModule.CoreSecurity.Tests.Support;

public sealed class TestContext
{
    public required InMemoryStoreRepository<User> Users { get; init; }
    public required InMemoryStoreRepository<Role> Roles { get; init; }
    public required InMemoryStoreRepository<Permission> Permissions { get; init; }
    public required InMemoryCoreSecurityStateStore StateStore { get; init; }
    public required InMemoryAuditStoreRepository AuditRepository { get; init; }
    public required IAuthenticationService AuthenticationService { get; init; }
    public required ITokenValidationService TokenValidationService { get; init; }
    public required IAuthorizationService AuthorizationService { get; init; }
    public required IApprovalWorkflowService ApprovalWorkflowService { get; init; }
    public required IUserAdministrationService UserAdministrationService { get; init; }
    public required IMfaVerificationService MfaVerificationService { get; init; }
    public required RequestContext RequestContext { get; init; }
}

public static class TestContextFactory
{
    public static async Task<TestContext> CreateAsync()
    {
        var users = new InMemoryStoreRepository<User>();
        var roles = new InMemoryStoreRepository<Role>();
        var permissions = new InMemoryStoreRepository<Permission>();
        var stateStore = new InMemoryCoreSecurityStateStore();
        var audit = new InMemoryAuditStoreRepository();
        var context = RequestContext.CreateAnonymous("127.0.0.1");
        var policy = new PolicyConfiguration
        {
            TokenLifetimeSeconds = 3600,
            AdminTokenLifetimeSeconds = 900,
            TokenIssuer = "auth-module",
            TokenAudience = "auth-module-clients",
            MaxLoginAttempts = 5,
            LockoutDurationSeconds = 900,
            StoreBasePath = "/tmp",
            EncryptionKeyPath = "/tmp/enc",
            HmacKeyPath = "/tmp/hmac",
            AuditEventRetentionDays = 90,
            UserRecordRetentionDays = 365,
            IncidentRecordRetentionDays = 365,
            SodApprovalRequiredForRoleChanges = true,
        };
        var securityConfig = new CoreSecurityConfiguration
        {
            MaxLoginAttempts = 3,
            LockoutDurationSeconds = 60,
            StepUpChallengeTtlSeconds = 120,
            TokenValidationCacheSeconds = 30,
            ApprovalRetryCount = 1,
            TokenSigningKey = "local-test-signing-key-local-test-sign",
        };

        var sink = new AuditEventSink(audit);
        var password = new PasswordVerificationService();
        var alertService = new SecurityAlertService(sink, new Microsoft.Extensions.Logging.Abstractions.NullLogger<SecurityAlertService>());
        var tokenService = new TokenService(policy, securityConfig, users, stateStore, sink);
        var authenticationService = new AuthenticationService(users, stateStore, password, tokenService, policy, securityConfig, sink, alertService);
        var authorizationService = new AuthorizationService(permissions, stateStore);
        var approvalService = new ApprovalWorkflowService(securityConfig, stateStore, sink);
        var mfaService = new MfaVerificationService(securityConfig, stateStore, sink);
        var userService = new UserAdministrationService(users, stateStore, password, sink);

        var user = new User
        {
            UserId = Guid.NewGuid(),
            Username = "alice",
            Email = "alice@example.com",
            DisplayName = "Alice",
            Status = UserStatus.Active,
            CreatedAt = context.Timestamp,
            UpdatedAt = context.Timestamp,
            CreatedBy = Guid.NewGuid(),
            Version = 0,
        };
        await users.SaveAsync(user, null, context);
        var credential = password.HashForNewCredential(user.UserId, "Password!123");
        stateStore.UpsertCredential(credential);
        stateStore.SetTokenVersion(user.UserId, 0);

        return new TestContext
        {
            Users = users,
            Roles = roles,
            Permissions = permissions,
            StateStore = stateStore,
            AuditRepository = audit,
            AuthenticationService = authenticationService,
            TokenValidationService = tokenService,
            AuthorizationService = authorizationService,
            ApprovalWorkflowService = approvalService,
            UserAdministrationService = userService,
            MfaVerificationService = mfaService,
            RequestContext = context with { UserId = Guid.NewGuid() },
        };
    }
}
