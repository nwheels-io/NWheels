using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using NWheels.Concurrency;
using NWheels.Entities;
using Autofac;
using NWheels.DataObjects;
using NWheels.Entities.Core;

namespace NWheels.Core
{
    public class UnitOfWorkFactory
    {
        private readonly ConcurrentDictionary<Type, FactoryByContract> _unitOfWorkFactoryPerRepositoryContract;
        private readonly IComponentContext _components;
        private readonly ITypeMetadataCache _metadataCache;
        private IDomainContextLogger _domainContextLogger;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public UnitOfWorkFactory(IComponentContext components)
        {
            _components = components;
            _unitOfWorkFactoryPerRepositoryContract = new ConcurrentDictionary<Type, FactoryByContract>();
            _metadataCache = components.Resolve<ITypeMetadataCache>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TRepository NewUnitOfWork<TRepository>(bool autoCommit, UnitOfWorkScopeOption? scopeOption = null, string connectionString = null) 
            where TRepository : class, IApplicationDataRepository
        {
            var concretized = _metadataCache.Concretize(typeof(TRepository));

            if ( concretized != typeof(TRepository) )
            {
                var factoryByContract = _unitOfWorkFactoryPerRepositoryContract.GetOrAdd(
                    concretized,
                    key => FactoryByContract.Create(key, this));

                return (TRepository)factoryByContract.NewUnitOfWork(autoCommit, scopeOption, connectionString);
            }

            var consumerScope = new ThreadStaticResourceConsumerScope<TRepository>(
                resourceFactory: scope => {
                    var factory = _components.Resolve<IDataRepositoryFactory>();
                    var repositoryInstance = factory.NewUnitOfWork(scope, typeof(TRepository), autoCommit, scopeOption, connectionString);
                    return (TRepository)repositoryInstance;
                },
                externallyOwned: true,
                forceNewResource: scopeOption == UnitOfWorkScopeOption.Root);

            if ( !consumerScope.IsOutermost )
            {
                Logger.NewNestedUnitOfWork(domainContext: consumerScope.Resource.ToString());
            }

            return consumerScope.Resource;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IApplicationDataRepository NewUnitOfWork(
            Type repositoryContractType, bool autoCommit = true, UnitOfWorkScopeOption? scopeOption = null, string connectionString = null)
        {
            var factoryByContract = _unitOfWorkFactoryPerRepositoryContract.GetOrAdd(
                repositoryContractType, 
                key => FactoryByContract.Create(key, this));

            return factoryByContract.NewUnitOfWork(autoCommit, scopeOption, connectionString);
        }
        
        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private IDomainContextLogger Logger
        {
            get
            {
                if ( _domainContextLogger == null )
                {
                    _domainContextLogger = _components.Resolve<IDomainContextLogger>();
                }

                return _domainContextLogger;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private abstract class FactoryByContract
        {
            public abstract IApplicationDataRepository NewUnitOfWork(bool autoCommit = true, UnitOfWorkScopeOption? scopeOption = null, string connectionString = null);

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

            public override IApplicationDataRepository NewUnitOfWork(bool autoCommit = true, UnitOfWorkScopeOption? scopeOption = null, string connectionString = null)
            {
                return _ownerFactory.NewUnitOfWork<TRepository>(autoCommit, scopeOption, connectionString);
            }

            #endregion
        }
    }
}
