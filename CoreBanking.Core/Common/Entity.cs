namespace CoreBanking.Core.Common
{
    public abstract class Entity
    {
        public DateTime DateCreated { get; protected set; }
        public DateTime DateUpdated { get; protected set; }

        protected Entity()
        {
            DateCreated = DateTime.UtcNow;
            DateUpdated = DateTime.UtcNow;
        }

        protected void UpdateTimestamp()
        {
            DateUpdated = DateTime.UtcNow;
        }

        // Equality comparison based on identity (to be overridden by derived classes)
        public override bool Equals(object obj)
        {
            if (obj is not Entity other)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            // Derived classes should implement proper identity comparison
            return GetType() == other.GetType() && GetIdentity() == other.GetIdentity();
        }

        public override int GetHashCode()
        {
            return GetIdentity()?.GetHashCode() ?? 0;
        }

        public static bool operator ==(Entity a, Entity b)
        {
            if (a is null && b is null)
                return true;

            if (a is null || b is null)
                return false;

            return a.Equals(b);
        }

        public static bool operator !=(Entity a, Entity b)
        {
            return !(a == b);
        }

        // Abstract method to force derived classes to define identity
        protected abstract object GetIdentity();
    }

    // Generic version for entities with typed IDs
    public abstract class Entity<TId> : Entity
    {
        public TId Id { get; protected set; }

        protected override object GetIdentity()
        {
            return Id;
        }
    }
}