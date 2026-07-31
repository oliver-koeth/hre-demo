using AuthModule.CoreSecurity.Application.Contracts;
using AuthModule.Foundation.Domain.Primitives;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace AuthModule.CoreSecurity.Api;

public static class CoreSecurityEndpoints
{
    public static IEndpointRouteBuilder MapCoreSecurityEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/core-security")
            .WithTags("Core Security");

        group.MapPost("/auth/login", async (LoginRequest request, HttpContext httpContext, IAuthenticationService authService) =>
        {
            var context = BuildContext(httpContext);
            var result = await authService.LoginAsync(request, context);
            return result.IsSuccess ? Results.Ok(result.Value) : result.Error.ToProblem();
        })
        .WithSummary("Authenticates a user and issues a session token.")
        .WithDescription("Validates login credentials and creates an authentication token for regular or privileged sessions.");

        group.MapPost("/auth/validate", async (ValidateTokenRequest request, HttpContext httpContext, ITokenValidationService validationService) =>
        {
            var context = BuildContext(httpContext);
            var result = await validationService.ValidateAsync(request, context);
            return result.IsSuccess ? Results.Ok(result.Value) : result.Error.ToProblem();
        })
        .WithSummary("Validates an authentication token.")
        .WithDescription("Checks token signature, expiry, and policy constraints to confirm the caller session is still valid.");

        group.MapPost("/authz/evaluate", async (AuthorizationRequest request, HttpContext httpContext, IAuthorizationService authorizationService) =>
        {
            var context = BuildContext(httpContext);
            var result = await authorizationService.AuthorizeAsync(request, context);
            return result.IsSuccess ? Results.Ok(result.Value) : result.Error.ToProblem();
        })
        .WithSummary("Evaluates authorization for an action.")
        .WithDescription("Determines whether the requesting identity can perform the requested action on the target resource.");

        group.MapPost("/mfa/challenges", async (CreateStepUpChallengeRequest request, HttpContext httpContext, IMfaVerificationService mfaService) =>
        {
            var context = BuildContext(httpContext);
            var result = await mfaService.CreateChallengeAsync(request, context);
            return result.IsSuccess ? Results.Ok(result.Value) : result.Error.ToProblem();
        })
        .WithSummary("Creates an MFA step-up challenge.")
        .WithDescription("Starts a multi-factor authentication challenge for a high-risk or privileged operation.");

        group.MapPost("/mfa/verify", async (VerifyStepUpChallengeRequest request, HttpContext httpContext, IMfaVerificationService mfaService) =>
        {
            var context = BuildContext(httpContext);
            var result = await mfaService.VerifyChallengeAsync(request, context);
            return result.IsSuccess ? Results.NoContent() : result.Error.ToProblem();
        })
        .WithSummary("Verifies an MFA challenge response.")
        .WithDescription("Validates the submitted MFA proof for an existing challenge and records the verification result.");

        group.MapPost("/governance/approvals", async (ApprovalRequest request, HttpContext httpContext, IApprovalWorkflowService approvalService) =>
        {
            var context = BuildContext(httpContext);
            var result = await approvalService.RequestApprovalAsync(request, context);
            return result.IsSuccess ? Results.Ok(result.Value) : result.Error.ToProblem();
        })
        .WithSummary("Submits a governance approval request.")
        .WithDescription("Creates an approval workflow entry for sensitive changes that require governance control.");

        group.MapPost("/governance/approvals/decide", async (ApprovalDecisionRequest request, HttpContext httpContext, IApprovalWorkflowService approvalService) =>
        {
            var context = BuildContext(httpContext);
            var result = await approvalService.DecideApprovalAsync(request, context);
            return result.IsSuccess ? Results.Ok(result.Value) : result.Error.ToProblem();
        })
        .WithSummary("Records an approval decision.")
        .WithDescription("Approves or rejects a pending governance approval request and stores the decision evidence.");

        group.MapPost("/users", async (CreateUserRequest request, HttpContext httpContext, IUserAdministrationService userService) =>
        {
            var context = BuildContext(httpContext);
            var result = await userService.CreateUserAsync(request, context);
            return result.IsSuccess ? Results.Ok(result.Value) : result.Error.ToProblem();
        })
        .WithSummary("Creates a user account.")
        .WithDescription("Registers a new user with identity and role assignments under current security policy rules.");

        group.MapPut("/users/{userId:guid}", async (Guid userId, UpdateUserRequest request, HttpContext httpContext, IUserAdministrationService userService) =>
        {
            var context = BuildContext(httpContext);
            var result = await userService.UpdateUserAsync(request with { UserId = userId }, context);
            return result.IsSuccess ? Results.Ok(result.Value) : result.Error.ToProblem();
        })
        .WithSummary("Updates a user account.")
        .WithDescription("Modifies user profile and role data for the specified user identifier.");

        group.MapPost("/users/{userId:guid}/disable", async (Guid userId, DisableUserRequest request, HttpContext httpContext, IUserAdministrationService userService) =>
        {
            var context = BuildContext(httpContext);
            var result = await userService.DisableUserAsync(request with { UserId = userId }, context);
            return result.IsSuccess ? Results.Ok(result.Value) : result.Error.ToProblem();
        })
        .WithSummary("Disables a user account.")
        .WithDescription("Blocks the specified user from future authentication and marks the account as disabled.");

        group.MapGet("/users/search", async (string? q, int? page, int? pageSize, HttpContext httpContext, IUserAdministrationService userService) =>
        {
            if (!httpContext.Request.Headers.TryGetValue("X-Actor-UserId", out _) ||
                !Guid.TryParse(httpContext.Request.Headers["X-Actor-UserId"], out _))
            {
                return Results.Problem(new ProblemDetails
                {
                    Title = "Unauthorized",
                    Detail = "The X-Actor-UserId header is required and must contain a valid GUID.",
                    Status = StatusCodes.Status401Unauthorized,
                    Type = "urn:auth-module:error:Unauthorized",
                });
            }

            var context = BuildContext(httpContext);
            var result = await userService.SearchUsersAsync(
                new SearchUsersRequest(q ?? string.Empty, page ?? 1, pageSize ?? 20),
                context);
            return result.IsSuccess ? Results.Ok(result.Value) : result.Error.ToProblem();
        })
        .WithSummary("Searches user accounts by display name.")
        .WithDescription("Returns a paginated list of users whose display name contains the supplied query string (case-insensitive). Requires the users:search permission.");

        return app;
    }

    private static RequestContext BuildContext(HttpContext context)
    {
        Guid? userId = Guid.TryParse(context.Request.Headers["X-Actor-UserId"], out var parsedUserId) ? parsedUserId : null;
        Guid? sessionId = Guid.TryParse(context.Request.Headers["X-Session-Id"], out var parsedSessionId) ? parsedSessionId : null;
        return new RequestContext(
            CorrelationId: Guid.NewGuid(),
            UserId: userId,
            SourceIp: context.Connection.RemoteIpAddress?.ToString(),
            Timestamp: DateTimeOffset.UtcNow,
            SessionId: sessionId);
    }
}
