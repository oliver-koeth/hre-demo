namespace AuthModule.Foundation.Domain.Entities;

public enum UserStatus
{
    PendingActivation,
    Active,
    Locked,
    Inactive,
}

public enum HashAlgorithm
{
    Argon2id,
    BCrypt,
}

public sealed class User : BaseStoreEntity
{
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public UserStatus Status { get; set; } = UserStatus.PendingActivation;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public Guid CreatedBy { get; set; }
    public override Guid Id => UserId;
}

public sealed class Credential : BaseStoreEntity
{
    public Guid CredentialId { get; set; }
    public Guid UserId { get; set; }
    public HashAlgorithm Algorithm { get; set; } = HashAlgorithm.Argon2id;
    public string Hash { get; set; } = string.Empty;
    public string Salt { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }
    public bool IsActive { get; set; } = true;
    public override Guid Id => CredentialId;
}

