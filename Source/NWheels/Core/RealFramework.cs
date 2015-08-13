using Autofac;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Concurrency.Core;
using NWheels.Extensions;
using NWheels.Hosting;
using NWheels.Entities;
using NWheels.Concurrency;
using NWheels.Logging.Core;

namespace NWheels.Core
{
    internal class RealFramework : IFramework, ICoreFramework
    {
        private readonly IComponentContext _components;
        private readonly INodeConfiguration _nodeConfig;
        private readonly IThreadLogAnchor _threadLogAnchor;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public RealFramework(IComponentContext components, INodeConfiguration nodeConfig, IThreadLogAnchor threadLogAnchor)
        {
            _components = components;
            _nodeConfig = nodeConfig;
            _threadLogAnchor = threadLogAnchor;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public T New<T>() where T : class
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TRepository NewUnitOfWork<TRepository>(bool autoCommit, IsolationLevel? isolationLevel = null) where TRepository : class, IApplicationDataRepository
        {
            var factory = _components.Resolve<IDataRepositoryFactory>();
            return factory.NewUnitOfWork<TRepository>(autoCommit, isolationLevel);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IApplicationDataRepository NewUnitOfWork(Type repositoryContractType, bool autoCommit = true, IsolationLevel? isolationLevel = null)
        {
            var factory = _components.Resolve<IDataRepositoryFactory>();
            return factory.NewUnitOfWork(repositoryContractType, autoCommit, isolationLevel);
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

        public ITimerHandle NewTimer(string timerName, string timerInstanceId, TimeSpan initialDueTime, TimeSpan? recurringPeriod, Action callback)
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ITimerHandle NewTimer<TParam>(
            string timerName, 
            string timerInstanceId, 
            TimeSpan initialDueTime, 
            TimeSpan? recurringPeriod, 
            Action<TParam> callback, 
            TParam parameter)
        {
            throw new NotImplementedException();
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
