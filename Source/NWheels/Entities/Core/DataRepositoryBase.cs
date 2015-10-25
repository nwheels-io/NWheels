using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Security;
using System.Threading;
using Autofac;
using NWheels.Authorization;
using NWheels.Authorization.Core;
using NWheels.Concurrency;
using NWheels.Conventions.Core;
using NWheels.DataObjects.Core;
using NWheels.Entities.Factories;
using NWheels.Extensions;
using NWheels.Logging;
using NWheels.Logging.Core;
using NWheels.Processing.Messages;

namespace NWheels.Entities.Core
{
    public abstract class DataRepositoryBase : IApplicationDataRepository, IRuntimeAccessContext
    {
        private readonly Dictionary<Type, IEntityRepository> _entityRepositoryByContractType;
        private readonly Dictionary<Type, IPartitionedRepository> _partitionedRepositoryByContractType;
        private readonly Dictionary<Type, Action<IDataRepositoryCallback>> _genericCallbacksByContractType;
        private readonly IComponentContext _components;
        private readonly IDomainObjectFactory _domainObjectFactory;
        private readonly IServiceBus _serviceBus;
        private readonly IDomainContextLogger _logger;
        private readonly bool _autoCommit;
        private readonly IResourceConsumerScopeHandle _consumerScope;
        private UnitOfWorkState _currentState;
        private ILifetimeScope _componentLifetimeScope;
        private bool _disposed;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected DataRepositoryBase(IResourceConsumerScopeHandle consumerScope, IComponentContext components, bool autoCommit)
        {
            var currentThreadLog = components.Resolve<IThreadLogAnchor>().CurrentThreadLog;
            InitializerThreadText = (currentThreadLog != null ? currentThreadLog.RootActivity.SingleLineText : string.Empty);

            _entityRepositoryByContractType = new Dictionary<Type, IEntityRepository>();
            _partitionedRepositoryByContractType = new Dictionary<Type, IPartitionedRepository>();
            _genericCallbacksByContractType = new Dictionary<Type, Action<IDataRepositoryCallback>>();
            _autoCommit = autoCommit;
            _components = components;
            _domainObjectFactory = components.Resolve<IDomainObjectFactory>();
            _serviceBus = components.Resolve<IServiceBus>();
            _logger = components.Resolve<IDomainContextLogger>();
            _currentState = UnitOfWorkState.Untouched;
            _componentLifetimeScope = null;
            _consumerScope = consumerScope;
            _disposed = false;

            _logger.NewRootUnitOfWork(domainContext: this.ToString());
            RuntimeEntityModelHelpers.CurrentDomainContext = this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual void Dispose()
        {
            bool shouldDisposeResourcesNow;
            DisposeConsumerScope(out shouldDisposeResourcesNow);

            if ( shouldDisposeResourcesNow )
            {
                RuntimeEntityModelHelpers.CurrentDomainContext = null;
            }
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
                ExecuteNotificationPhase(changeSet);

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
            where TEntityImpl : TEntityContract
        {
            _entityRepositoryByContractType.Add(typeof(TEntityContract), (IEntityRepository)repo); 
            _genericCallbacksByContractType.Add(typeof(TEntityContract),
                callback => {
                    ((IDataRepositoryCallback<TEntityContract>)callback).Invoke<TEntityImpl>(repo);
                });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void RegisterPartitionedRepository<TEntityContract, TPartition>(IPartitionedRepository<TEntityContract, TPartition> repo)
        {
            _partitionedRepositoryByContractType.Add(typeof(TEntityContract), (IPartitionedRepository)repo);
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

        public IEntityRepository GetEntityRepository(object entity)
        {
            IEntityRepository repository;

            if ( TryGetEntityRepository(entity, out repository) )
            {
                return repository;
            }

            throw new KeyNotFoundException(
                "Entity repository for contract '" + ((IObject)entity).ContractType.FullName + "' could not be found in the data repository.");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool TryGetEntityRepository(object entity, out IEntityRepository entityRepository)
        {
            var contractType = ((IObject)entity).ContractType;
            var partitioned = (entity.As<IPersistableObject>() as IPartitionedObject);

            if ( partitioned != null )
            {
                IPartitionedRepository partitionedRepository;

                if ( _partitionedRepositoryByContractType.TryGetValue(contractType, out partitionedRepository) )
                {
                    entityRepository = partitionedRepository[partitioned.PartitionValue];
                    return true;
                }
            }
            else
            {
                return TryGetEntityRepository(contractType, out entityRepository);
            }

            entityRepository = null;
            return false;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of IRuntimeAccessContext

        ISession IRuntimeAccessContext.Session
        {
            get
            {
                return NWheels.Authorization.Core.Session.Current;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        string IRuntimeAccessContext.UserStory
        {
            get
            {
                return null;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        Type IRuntimeAccessContext.ApiContract
        {
            get
            {
                return null;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        string IRuntimeAccessContext.ApiOperation
        {
            get
            {
                return null;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        Type IRuntimeAccessContext.DomainContext
        {
            get
            {
                return this.GetType();
            }
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual void InitializeCurrentSchema()
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual void EnsureDomainObjectTypesCreated()
        {
            foreach ( var contractType in _entityRepositoryByContractType.Keys )
            {
                _domainObjectFactory.GetOrBuildDomainObjectType(contractType, this.PersistableObjectFactory.GetType());
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IQueryable<TEntity> AuthorizeQuery<TEntity>(IQueryable<TEntity> source)
        {
            //return source;
            //TODO: temporarily disabled; re-enable when all applications are ready
            var rule = GetRuntimeEntityAccessRule<TEntity>();
            return rule.AuthorizeQuery(this, source);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IRuntimeEntityAccessRule<TEntity> GetRuntimeEntityAccessRule<TEntity>()
        {
            var identityInfo = (Thread.CurrentPrincipal.Identity as IIdentityInfo);

            if ( identityInfo == null )
            {
                throw new SecurityException("User is not authorized to access data.");
            }

            var rule = identityInfo.GetEntityAccessRule<TEntity>();

            if ( rule == null )
            {
                throw new SecurityException("No access rule defined for entity type: " + typeof(TEntity).FullName);
            }

            return rule;
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

        public IDomainObjectFactory DomainObjectFactory
        {
            get
            {
                return _domainObjectFactory;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public abstract IEntityObjectFactory PersistableObjectFactory { get; }
        
        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string InitializerThreadText { get; private set; }

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

        protected virtual void BeginLifetimeScope()
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

        protected virtual void EndLifetimeScope()
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

        protected virtual void ExecuteNotificationPhase(IEnumerable<object> changedEntities)
        {
            //var entityGroups = changedEntities.Select(e => e.As<IDomainObject>()).GroupBy(e => new { e.ContractType, e.State });

            //foreach ( var group in entityGroups )
            //{
            //    var repository = GetEntityRepository(group.Key.ContractType);
            //    var message = repository.CreateChangeMessage(group, group.Key.State);

            //    if ( message != null )
            //    {
            //        _serviceBus.EnqueueMessage(message);
            //    }
            //}
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
