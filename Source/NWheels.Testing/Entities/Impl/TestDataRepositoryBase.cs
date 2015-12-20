using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using NWheels.Concurrency;
using NWheels.Conventions.Core;
using NWheels.Entities;
using NWheels.Entities.Core;
using NWheels.Extensions;

namespace NWheels.Testing.Entities.Impl
{
    public abstract class TestDataRepositoryBase : DataRepositoryBase
    {
        private readonly IEntityObjectFactory _entityObjectFactory;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected TestDataRepositoryBase(
            IResourceConsumerScopeHandle consumerScope, 
            IComponentContext components, 
            bool autoCommit)
            : base(consumerScope, components, autoCommit)
        {
            _entityObjectFactory = components.Resolve<IEntityObjectFactory>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void ResetState()
        {
            base.ResetCurrentState(UnitOfWorkState.Untouched);

        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override IEntityObjectFactory PersistableObjectFactory 
        {
            get
            {
                return _entityObjectFactory;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override IEnumerable<IDomainObject> GetCurrentChangeSet()
        {
            IEnumerable<IDomainObject> changeSet = new IDomainObject[0];

            foreach ( var entityRepo in GetEntityRepositories().Where(repo => repo != null) )
            {
                var storedEntityObjects = ((System.Collections.IEnumerable)entityRepo).Cast<object>()
                    .Cast<IDomainObject>()
                    .ToArray();
                
                var changedEntityObjects = storedEntityObjects.Where(e => e.State != EntityState.RetrievedPristine);
                changeSet = changeSet.Concat(changedEntityObjects);
            }

            return changeSet;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnCommitChanges()
        {

        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnRollbackChanges()
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static TRepository ResetState<TRepository>(TRepository repository)
            where TRepository : IApplicationDataRepository
        {
            ((TestDataRepositoryBase)(object)repository).ResetState();
            return repository;
        }
    }
}
