using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Autofac;
using NWheels.Concurrency;
using NWheels.Extensions;

namespace NWheels.Entities.Core
{
    public abstract class DataRepositoryBase : IApplicationDataRepository
    {
        private readonly Dictionary<Type, IEntityRepository> _entityRepositoryByContractType;
        private readonly Dictionary<Type, Action<IDataRepositoryCallback>> _genericCallbacksByContractType;
        private readonly IComponentContext _components;
        private readonly bool _autoCommit;
        private readonly IResourceConsumerScopeHandle _consumerScope;
        private UnitOfWorkState _currentState;
        private ILifetimeScope _componentLifetimeScope;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected DataRepositoryBase(IResourceConsumerScopeHandle consumerScope, IComponentContext components, bool autoCommit)
        {
            _entityRepositoryByContractType = new Dictionary<Type, IEntityRepository>();
            _genericCallbacksByContractType = new Dictionary<Type, Action<IDataRepositoryCallback>>();
            _autoCommit = autoCommit;
            _components = components;
            _currentState = UnitOfWorkState.Untouched;
            _componentLifetimeScope = null;
            _consumerScope = consumerScope;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual void Dispose()
        {
            bool shouldDisposeResourcesNow;
            DisposeConsumerScope(out shouldDisposeResourcesNow);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void InvokeGenericOperation(Type entityContractType, IDataRepositoryCallback callback)
        {
            _genericCallbacksByContractType[entityContractType](callback);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        public abstract Type[] GetEntityContractsInRepository();
        public abstract Type[] GetEntityTypesInRepository();
        public abstract IEntityRepository[] GetEntityRepositories();

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void CommitChanges()
        {
            if ( _consumerScope == null || _consumerScope.IsInnermost )
            {
                ValidateState(UnitOfWorkState.Untouched, UnitOfWorkState.Dirty);
                OnCommitChanges();
                _currentState = UnitOfWorkState.Committed;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void RollbackChanges()
        {
            if ( _consumerScope == null || _consumerScope.IsInnermost )
            {
                ValidateState(UnitOfWorkState.Untouched, UnitOfWorkState.Dirty);
                OnRollbackChanges();
                _currentState = UnitOfWorkState.RolledBack;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void ValidateOperationalState()
        {
            ValidateState(UnitOfWorkState.Untouched, UnitOfWorkState.Dirty);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void RegisterEntityRepository<TEntityContract, TEntityImpl>(IEntityRepository<TEntityContract> repo)
        {
            _entityRepositoryByContractType.Add(typeof(TEntityContract), (IEntityRepository)repo); 
            _genericCallbacksByContractType.Add(typeof(TEntityContract), callback => callback.Invoke<TEntityContract, TEntityImpl>(repo));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IEntityRepository GetEntityRepository(Type contractType)
        {
            IEntityRepository repository;

            if ( _entityRepositoryByContractType.TryGetValue(contractType, out repository) )
            {
                return repository;
            }

            foreach ( var baseContractType in contractType.GetInterfaces().Where(intf => intf.IsEntityContract()) )
            {
                if ( _entityRepositoryByContractType.TryGetValue(baseContractType, out repository) )
                {
                    return repository;
                }
            }

            throw new KeyNotFoundException("Entity repository for contract '" + contractType.FullName + "' could not be found in the data repository.");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool IsAutoCommitMode
        {
            get
            {
                return _autoCommit;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public UnitOfWorkState UnitOfWorkState
        {
            get
            {
                return _currentState;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IComponentContext Components
        {
            get
            {
                return (_componentLifetimeScope ?? _components);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected abstract void OnCommitChanges();
        protected abstract void OnRollbackChanges();

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected void DisposeConsumerScope(out bool shouldDisposeResourcesNow)
        {
            try
            {
                if ( _consumerScope == null || _consumerScope.IsOutermost )
                {
                    if ( _currentState == UnitOfWorkState.Dirty )
                    {
                        if ( _autoCommit )
                        {
                            CommitChanges();
                        }
                        else
                        {
                            RollbackChanges();
                        }
                    }

                    shouldDisposeResourcesNow = true;
                }
                else
                {
                    shouldDisposeResourcesNow = false;
                }
            }
            finally
            {
                if ( _consumerScope != null )
                {
                    _consumerScope.Outermost.Dispose();
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected void ResetCurrentState(UnitOfWorkState newState)
        {
            _currentState = newState;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected void BeginLifetimeScope()
        {
            if ( _componentLifetimeScope != null )
            {
                throw new InvalidOperationException("Component lifetime scope already initialized.");
            }

            _componentLifetimeScope = ((ILifetimeScope)_components).BeginLifetimeScope(builder => {
                builder
                    .RegisterInstance(this)
                    .As(typeof(DataRepositoryBase), this.GetType());
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected void EndLifetimeScope()
        {
            if ( _componentLifetimeScope == null)
            {
                throw new InvalidOperationException("Component lifetime scope not initialized.");
            }

            _componentLifetimeScope.Dispose();
            _componentLifetimeScope = null;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void ValidateState(params UnitOfWorkState[] allowedStates)
        {
            if ( !allowedStates.Contains(_currentState) )
            {
                throw new InvalidOperationException("Operation cannot be performed when unit of work is in the state: " + _currentState);
            }
        }
    }
}
