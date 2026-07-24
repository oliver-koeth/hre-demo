namespace AuthModule.Foundation.Domain.Entities;

public sealed class Role : BaseStoreEntity
{
    public Guid RoleId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsSystem { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public Guid CreatedBy { get; set; }
    public override Guid Id => RoleId;
}

public sealed class Permission : BaseStoreEntity
{
    public Guid PermissionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Resource { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsSystem { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public Guid CreatedBy { get; set; }
    public override Guid Id => PermissionId;
}

public sealed class RolePermissionAssignment : BaseStoreEntity
{
    public Guid AssignmentId { get; set; }
    public Guid RoleId { get; set; }
    public Guid PermissionId { get; set; }
    public DateTimeOffset? ValidFrom { get; set; }
    public DateTimeOffset? ValidUntil { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public Guid CreatedBy { get; set; }
    public override Guid Id => AssignmentId;

    public bool IsActiveAt(DateTimeOffset moment) =>
        !IsDeleted
        && (ValidFrom is null || ValidFrom <= moment)
        && (ValidUntil is null || ValidUntil >= moment);

    public bool HasValidWindow() => ValidFrom is null || ValidUntil is null || ValidFrom <= ValidUntil;
}

public sealed class UserRoleAssignment : BaseStoreEntity
{
    public Guid AssignmentId { get; set; }
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
    public DateTimeOffset? ValidFrom { get; set; }
    public DateTimeOffset? ValidUntil { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public Guid CreatedBy { get; set; }
    public override Guid Id => AssignmentId;

    public bool IsActiveAt(DateTimeOffset moment) =>
        !IsDeleted
        && (ValidFrom is null || ValidFrom <= moment)
        && (ValidUntil is null || ValidUntil >= moment);

    public bool HasValidWindow() => ValidFrom is null || ValidUntil is null || ValidFrom <= ValidUntil;
}

