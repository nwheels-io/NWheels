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

namespace NWheels.Client
{
    public abstract class ClientDataRepositoryBase : DataRepositoryBase
    {
        private readonly IEntityObjectFactory _entityObjectFactory;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected ClientDataRepositoryBase(
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

        protected override IEnumerable<IEntityObject> GetCurrentChangeSet()
        {
            IEnumerable<IEntityObject> changeSet = new IEntityObject[0];

            foreach (var entityRepo in GetEntityRepositories().Where(repo => repo != null))
            {
                var storedEntityObjects = ((System.Collections.IEnumerable)entityRepo).Cast<object>()
                    .Select(obj => obj.As<IPersistableObject>())
                    .Cast<IEntityObject>()
                    .ToArray();

                var changedEntityObjects = storedEntityObjects.Where(e => e.As<IDomainObject>().State != EntityState.RetrievedPristine);
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
            ((ClientDataRepositoryBase)(object)repository).ResetState();
            return repository;
        }
    }
}
