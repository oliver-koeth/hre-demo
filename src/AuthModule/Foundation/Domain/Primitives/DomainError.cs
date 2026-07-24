namespace AuthModule.Foundation.Domain.Primitives;

public enum DomainErrorCode
{
    NotFound,
    Conflict,
    ValidationFailed,
    IntegrityViolation,
    PolicyViolation,
    Unauthorized,
    Forbidden,
    Internal,
}

public sealed record DomainError(
    DomainErrorCode Code,
    string Message,
    Guid CorrelationId,
    IReadOnlyDictionary<string, string>? Details = null);

