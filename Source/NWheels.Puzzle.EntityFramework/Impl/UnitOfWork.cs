using System;
using System.Collections.Generic;
using System.Data.Entity.Core.EntityClient;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Entities;

namespace NWheels.Puzzle.EntityFramework.Impl
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly EntityConnection _entityConnection;
        private readonly bool _autoCommit;
        private ObjectContext _objectContext;
        private UnitOfWorkState _currentState;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public UnitOfWork(EntityConnection connection, bool autoCommit)
        {
            _entityConnection = connection;
            _objectContext = null;//new ObjectContext(connection);
            _autoCommit = autoCommit;
            _currentState = UnitOfWorkState.Pristine;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Dispose()
        {
            _objectContext.Dispose();
            _entityConnection.Dispose();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void CommitChanges()
        {
            ValidateState(UnitOfWorkState.Pristine, UnitOfWorkState.Dirty);
            _objectContext.SaveChanges(SaveOptions.AcceptAllChangesAfterSave);
            _currentState = UnitOfWorkState.Committed;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void RollbackChanges()
        {
            ValidateState(UnitOfWorkState.Pristine, UnitOfWorkState.Dirty);
            _objectContext.Dispose();
            _entityConnection.Dispose();
            _currentState = UnitOfWorkState.RolledBack;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void ValidateOperationalState()
        {
            ValidateState(UnitOfWorkState.Pristine, UnitOfWorkState.Dirty);
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

        internal ObjectContext ObjectContext
        {
            get
            {
                return _objectContext;
            }
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
