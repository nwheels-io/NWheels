using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Autofac;

namespace NWheels.Entities.Core
{
    public abstract class DataRepositoryBase : IApplicationDataRepository
    {
        private readonly Dictionary<Type, Action<IDataRepositoryCallback>> _genericCallbacksByContractType;
        private readonly IComponentContext _components;
        private readonly bool _autoCommit;
        private UnitOfWorkState _currentState;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected DataRepositoryBase(IComponentContext components, bool autoCommit)
        {
            _genericCallbacksByContractType = new Dictionary<Type, Action<IDataRepositoryCallback>>();
            _autoCommit = autoCommit;
            _components = components;
            _currentState = UnitOfWorkState.Untouched;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual void Dispose()
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void InvokeGenericOperation(Type contractType, IDataRepositoryCallback callback)
        {
            _genericCallbacksByContractType[contractType](callback);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        public abstract Type[] GetEntityContractsInRepository();
        public abstract Type[] GetEntityTypesInRepository();
        public abstract IEntityRepository[] GetEntityRepositories();

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void CommitChanges()
        {
            ValidateState(UnitOfWorkState.Untouched, UnitOfWorkState.Dirty);
            OnCommitChanges();
            _currentState = UnitOfWorkState.Committed;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void RollbackChanges()
        {
            ValidateState(UnitOfWorkState.Untouched, UnitOfWorkState.Dirty);
            OnRollbackChanges();
            _currentState = UnitOfWorkState.RolledBack;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void ValidateOperationalState()
        {
            ValidateState(UnitOfWorkState.Untouched, UnitOfWorkState.Dirty);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void RegisterEntityRepository<TEntityContract, TEntityImpl>(IEntityRepository<TEntityContract> repo)
        {
            _genericCallbacksByContractType.Add(typeof(TEntityContract), callback => callback.Invoke<TEntityContract, TEntityImpl>(repo));
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
                return _components;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected abstract void OnCommitChanges();
        protected abstract void OnRollbackChanges();

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected void ResetCurrentState(UnitOfWorkState newState)
        {
            _currentState = newState;
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
