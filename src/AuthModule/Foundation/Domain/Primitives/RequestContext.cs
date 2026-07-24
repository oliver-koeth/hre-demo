namespace AuthModule.Foundation.Domain.Primitives;

public sealed record RequestContext(
    Guid CorrelationId,
    Guid? UserId,
    string? SourceIp,
    DateTimeOffset Timestamp,
    Guid? SessionId)
{
    public static RequestContext CreateAnonymous(string? sourceIp = null) =>
        new(Guid.NewGuid(), null, sourceIp, DateTimeOffset.UtcNow, null);
}

