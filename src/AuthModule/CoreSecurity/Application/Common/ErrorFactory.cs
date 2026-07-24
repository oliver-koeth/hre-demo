using AuthModule.Foundation.Domain.Primitives;

namespace AuthModule.CoreSecurity.Application.Common;

internal static class ErrorFactory
{
    public static DomainError Validation(string message, RequestContext context) =>
        new(DomainErrorCode.ValidationFailed, message, context.CorrelationId);

    public static DomainError Unauthorized(string message, RequestContext context) =>
        new(DomainErrorCode.Unauthorized, message, context.CorrelationId);

    public static DomainError Forbidden(string message, RequestContext context) =>
        new(DomainErrorCode.Forbidden, message, context.CorrelationId);

    public static DomainError Conflict(string message, RequestContext context) =>
        new(DomainErrorCode.Conflict, message, context.CorrelationId);

    public static DomainError NotFound(string message, RequestContext context) =>
        new(DomainErrorCode.NotFound, message, context.CorrelationId);

    public static DomainError Internal(string message, RequestContext context) =>
        new(DomainErrorCode.Internal, message, context.CorrelationId);
}
