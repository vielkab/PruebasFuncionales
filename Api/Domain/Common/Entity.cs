namespace Api.Domain.Common;

public abstract class Entity<TId>
{
    public TId Id { get; protected set; } = default!;

    public override bool Equals(object? obj)
    {
        return obj is Entity<TId> entity &&
               EqualityComparer<TId>.Default.Equals(Id, entity.Id);
    }

    public override int GetHashCode()
    {
        return Id is null ? 0 : EqualityComparer<TId>.Default.GetHashCode(Id);
    }
}
