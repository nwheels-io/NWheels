using System;
using System.Data.Common;
using System.Linq;

namespace NWheels.Entities.Core
{
    public abstract class DataRepositoryBase : IApplicationDataRepository
    {
        private readonly bool _autoCommit;
        private UnitOfWorkState _currentState;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected DataRepositoryBase(bool autoCommit)
        {
            _autoCommit = autoCommit;
            _currentState = UnitOfWorkState.Untouched;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual void Dispose()
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public abstract Type[] GetEntityTypesInRepository();

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
