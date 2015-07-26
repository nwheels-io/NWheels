using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Extensions;

namespace NWheels.Entities
{
    public enum EntityState
    {
        NewPristine,
        NewModified,
        NewDeleted,
        RetrievedPristine,
        RetrievedModified,
        RetrievedDeleted
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public static class EntityStateExtensions
    {
        public static bool IsPristine(this EntityState state)
        {
            return state.IsIn(EntityState.NewPristine, EntityState.RetrievedPristine);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static bool IsModified(this EntityState state)
        {
            return state.IsIn(EntityState.NewModified, EntityState.RetrievedModified);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static bool IsDeleted(this EntityState state)
        {
            return state.IsIn(EntityState.NewDeleted, EntityState.RetrievedDeleted);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static bool IsNew(this EntityState state)
        {
            return state.IsIn(EntityState.NewPristine, EntityState.NewModified, EntityState.NewDeleted);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static bool IsRetrieved(this EntityState state)
        {
            return state.IsIn(EntityState.RetrievedPristine, EntityState.RetrievedModified, EntityState.RetrievedDeleted);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static EntityState SetModified(this EntityState state)
        {
            switch ( state )
            {
                case EntityState.NewPristine:
                    return EntityState.NewModified;
                case EntityState.RetrievedPristine:
                    return EntityState.RetrievedModified;
                default:
                    return state;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static EntityState SetDeleted(this EntityState state)
        {
            switch ( state )
            {
                case EntityState.NewPristine:
                case EntityState.NewModified:
                    return EntityState.NewDeleted;
                case EntityState.RetrievedPristine:
                case EntityState.RetrievedModified:
                    return EntityState.RetrievedDeleted;
                default:
                    return state;
            }
        }
    }
}
