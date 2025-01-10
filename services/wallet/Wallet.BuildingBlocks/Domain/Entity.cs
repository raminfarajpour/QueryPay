namespace Wallet.BuildingBlocks.Domain;

public abstract class Entity<TId> where TId : notnull
{
    public TId Id { get; }

    public override bool Equals(object? obj)
        => obj is Entity<TId> entity && Id.Equals(entity.Id);

    public static bool operator ==(Entity<TId>? left, Entity<TId>? right)
        => left?.Equals(right) ?? right is null;

    public static bool operator !=(Entity<TId>? left, Entity<TId>? right)
        => !(left == right);

    public override int GetHashCode()
        => Id.GetHashCode();
}