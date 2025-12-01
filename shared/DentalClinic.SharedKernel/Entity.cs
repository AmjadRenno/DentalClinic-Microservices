namespace DentalClinic.SharedKernel;

public abstract class Entity : IEquatable<Entity>
{
    public Guid Id { get; init; }

    protected Entity(Guid id)
    {
        Id = id;
    }

    public bool Equals(Entity? other) => other is not null && other.Id == Id;
    public override bool Equals(object? obj) => obj is Entity other && Equals(other);
    public override int GetHashCode() => Id.GetHashCode();

    public static bool operator ==(Entity? left, Entity? right) => Equals(left, right);
    public static bool operator !=(Entity? left, Entity? right) => !Equals(left, right);
}
