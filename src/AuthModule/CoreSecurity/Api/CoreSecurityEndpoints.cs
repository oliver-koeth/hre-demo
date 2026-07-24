using AuthModule.CoreSecurity.Application.Contracts;
using AuthModule.Foundation.Domain.Primitives;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace AuthModule.CoreSecurity.Api;

public static class CoreSecurityEndpoints
{
    public static IEndpointRouteBuilder MapCoreSecurityEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/core-security");

        group.MapPost("/auth/login", async (LoginRequest request, HttpContext httpContext, IAuthenticationService authService) =>
        {
            var context = BuildContext(httpContext);
            var result = await authService.LoginAsync(request, context);
            return result.IsSuccess ? Results.Ok(result.Value) : result.Error.ToProblem();
        });

        group.MapPost("/auth/validate", async (ValidateTokenRequest request, HttpContext httpContext, ITokenValidationService validationService) =>
        {
            var context = BuildContext(httpContext);
            var result = await validationService.ValidateAsync(request, context);
            return result.IsSuccess ? Results.Ok(result.Value) : result.Error.ToProblem();
        });

        group.MapPost("/authz/evaluate", async (AuthorizationRequest request, HttpContext httpContext, IAuthorizationService authorizationService) =>
        {
            var context = BuildContext(httpContext);
            var result = await authorizationService.AuthorizeAsync(request, context);
            return result.IsSuccess ? Results.Ok(result.Value) : result.Error.ToProblem();
        });

        group.MapPost("/mfa/challenges", async (CreateStepUpChallengeRequest request, HttpContext httpContext, IMfaVerificationService mfaService) =>
        {
            var context = BuildContext(httpContext);
            var result = await mfaService.CreateChallengeAsync(request, context);
            return result.IsSuccess ? Results.Ok(result.Value) : result.Error.ToProblem();
        });

        group.MapPost("/mfa/verify", async (VerifyStepUpChallengeRequest request, HttpContext httpContext, IMfaVerificationService mfaService) =>
        {
            var context = BuildContext(httpContext);
            var result = await mfaService.VerifyChallengeAsync(request, context);
            return result.IsSuccess ? Results.NoContent() : result.Error.ToProblem();
        });

        group.MapPost("/governance/approvals", async (ApprovalRequest request, HttpContext httpContext, IApprovalWorkflowService approvalService) =>
        {
            var context = BuildContext(httpContext);
            var result = await approvalService.RequestApprovalAsync(request, context);
            return result.IsSuccess ? Results.Ok(result.Value) : result.Error.ToProblem();
        });

        group.MapPost("/governance/approvals/decide", async (ApprovalDecisionRequest request, HttpContext httpContext, IApprovalWorkflowService approvalService) =>
        {
            var context = BuildContext(httpContext);
            var result = await approvalService.DecideApprovalAsync(request, context);
            return result.IsSuccess ? Results.Ok(result.Value) : result.Error.ToProblem();
        });

        group.MapPost("/users", async (CreateUserRequest request, HttpContext httpContext, IUserAdministrationService userService) =>
        {
            var context = BuildContext(httpContext);
            var result = await userService.CreateUserAsync(request, context);
            return result.IsSuccess ? Results.Ok(result.Value) : result.Error.ToProblem();
        });

        group.MapPut("/users/{userId:guid}", async (Guid userId, UpdateUserRequest request, HttpContext httpContext, IUserAdministrationService userService) =>
        {
            var context = BuildContext(httpContext);
            var result = await userService.UpdateUserAsync(request with { UserId = userId }, context);
            return result.IsSuccess ? Results.Ok(result.Value) : result.Error.ToProblem();
        });

        group.MapPost("/users/{userId:guid}/disable", async (Guid userId, DisableUserRequest request, HttpContext httpContext, IUserAdministrationService userService) =>
        {
            var context = BuildContext(httpContext);
            var result = await userService.DisableUserAsync(request with { UserId = userId }, context);
            return result.IsSuccess ? Results.Ok(result.Value) : result.Error.ToProblem();
        });

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
