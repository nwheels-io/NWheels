using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using NWheels.Concurrency;
using NWheels.Entities;
using Autofac;

namespace NWheels.Core
{
    public class UnitOfWorkFactory
    {
        private readonly ConcurrentDictionary<Type, FactoryByContract> _unitOfWorkFactoryPerRepositoryContract;
        private readonly IComponentContext _components;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public UnitOfWorkFactory(IComponentContext components)
        {
            _components = components;
            _unitOfWorkFactoryPerRepositoryContract = new ConcurrentDictionary<Type, FactoryByContract>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TRepository NewUnitOfWork<TRepository>(bool autoCommit, IsolationLevel? isolationLevel = null) where TRepository : class, IApplicationDataRepository
        {
            var consumerScope = new ThreadStaticResourceConsumerScope<TRepository>(
                resourceFactory: scope => {
                    var factory = _components.Resolve<IDataRepositoryFactory>();
                    var repositoryInstance = factory.NewUnitOfWork(scope, typeof(TRepository), autoCommit, isolationLevel);
                    return (TRepository)repositoryInstance;
                },
                externallyOwned: true);

            return consumerScope.Resource;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IApplicationDataRepository NewUnitOfWork(Type repositoryContractType, bool autoCommit = true, IsolationLevel? isolationLevel = null)
        {
            var factoryByContract = _unitOfWorkFactoryPerRepositoryContract.GetOrAdd(
                repositoryContractType, 
                key => FactoryByContract.Create(key, this));

            return factoryByContract.NewUnitOfWork(autoCommit, isolationLevel);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private abstract class FactoryByContract
        {
            public abstract IApplicationDataRepository NewUnitOfWork(bool autoCommit = true, IsolationLevel? isolationLevel = null);

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public static FactoryByContract Create(Type repositoryContractType, UnitOfWorkFactory ownerFactory)
            {
                var closedFactoryType = typeof(FactoryByContract<>).MakeGenericType(repositoryContractType);
                return (FactoryByContract)Activator.CreateInstance(closedFactoryType, ownerFactory);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        private class FactoryByContract<TRepository> : FactoryByContract
            where TRepository : class, IApplicationDataRepository
        {
            private readonly UnitOfWorkFactory _ownerFactory;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public FactoryByContract(UnitOfWorkFactory ownerFactory)
            {
                _ownerFactory = ownerFactory;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            #region Overrides of FactoryByContract

            public override IApplicationDataRepository NewUnitOfWork(bool autoCommit = true, IsolationLevel? isolationLevel = null)
            {
                return _ownerFactory.NewUnitOfWork<TRepository>(autoCommit, isolationLevel);
            }

            #endregion
        }
    }
}
