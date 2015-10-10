using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using NWheels.Concurrency;

namespace NWheels.Entities.Core
{
    public abstract class UnitOfWorkDataRepositoryBase : DataRepositoryBase
    {
        private readonly Dictionary<IEntityId, IEntityObject> _entityCache;
        private readonly HashSet<IEntityObject> _insertBatch;
        private readonly HashSet<IEntityObject> _updateBatch;
        private readonly HashSet<IEntityObject> _deleteBatch;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected UnitOfWorkDataRepositoryBase(IResourceConsumerScopeHandle consumerScope, IComponentContext components, bool autoCommit)
            : base(consumerScope, components, autoCommit)
        {
            _entityCache = new Dictionary<IEntityId, IEntityObject>();
            _insertBatch = new HashSet<IEntityObject>();
            _updateBatch = new HashSet<IEntityObject>();
            _deleteBatch = new HashSet<IEntityObject>();

            BeginLifetimeScope();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void Dispose()
        {
            bool shouldDisposeResourcesNow;
            DisposeConsumerScope(out shouldDisposeResourcesNow);

            if ( shouldDisposeResourcesNow )
            {
                EndLifetimeScope();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void TrackEntity<TEntityContract>(ref TEntityContract entity, EntityState state)
        {
            var key = ((IEntityObject)entity).GetId();

            if ( !_entityCache.ContainsKey(key) )
            {
                _entityCache.Add(key, (IEntityObject)entity);
                NotifyEntityState((IEntityObject)entity, state);
            }
            else
            {
                entity = (TEntityContract)_entityCache[key];
            }
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

        protected override IEnumerable<IEntityObject> GetCurrentChangeSet()
        {
            return _insertBatch.Concat(_updateBatch).Concat(_deleteBatch);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected HashSet<IEntityObject> InsertBatch
        {
            get { return _insertBatch; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected HashSet<IEntityObject> UpdateBatch
        {
            get { return _updateBatch; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected HashSet<IEntityObject> DeleteBatch
        {
            get { return _deleteBatch; }
        }
    }
}
