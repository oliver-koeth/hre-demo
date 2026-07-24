using AuthModule.Foundation.Domain.Primitives;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AuthModule.CoreSecurity.Api;

public static class CoreSecurityProblemDetails
{
    public static IResult ToProblem(this DomainError error)
    {
        var status = error.Code switch
        {
            DomainErrorCode.ValidationFailed => StatusCodes.Status400BadRequest,
            DomainErrorCode.Unauthorized => StatusCodes.Status401Unauthorized,
            DomainErrorCode.Forbidden => StatusCodes.Status403Forbidden,
            DomainErrorCode.NotFound => StatusCodes.Status404NotFound,
            DomainErrorCode.Conflict => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError,
        };

        var details = new ProblemDetails
        {
            Title = error.Code.ToString(),
            Detail = error.Message,
            Status = status,
            Type = $"urn:auth-module:error:{error.Code}",
        };
        details.Extensions["errorCode"] = error.Code.ToString().ToUpperInvariant();
        details.Extensions["correlationId"] = error.CorrelationId;
        return Results.Problem(details);
    }
}
