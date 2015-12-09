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

namespace NWheels.Core
{
    internal class RealFramework : IFramework, ICoreFramework
    {
        private readonly IComponentContext _components;
        private readonly INodeConfiguration _nodeConfig;
        private readonly IThreadLogAnchor _threadLogAnchor;
        private readonly UnitOfWorkFactory _unitOfWorkFactory;
        private readonly RealTimeoutManager _timeoutManager;

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

        public TRepository NewUnitOfWork<TRepository>(bool autoCommit, IsolationLevel? isolationLevel = null, string databaseName = null) 
            where TRepository : class, IApplicationDataRepository
        {
            return _unitOfWorkFactory.NewUnitOfWork<TRepository>(autoCommit, isolationLevel, databaseName);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IApplicationDataRepository NewUnitOfWork(
            Type domainContextType, 
            bool autoCommit = true, 
            IsolationLevel? isolationLevel = null, 
            string databaseName = null)
        {
            return _unitOfWorkFactory.NewUnitOfWork(domainContextType, autoCommit, isolationLevel, databaseName);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IApplicationDataRepository NewUnitOfWorkForEntity(
            Type entityContractType, 
            bool autoCommit = true, 
            IsolationLevel? isolationLevel = null, 
            string databaseName = null)
        {
            var dataRepositoryFactory = _components.Resolve<IDataRepositoryFactory>();
            var dataRepositoryContract = dataRepositoryFactory.GetDataRepositoryContract(entityContractType);
            
            return _unitOfWorkFactory.NewUnitOfWork(dataRepositoryContract, autoCommit, isolationLevel, databaseName);
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
    }
}
