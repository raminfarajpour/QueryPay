namespace Billing.Domain.SeedWorks;

public abstract class Entity<TId> where TId : notnull
{
    public TId Id { get; protected set; }

    public override bool Equals(object? obj)
        => obj is not null &&
           obj is Entity<TId> entity &&
           obj.GetType() == GetType() &&
           Id is not null &&
           Id.Equals(entity.Id);

    public static bool operator ==(Entity<TId> left, Entity<TId> right)
        => left is null && right is null || left is not null && left.Equals(right);

    public static bool operator !=(Entity<TId> left, Entity<TId> right)
        => !(left == right);
    public override int GetHashCode()
        => HashCode.Combine(GetType(), Id);
}