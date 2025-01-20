namespace Wallet.BuildingBlocks.Domain;

public abstract record ValueObject<T> : IEquatable<T> where T : ValueObject<T>
{
    protected abstract IEnumerable<object> GetEqualityComponents();

    public virtual bool Equals(T? obj)
    {
        if (obj is null) return false; 
        if (ReferenceEquals(this, obj)) return true;
        
        return obj?.GetType() == GetType() &&
               GetEqualityComponents().SequenceEqual(obj.GetEqualityComponents());
    }

    public override int GetHashCode()
        => GetEqualityComponents()
            .Select(x => x?.GetHashCode() ?? 0)
            .Aggregate((x, y) => x ^ y);
    
}