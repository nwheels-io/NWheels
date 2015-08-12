using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using NWheels.Concurrency;
using NWheels.Entities;
using NWheels.Entities.Core;

namespace NWheels.Testing.Entities.Impl
{
    public abstract class TestDataRepositoryBase : DataRepositoryBase
    {
        protected TestDataRepositoryBase(IResourceConsumerScopeHandle consumerScope, IComponentContext components, bool autoCommit)
            : base(consumerScope, components, autoCommit)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void ResetState()
        {
            base.ResetCurrentState(UnitOfWorkState.Untouched);
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
