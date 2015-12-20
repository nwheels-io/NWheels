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
        private readonly Dictionary<IEntityId, IDomainObject> _entityCache;
        private readonly HashSet<IDomainObject> _insertBatch;
        private readonly HashSet<IDomainObject> _updateBatch;
        private readonly HashSet<IDomainObject> _saveBatch;
        private readonly HashSet<IDomainObject> _deleteBatch;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected UnitOfWorkDataRepositoryBase(IResourceConsumerScopeHandle consumerScope, IComponentContext components, bool autoCommit)
            : base(consumerScope, components, autoCommit)
        {
            _entityCache = new Dictionary<IEntityId, IDomainObject>();
            _insertBatch = new HashSet<IDomainObject>();
            _updateBatch = new HashSet<IDomainObject>();
            _saveBatch = new HashSet<IDomainObject>();
            _deleteBatch = new HashSet<IDomainObject>();

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

        public void TrackEntity(ref IDomainObject entity, EntityState state)
        {
            var key = entity.GetId();

            if ( key.Value == null )
            {
                NotifyEntityState(entity, EntityState.NewPristine);
                return;
            }

            if ( !_entityCache.ContainsKey(key) )
            {
                _entityCache.Add(key, entity);
                NotifyEntityState(entity, state);
            }
            else
            {
                entity = _entityCache[key];
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void NotifyEntityState(IDomainObject entity, EntityState state)
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

        public void SaveEntity(IDomainObject entity)
        {
            _saveBatch.Add(entity);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool TryGetFromCache<TEntityContract, TIdValue>(TIdValue idValue, out TEntityContract entity)
        {
            var key = new EntityId<TEntityContract, TIdValue>(idValue);
            IDomainObject entityObject;

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

        protected override IEnumerable<IDomainObject> GetCurrentChangeSet()
        {
            return _insertBatch.Concat(_updateBatch).Concat(_saveBatch).Concat(_deleteBatch);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected HashSet<IDomainObject> InsertBatch
        {
            get { return _insertBatch; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected HashSet<IDomainObject> UpdateBatch
        {
            get { return _updateBatch; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected HashSet<IDomainObject> SaveBatch
        {
            get { return _saveBatch; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected HashSet<IDomainObject> DeleteBatch
        {
            get { return _deleteBatch; }
        }
    }
}
