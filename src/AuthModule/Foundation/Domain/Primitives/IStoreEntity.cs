namespace AuthModule.Foundation.Domain.Primitives;

public interface IStoreEntity
{
    Guid Id { get; }
    int Version { get; set; }
    bool IsDeleted { get; set; }
    DateTimeOffset? DeletedAt { get; set; }
    Guid? DeletedBy { get; set; }
}

