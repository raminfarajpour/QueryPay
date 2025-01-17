namespace Billing.Domain.SeedWorks;

public abstract class ValueObject<T> where T : ValueObject<T>
{
    protected abstract IEnumerable<object>? GetEqualityComponents();

    public override bool Equals(object? obj)
        => obj is T valueObject &&
           obj.GetType() == GetType() &&
           GetEqualityComponents().SequenceEqual(valueObject.GetEqualityComponents());

    public static bool operator ==(ValueObject<T>? left, ValueObject<T>? right)
    {
        if (ReferenceEquals(left, right)) return true;
        if (left is null || right is null) return false;

        return left.Equals(right);
    }

    public static bool operator !=(ValueObject<T>? left, ValueObject<T>? right)
        => !(left == right);

    public override int GetHashCode()
        => GetEqualityComponents()
                .Select(x => x?.GetHashCode() ?? 0)
                .Aggregate((x, y) => x ^ y);
}
