using AuthModule.Foundation.Domain.Primitives;

namespace AuthModule.Integration.Application.Common;

internal static class ErrorFactory
{
    public static DomainError Validation(string message, RequestContext context) =>
        new(DomainErrorCode.ValidationFailed, message, context.CorrelationId);

    public static DomainError NotFound(string message, RequestContext context) =>
        new(DomainErrorCode.NotFound, message, context.CorrelationId);

    public static DomainError Conflict(string message, RequestContext context) =>
        new(DomainErrorCode.Conflict, message, context.CorrelationId);

    public static DomainError PolicyViolation(string message, RequestContext context) =>
        new(DomainErrorCode.PolicyViolation, message, context.CorrelationId);
}
