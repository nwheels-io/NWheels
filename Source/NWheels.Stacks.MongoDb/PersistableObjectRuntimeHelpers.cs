using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Entities.Core;
using NWheels.Stacks.MongoDb.LazyLoaders;

namespace NWheels.Stacks.MongoDb
{
    public static class PersistableObjectRuntimeHelpers
    {
        public static TId ImportPersistableLazyLoadObject<TId>(object importValue)
        {
            var domainObject = importValue as IDomainObject;
            var lazyLoaderById = importValue as ObjectLazyLoaderById;

            if ( domainObject != null )
            {
                return (TId)domainObject.GetId().Value;
            }

            if ( lazyLoaderById != null )
            {
                return (TId)lazyLoaderById.EntityId;
            }

            return default(TId);
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public static TId? ImportPersistableLazyLoadObjectNullable<TId>(object importValue)
            where TId : struct
        {
            var domainObject = importValue as IDomainObject;
            var lazyLoaderById = importValue as ObjectLazyLoaderById;

            if ( domainObject != null )
            {
                return (TId)domainObject.GetId().Value;
            }

            if ( lazyLoaderById != null )
            {
                return (TId)lazyLoaderById.EntityId;
            }

            return null;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public static TId[] ImportPersistableLazyLoadObjectCollection<TId>(object importValue)
        {
            var domainCollection = importValue as System.Collections.IEnumerable;
            var lazyLoaderByIdList = importValue as ObjectCollectionLazyLoaderById<TId>;

            if ( domainCollection != null )
            {
                return domainCollection.Cast<IDomainObject>().Select(obj => (TId)obj.GetId().Value).ToArray();
            }

            if ( lazyLoaderByIdList != null )
            {
                return lazyLoaderByIdList.DocumentIds;
            }

            return null;
        }
    }
}
