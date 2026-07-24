using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuthModule.CoreSecurity.Application.Common;
using AuthModule.CoreSecurity.Application.Contracts;
using AuthModule.CoreSecurity.Configuration;
using AuthModule.CoreSecurity.Domain;
using AuthModule.CoreSecurity.Persistence;
using AuthModule.Foundation.Configuration;
using AuthModule.Foundation.Domain.Entities;
using AuthModule.Foundation.Domain.Primitives;
using AuthModule.Foundation.Persistence.Contracts;
using Microsoft.IdentityModel.Tokens;

namespace AuthModule.CoreSecurity.Application.Tokens;

public interface ITokenIssueService
{
    Result<LoginResponse, DomainError> Issue(User user, Guid sessionId, int tokenVersion, bool isPrivileged, RequestContext context);
}

public sealed class TokenService(
    PolicyConfiguration policyConfiguration,
    CoreSecurityConfiguration coreSecurityConfiguration,
    IStoreRepository<User> userRepository,
    ICoreSecurityStateStore stateStore,
    IAuditEventSink auditEventSink) : ITokenIssueService, ITokenValidationService
{
    private static readonly JwtSecurityTokenHandler Handler = new() { MapInboundClaims = false };
    private readonly byte[] _signingKey = Encoding.UTF8.GetBytes(coreSecurityConfiguration.TokenSigningKey);
    private readonly ConcurrentDictionary<Guid, (bool Active, int Version, DateTimeOffset ExpiresAt)> _validationCache = new();

    public Result<LoginResponse, DomainError> Issue(
        User user,
        Guid sessionId,
        int tokenVersion,
        bool isPrivileged,
        RequestContext context)
    {
        var now = context.Timestamp;
        var ttl = TimeSpan.FromSeconds(isPrivileged ? policyConfiguration.AdminTokenLifetimeSeconds : policyConfiguration.TokenLifetimeSeconds);
        var expiresAt = now.Add(ttl);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
            new("sid", sessionId.ToString()),
            new("tv", tokenVersion.ToString()),
            new("privileged", isPrivileged ? "1" : "0"),
            new("permissions", string.Empty),
        };

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Issuer = policyConfiguration.TokenIssuer,
            Audience = policyConfiguration.TokenAudience,
            Expires = expiresAt.UtcDateTime,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(_signingKey), SecurityAlgorithms.HmacSha256),
        };

        var token = Handler.CreateToken(descriptor);
        var jwt = Handler.WriteToken(token);

        _ = auditEventSink.AppendSecurityEventAsync(new SecurityAuditEvent
        {
            EventId = Guid.NewGuid(),
            EventType = SecurityEventType.TokenIssued,
            ActorId = user.UserId,
            CorrelationId = context.CorrelationId,
            SessionId = sessionId,
            Timestamp = now,
            Result = OperationResult.Success,
            Details = "Token issued.",
        }, context);

        return Result<LoginResponse, DomainError>.Success(new LoginResponse(jwt, expiresAt, sessionId));
    }

    public async Task<Result<AccessTokenClaimsModel, DomainError>> ValidateAsync(ValidateTokenRequest request, RequestContext context)
    {
        ClaimsPrincipal principal;
        SecurityToken validatedToken;
        try
        {
            principal = Handler.ValidateToken(
                request.AccessToken,
                new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(_signingKey),
                    ValidateIssuer = true,
                    ValidIssuer = policyConfiguration.TokenIssuer,
                    ValidateAudience = true,
                    ValidAudience = policyConfiguration.TokenAudience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                },
                out validatedToken);
        }
        catch (Exception ex)
        {
            return Result<AccessTokenClaimsModel, DomainError>.Failure(ErrorFactory.Unauthorized($"Token validation failed: {ex.Message}", context));
        }

        if (validatedToken is not JwtSecurityToken jwtToken)
        {
            return Result<AccessTokenClaimsModel, DomainError>.Failure(ErrorFactory.Unauthorized("Unsupported token format.", context));
        }

        var userIdClaim = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        var sessionClaim = principal.FindFirst("sid")?.Value;
        var versionClaim = principal.FindFirst("tv")?.Value;
        var privilegedClaim = principal.FindFirst("privileged")?.Value ?? "0";
        var permissionsClaim = principal.FindFirst("permissions")?.Value ?? string.Empty;

        if (!Guid.TryParse(userIdClaim, out var userId) || !Guid.TryParse(sessionClaim, out var sessionId) || !int.TryParse(versionClaim, out var tokenVersion))
        {
            return Result<AccessTokenClaimsModel, DomainError>.Failure(ErrorFactory.Unauthorized("Missing required claims.", context));
        }

        var activeAndVersion = await ResolveUserStateAsync(userId, context);
        if (activeAndVersion.IsFailure)
        {
            return Result<AccessTokenClaimsModel, DomainError>.Failure(activeAndVersion.Error);
        }

        var (active, currentTokenVersion) = activeAndVersion.Value;
        if (!active || currentTokenVersion != tokenVersion)
        {
            return Result<AccessTokenClaimsModel, DomainError>.Failure(ErrorFactory.Unauthorized("Token rejected by account state or version.", context));
        }

        var model = new AccessTokenClaimsModel
        {
            SubjectUserId = userId,
            SessionId = sessionId,
            TokenVersion = tokenVersion,
            Issuer = jwtToken.Issuer,
            Audience = principal.FindFirst("aud")?.Value ?? policyConfiguration.TokenAudience,
            IssuedAt = jwtToken.ValidFrom,
            ExpiresAt = jwtToken.ValidTo,
            PermissionKeys = permissionsClaim.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries),
            IsPrivileged = privilegedClaim == "1",
        };

        _ = auditEventSink.AppendSecurityEventAsync(new SecurityAuditEvent
        {
            EventId = Guid.NewGuid(),
            EventType = SecurityEventType.TokenValidated,
            ActorId = model.SubjectUserId,
            CorrelationId = context.CorrelationId,
            SessionId = model.SessionId,
            Timestamp = context.Timestamp,
            Result = OperationResult.Success,
            Details = "Token validated.",
        }, context);

        return Result<AccessTokenClaimsModel, DomainError>.Success(model);
    }

    private async Task<Result<(bool Active, int TokenVersion), DomainError>> ResolveUserStateAsync(Guid userId, RequestContext context)
    {
        var now = context.Timestamp;
        if (_validationCache.TryGetValue(userId, out var cached) && cached.ExpiresAt > now)
        {
            return Result<(bool Active, int TokenVersion), DomainError>.Success((cached.Active, cached.Version));
        }

        var user = await userRepository.GetAsync(new StoreQuery(userId), context);
        if (user.IsFailure || user.Value is null)
        {
            return Result<(bool Active, int TokenVersion), DomainError>.Failure(ErrorFactory.Unauthorized("User not found for token validation.", context));
        }

        var active = user.Value.Status == UserStatus.Active;
        var version = stateStore.GetTokenVersion(userId);
        _validationCache[userId] = (active, version, now.AddSeconds(coreSecurityConfiguration.TokenValidationCacheSeconds));
        return Result<(bool Active, int TokenVersion), DomainError>.Success((active, version));
    }
}
