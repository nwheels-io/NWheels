using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;

namespace NWheels.Entities.Core
{
    public abstract class UnitOfWorkDataRepositoryBase : DataRepositoryBase
    {
        private readonly Dictionary<IEntityId, IEntityObject> _entityCache;
        private readonly HashSet<IEntityObject> _insertBatch;
        private readonly HashSet<IEntityObject> _updateBatch;
        private readonly HashSet<IEntityObject> _deleteBatch;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected UnitOfWorkDataRepositoryBase(IComponentContext components, bool autoCommit)
            : base(components, autoCommit)
        {
            _entityCache = new Dictionary<IEntityId, IEntityObject>();
            _insertBatch = new HashSet<IEntityObject>();
            _updateBatch = new HashSet<IEntityObject>();
            _deleteBatch = new HashSet<IEntityObject>();

            base.BeginLifetimeScope();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void Dispose()
        {
            base.EndLifetimeScope();
            base.Dispose();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void TrackEntity<TEntityContract>(ref TEntityContract entity, EntityState state)
        {
            var key = ((IEntityObject)entity).GetId();

            if ( !_entityCache.ContainsKey(key) )
            {
                _entityCache.Add(key, (IEntityObject)entity);
            }
            else
            {
                entity = (TEntityContract)_entityCache[key];
            }

            NotifyEntityState((IEntityObject)entity, state);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void NotifyEntityState(IEntityObject entity, EntityState state)
        {
            switch ( state )
            {
                case EntityState.NewPristine:
                case EntityState.NewModified:
                    _insertBatch.Add(entity);
                    break;
                case EntityState.RetrievedModified:
                    _updateBatch.Add(entity);
                    break;
                case EntityState.RetrievedDeleted:
                    _deleteBatch.Add(entity);
                    break;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool TryGetFromCache<TEntityContract, TIdValue>(TIdValue idValue, out TEntityContract entity)
        {
            var key = new EntityId<TEntityContract, TIdValue>(idValue);
            IEntityObject entityObject;

            if ( _entityCache.TryGetValue(key, out entityObject) )
            {
                entity = (TEntityContract)entityObject;
                return true;
            }
            else
            {
                entity = default(TEntityContract);
                return false;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected HashSet<IEntityObject> InsertQueue
        {
            get { return _insertBatch; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected HashSet<IEntityObject> UpdateQueue
        {
            get { return _updateBatch; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected HashSet<IEntityObject> DeleteQueue
        {
            get { return _deleteBatch; }
        }
    }
}
