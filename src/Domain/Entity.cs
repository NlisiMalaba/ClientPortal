using Shared;

namespace Domain;

public abstract class Entity<TId> : IEquatable<Entity<TId>>
    where TId : notnull
{
    private TId _id = default!;

    protected Entity()
    {
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    protected Entity(TId id)
        : this()
    {
        Id = id;
    }

    public TId Id
    {
        get => _id;
        protected init
        {
            _id = ValidateId(value);
        }
    }

    public DateTime CreatedAt { get; protected init; }

    public DateTime UpdatedAt { get; protected set; }

    protected void MarkUpdated()
    {
        UpdatedAt = DateTime.UtcNow;
    }

    public bool Equals(Entity<TId>? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return GetType() == other.GetType() && EqualityComparer<TId>.Default.Equals(Id, other.Id);
    }

    public override bool Equals(object? obj)
    {
        return obj is Entity<TId> other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(GetType(), Id);
    }

    public static bool operator ==(Entity<TId>? left, Entity<TId>? right)
    {
        return left is null ? right is null : left.Equals(right);
    }

    public static bool operator !=(Entity<TId>? left, Entity<TId>? right)
    {
        return !(left == right);
    }

    private static TId ValidateId(TId id)
    {
        if (id is string stringId)
        {
            return (TId)(object)Guard.NotEmpty(stringId, nameof(id));
        }

        if (id is ValueType && EqualityComparer<TId>.Default.Equals(id, default!))
        {
            throw new ArgumentException("Value cannot be default.", nameof(id));
        }

        return id;
    }
}
