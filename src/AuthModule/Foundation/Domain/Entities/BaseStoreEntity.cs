using AuthModule.Foundation.Domain.Primitives;

namespace AuthModule.Foundation.Domain.Entities;

public abstract class BaseStoreEntity : IStoreEntity
{
    public abstract Guid Id { get; }
    public int Version { get; set; }
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }
}

