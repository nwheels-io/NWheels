using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Autofac;
using NWheels.Concurrency;
using NWheels.DataObjects.Core;
using NWheels.Extensions;
using NWheels.Logging;

namespace NWheels.Entities.Core
{
    public abstract class DataRepositoryBase : IApplicationDataRepository
    {
        private readonly Dictionary<Type, IEntityRepository> _entityRepositoryByContractType;
        private readonly Dictionary<Type, Action<IDataRepositoryCallback>> _genericCallbacksByContractType;
        private readonly IComponentContext _components;
        private readonly IDomainContextLogger _logger;
        private readonly bool _autoCommit;
        private readonly IResourceConsumerScopeHandle _consumerScope;
        private UnitOfWorkState _currentState;
        private ILifetimeScope _componentLifetimeScope;
        private bool _disposed;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected DataRepositoryBase(IResourceConsumerScopeHandle consumerScope, IComponentContext components, bool autoCommit)
        {
            _entityRepositoryByContractType = new Dictionary<Type, IEntityRepository>();
            _genericCallbacksByContractType = new Dictionary<Type, Action<IDataRepositoryCallback>>();
            _autoCommit = autoCommit;
            _components = components;
            _logger = components.Resolve<IDomainContextLogger>();
            _currentState = UnitOfWorkState.Untouched;
            _componentLifetimeScope = null;
            _consumerScope = consumerScope;
            _disposed = false;
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

                var changeSet = GetCurrentChangeSet().ToArray();

                ExecuteValidationPhase(changeSet);
                ExecuteBeforeSavePhase(changeSet);
                ExecuteCommitToPersistenceLayer();
                ExecuteAfterSavePhase(changeSet);

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

        public IEntityRepository GetEntityRepository(Type entityContractType)
        {
            IEntityRepository repository;

            if ( TryGetEntityRepository(entityContractType, out repository) )
            {
                return repository;
            }

            throw new KeyNotFoundException("Entity repository for contract '" + entityContractType.FullName + "' could not be found in the data repository.");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool TryGetEntityRepository(Type entityContractType, out IEntityRepository entityRepository)
        {
            if ( _entityRepositoryByContractType.TryGetValue(entityContractType, out entityRepository) )
            {
                return true;
            }

            foreach ( var baseContractType in entityContractType.GetInterfaces().Where(intf => intf.IsEntityContract()) )
            {
                if ( _entityRepositoryByContractType.TryGetValue(baseContractType, out entityRepository) )
                {
                    return true;
                }
            }

            return false;
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

        protected abstract IEnumerable<IEntityObject> GetCurrentChangeSet();
        protected abstract void OnCommitChanges();
        protected abstract void OnRollbackChanges();

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected void DisposeConsumerScope(out bool shouldDisposeResourcesNow)
        {
            shouldDisposeResourcesNow = false;

            try
            {
                if ( !_disposed && (_consumerScope == null || _consumerScope.Innermost.IsOutermost) )
                {
                    _logger.EndOfRootUnitOfWork(this.ToString());

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
                    _logger.EndOfNestedUnitOfWork(this.ToString());
                }
            }
            finally
            {
                if ( _consumerScope != null && !_disposed )
                {
                    _consumerScope.Innermost.Dispose();
                }
                
                _disposed |= shouldDisposeResourcesNow;
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

        protected virtual void ExecuteValidationPhase(IEnumerable<object> changedEntities)
        {
            using ( var activity = _logger.ExecutingValidationPhase() )
            {
                try
                {
                    VisitDomainObjects(
                        changedEntities, 
                        predicate: ShouldValidateDomainObject, 
                        action: ValidateDomainObject, 
                        logFactory: obj => _logger.ValidateObject(obj.ToString()));
                }
                catch ( Exception e )
                {
                    activity.Fail(e);
                    throw;
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual void ExecuteBeforeSavePhase(IEnumerable<object> changedEntities)
        {
            using ( var activity = _logger.ExecutingBeforeSavePhase() )
            {
                try
                {
                    VisitDomainObjects(
                        changedEntities, 
                        predicate: null, 
                        action: BeforeSaveDomainObject, 
                        logFactory: obj => _logger.BeforeSaveObject(obj.ToString()));
                }
                catch ( Exception e )
                {
                    activity.Fail(e);
                    throw;
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void ExecuteCommitToPersistenceLayer()
        {
            using ( var activity = _logger.CommittingChangesToPersistenceLayer() )
            {
                try
                {
                    OnCommitChanges();
                }
                catch ( Exception e )
                {
                    activity.Fail(e);
                    throw;
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual void ExecuteAfterSavePhase(IEnumerable<object> changedEntities)
        {
            using ( var activity = _logger.ExecutingAfterSavePhase() )
            {
                try
                {
                    VisitDomainObjects(
                        changedEntities, 
                        predicate: null,
                        action: AfterSaveDomainObject, 
                        logFactory: obj => _logger.AfterSaveObject(obj.ToString()));
                }
                catch ( Exception e )
                {
                    activity.Fail(e);
                    throw;
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected void VisitDomainObjects(
            IEnumerable<object> persistableOrDomainObjects,
            Func<IDomainObject, bool> predicate,
            Action<IDomainObject> action, 
            Func<object, ILogActivity> logFactory)
        {
            foreach ( var domainObject in persistableOrDomainObjects.Select(e => e.As<IDomainObject>()) )
            {
                if ( predicate == null || predicate(domainObject) )
                {
                    VisitSingleDomainObject(domainObject, action, logFactory);

                    var haveNestedObjects = domainObject as IHaveNestedObjects;

                    if ( haveNestedObjects != null )
                    {
                        try
                        {
                            var nestedObjects = new HashSet<object>();
                            haveNestedObjects.DeepListNestedObjects(nestedObjects);

                            foreach ( var nestedDomainObject in 
                                nestedObjects.Select(e => e.As<IDomainObject>()).Where(obj => predicate == null || predicate(obj)) )
                            {
                                VisitSingleDomainObject(nestedDomainObject, action, logFactory);
                            }
                        }
                        catch ( Exception e )
                        {
                            _logger.FailedToVisitNestedObjects(e);
                        }
                    }
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void VisitSingleDomainObject(IDomainObject obj, Action<IDomainObject> action, Func<object, ILogActivity> logFactory)
        {
            using ( var activity = logFactory(obj) )
            {
                try
                {
                    action(obj);
                }
                catch ( Exception e )
                {
                    activity.Fail(e);
                    throw;
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private bool ShouldValidateDomainObject(IDomainObject obj)
        {
            var state = obj.State;
            return (state != EntityState.RetrievedPristine && !state.IsDeleted());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void ValidateDomainObject(IDomainObject obj)
        {
            obj.Validate();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void BeforeSaveDomainObject(IDomainObject obj)
        {
            obj.BeforeCommit();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void AfterSaveDomainObject(IDomainObject obj)
        {
            obj.AfterCommit();
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
