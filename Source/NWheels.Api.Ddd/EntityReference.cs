using System;

namespace NWheels.Api.Ddd
{
    public abstract class EntityReference : IEquatable<EntityReference>
    {
        bool IEquatable<EntityReference>.Equals(EntityReference other)
        {
            return this.Equals(other);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static bool operator == (EntityReference a, EntityReference b)
        {
            if (ReferenceEquals(a, null))
            {
                return ReferenceEquals(b, null);
            }
            
            return a.Equals(b);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static bool operator != (EntityReference a, EntityReference b)
        {
            return !(a == b);
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IEntityReference<out TEntity, out TId> 
        where TEntity : class 
    {
        TId Id { get; }
        TEntity Instance { get; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public abstract class EntityReference<TEntity, TId> : EntityReference, IEntityReference<TEntity, TId>
        where TEntity : class
    {
        public override bool Equals (object obj)
        {
            var other = obj as EntityReference<TEntity, TId>;

            if (other != null)
            {
                return this.Id.Equals(other.Id);
            }

            return false;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual TId Id { get; protected set; }
        public virtual TEntity Instance { get; protected set; }
    }
}
