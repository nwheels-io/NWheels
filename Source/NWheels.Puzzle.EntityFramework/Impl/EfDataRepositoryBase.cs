using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration;
using System.Data.Entity.ModelConfiguration.Configuration;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using NWheels.Entities;
using NWheels.Puzzle.EntityFramework.Conventions;
using System.Reflection;
using Autofac;
using NWheels.DataObjects;

namespace NWheels.Puzzle.EntityFramework.Impl
{
    public abstract class EfDataRepositoryBase : IApplicationDataRepository
    {
        private readonly DbCompiledModel _compiledModel;
        private readonly bool _autoCommit;
        private readonly DbConnection _connection;
        private ObjectContext _objectContext;
        private UnitOfWorkState _currentState;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected EfDataRepositoryBase(DbCompiledModel compiledModel, DbConnection connection, bool autoCommit)
        {
            _compiledModel = compiledModel;
            _autoCommit = autoCommit;
            _connection = connection;
            _objectContext = compiledModel.CreateObjectContext<ObjectContext>(connection);
            _currentState = UnitOfWorkState.Untouched;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Dispose()
        {
            _objectContext.Dispose();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public abstract Type[] GetEntityTypesInRepository();

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void CommitChanges()
        {
            ValidateState(UnitOfWorkState.Untouched, UnitOfWorkState.Dirty);
            _objectContext.SaveChanges(SaveOptions.AcceptAllChangesAfterSave);
            _currentState = UnitOfWorkState.Committed;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void RollbackChanges()
        {
            ValidateState(UnitOfWorkState.Untouched, UnitOfWorkState.Dirty);
            _objectContext.Dispose();
            _connection.Dispose();
            _currentState = UnitOfWorkState.RolledBack;
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

        public DbCompiledModel CompiledModel
        {
            get
            {
                return _compiledModel;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal void ValidateOperationalState()
        {
            ValidateState(UnitOfWorkState.Untouched, UnitOfWorkState.Dirty);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal ObjectSet<T> CreateObjectSet<T>() where T : class
        {
            return _objectContext.CreateObjectSet<T>();
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

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static Expression<Func<TEntityImpl, TProperty>> CreatePropertyExpression<TEntityImpl, TProperty>(MethodInfo getterMethod)
        {
            var parameter = Expression.Parameter(typeof(TEntityImpl), "e");
            return Expression.Lambda<Func<TEntityImpl, TProperty>>(Expression.Property(parameter, getterMethod), new[] { parameter });
        }
    }
}
