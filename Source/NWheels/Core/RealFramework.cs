using Autofac;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NWheels.Authorization;
using NWheels.Concurrency.Core;
using NWheels.Extensions;
using NWheels.Hosting;
using NWheels.Entities;
using NWheels.Concurrency;
using NWheels.Concurrency.Impl;
using NWheels.Logging.Core;
using System.Collections.Concurrent;
using NWheels.Authorization.Core;
using NWheels.Conventions.Core;
using NWheels.Hosting.Core;
using NWheels.Entities.Core;
using NWheels.Logging;

namespace NWheels.Core
{
    internal class RealFramework : IFramework, ICoreFramework
    {
        private readonly IComponentContext _components;
        private readonly INodeConfiguration _nodeConfig;
        private readonly IThreadLogAnchor _threadLogAnchor;
        private readonly UnitOfWorkFactory _unitOfWorkFactory;
        private readonly RealTimeoutManager _timeoutManager;
        private INodeHostLogger _nodeHostLogger;
        private IPlainLog _plainLog;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public RealFramework(IComponentContext components, INodeConfiguration nodeConfig, IThreadLogAnchor threadLogAnchor, RealTimeoutManager timeoutManager)
        {
            _components = components;
            _nodeConfig = nodeConfig;
            _threadLogAnchor = threadLogAnchor;
            _unitOfWorkFactory = components.Resolve<UnitOfWorkFactory>();
            _timeoutManager = timeoutManager;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public T NewDomainObject<T>() where T : class
        {
            var entityObjectFactory = _components.Resolve<IEntityObjectFactory>();
            var persistableObject = entityObjectFactory.NewEntity<T>();
            return _components.Resolve<IDomainObjectFactory>().CreateDomainObjectInstance<T>(persistableObject);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public T NewDomainObject<T>(IComponentContext externalComponents) where T : class
        {
            var entityObjectFactory = _components.Resolve<IEntityObjectFactory>();
            var persistableObject = entityObjectFactory.NewEntity<T>(externalComponents);
            return _components.Resolve<IDomainObjectFactory>().CreateDomainObjectInstance<T>(persistableObject);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TRepository NewUnitOfWork<TRepository>(bool autoCommit, UnitOfWorkScopeOption? scopeOption = null, string connectionString = null) 
            where TRepository : class, IApplicationDataRepository
        {
            return _unitOfWorkFactory.NewUnitOfWork<TRepository>(autoCommit, scopeOption, connectionString);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IApplicationDataRepository NewUnitOfWork(
            Type domainContextType, 
            bool autoCommit = true, 
            UnitOfWorkScopeOption? scopeOption = null,
            string connectionString = null)
        {
            return _unitOfWorkFactory.NewUnitOfWork(domainContextType, autoCommit, scopeOption, connectionString);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IApplicationDataRepository NewUnitOfWorkForEntity(
            Type entityContractType, 
            bool autoCommit = true, 
            UnitOfWorkScopeOption? scopeOption = null,
            string connectionString = null)
        {
            var dataRepositoryFactory = _components.Resolve<IDataRepositoryFactory>();
            var dataRepositoryContract = dataRepositoryFactory.GetDataRepositoryContract(entityContractType);

            return _unitOfWorkFactory.NewUnitOfWork(dataRepositoryContract, autoCommit, scopeOption, connectionString);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Thread CreateThread(Action threadCode, Func<ILogActivity> threadLogFactory, ThreadTaskType? taskType, string description)
        {
            var runner = new ThreadRunner(this, threadCode, threadLogFactory, taskType, description);
            return runner.ThreadObject;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void RunThreadCode(Action threadCode, Func<ILogActivity> threadLogFactory, ThreadTaskType? taskType, string description)
        {
            ThreadTaskType effectiveTaskType = taskType.GetValueOrDefault(ThreadTaskType.Unspecified);
            string effectiveDescription = description ?? "Framework.RunThreadCode[unspecified]";
            Exception terminatingException = null;

            try
            {
                using ( var rootActivity = (threadLogFactory != null ? threadLogFactory() : null) )
                {
                    if ( threadLogFactory != null )
                    {
                        var threadLog = _threadLogAnchor.CurrentThreadLog;
                        effectiveDescription = threadLog.RootActivity.SingleLineText;
                        effectiveTaskType = threadLog.TaskType;
                    }

                    if (Thread.CurrentThread.Name == null)
                    {
                        Thread.CurrentThread.Name = effectiveDescription;
                    }

                    try
                    {
                        threadCode();
                    }
                    catch ( Exception e0 )
                    {
                        terminatingException = e0;

                        if ( threadLogFactory != null )
                        {
                            rootActivity.Fail(terminatingException);
                        }
                        
                        SafeGetComponent(ref _nodeHostLogger).ThreadTerminatedByException(
                            taskType: effectiveTaskType,
                            rootActivity: effectiveDescription,
                            exception: terminatingException);
                    }
                }
            }
            catch ( Exception e1 )
            {
                try
                {
                    var plainLog = SafeGetComponent(ref _plainLog);

                    if ( terminatingException != null )
                    {
                        plainLog.Critical("Thread [{0}] terminated by exception: {1}", effectiveDescription, terminatingException);
                    }

                    if ( e1 != terminatingException )
                    {
                        plainLog.Critical("Unhandled exception in thread [{0}]: {1}", effectiveDescription, e1);
                    }
                }
                catch ( Exception e2 )
                {
                    try
                    {
                        if ( terminatingException != null )
                        {
                            CrashLog.LogUnhandledException(terminatingException, isTerminating: false);
                        }

                        CrashLog.LogUnhandledException(e1, isTerminating: false);
                        CrashLog.LogUnhandledException(e2, isTerminating: false);
                    }
                    // ReSharper disable once EmptyGeneralCatchClause
                    catch
                    {
                    }
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IDomainObject NewDomainObject(Type contractType)
        {
            var entityObjectFactory = _components.Resolve<IEntityObjectFactory>();
            var persistableObject = (IPersistableObject)entityObjectFactory.NewEntity(contractType);
            return _components.Resolve<IDomainObjectFactory>().CreateDomainObjectInstance(persistableObject);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Guid NewGuid()
        {
            return Guid.NewGuid();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public int NewRandomInt32()
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public long NewRandomInt64()
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IResourceLock NewLock(ResourceLockMode mode, string resourceNameFormat, params object[] formatArgs)
        {
            return new ResourceLock(mode, resourceNameFormat.FormatIf(formatArgs));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ITimeoutHandle NewTimer(string timerName, string timerInstanceId, TimeSpan initialDueTime, Action callback)
        {
            RealTimeoutHandle h = new RealTimeoutHandleNoParam(timerName, timerInstanceId, initialDueTime, callback, _timeoutManager);
            _timeoutManager.AddTimeoutEvent(h);
            return h;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ITimeoutHandle NewTimer<TParam>(
            string timerName, 
            string timerInstanceId, 
            TimeSpan initialDueTime, 
            Action<TParam> callback, 
            TParam parameter)
        {
            RealTimeoutHandle h = new RealTimeoutHandle<TParam>(timerName, timerInstanceId, initialDueTime, callback, parameter, _timeoutManager);
            _timeoutManager.AddTimeoutEvent(h);
            return h;
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

        public INodeConfiguration CurrentNode
        {
            get
            {
                return _nodeConfig;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IIdentityInfo CurrentIdentity
        {
            get
            {
                var principal = Thread.CurrentPrincipal;

                if ( principal != null )
                {
                    return (principal.Identity as IIdentityInfo);
                }

                return null;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string CurrentSessionId
        {
            get
            {
                var currentSession = Session.Current;

                if ( currentSession != null )
                {
                    return currentSession.Id;
                }
                else
                {
                    return null;
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Guid CurrentCorrelationId
        {
            get
            {
                var currentThreadLog = _threadLogAnchor.CurrentThreadLog;
                return (currentThreadLog != null ? currentThreadLog.CorrelationId : Guid.Empty);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public DateTime UtcNow
        {
            get
            {
                return DateTime.UtcNow;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IReadOnlyThreadLog CurrentThreadLog
        {
            get
            {
                return _threadLogAnchor.CurrentThreadLog;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private T SafeGetComponent<T>(ref T field)
            where T : class
        {
            if ( field == null )
            {
                field = _components.Resolve<T>();
            }

            return field;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class ThreadRunner
        {
            private readonly RealFramework _ownerFramework;
            private readonly Action _threadCode;
            private readonly Func<ILogActivity> _threadLogFactory;
            private readonly ThreadTaskType? _taskType;
            private readonly string _description;
            
            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public readonly Thread ThreadObject;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ThreadRunner(
                RealFramework ownerFramework, 
                Action threadCode, 
                Func<ILogActivity> threadLogFactory, 
                ThreadTaskType? taskType, 
                string description)
            {
                this._ownerFramework = ownerFramework;
                this._threadCode = threadCode;
                this._threadLogFactory = threadLogFactory;
                this._taskType = taskType;
                this._description = description;

                this.ThreadObject = new Thread(RunThread);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void RunThread()
            {
                _ownerFramework.RunThreadCode(_threadCode, _threadLogFactory, _taskType, _description);
            }
        }
    }
}
