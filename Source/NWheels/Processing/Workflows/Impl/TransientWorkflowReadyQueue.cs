using System;
using System.Collections.Concurrent;
using System.Threading;
using Autofac;
using NWheels.Hosting;
using NWheels.Logging;
using NWheels.Processing.Workflows.Core;

namespace NWheels.Processing.Workflows.Impl
{
    internal class TransientWorkflowReadyQueue : LifecycleEventListenerBase, IWorkflowReadyQueue
    {
        private readonly IComponentContext _components;
        private readonly ILogger _logger;
        private IWorkflowInstanceContainer _instanceContainer;
        private Thread _listenerThread;
        private BlockingCollection<WorkItem> _queue;
        private CancellationTokenSource _cancellation;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TransientWorkflowReadyQueue(IComponentContext components, ILogger logger)
        {
            _components = components;
            _logger = logger;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void EnqueueRun(Guid workflowInstanceId)
        {
            _queue.Add(new WorkItem(workflowInstanceId, events: null));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void EnqueueDispatchAndRun(Guid workflowInstanceId, IWorkflowEvent[] events)
        {
            _queue.Add(new WorkItem(workflowInstanceId, events));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of LifecycleEventListenerBase

        public override void NodeActivating()
        {
            _instanceContainer = _components.Resolve<IWorkflowInstanceContainer>();
            _cancellation = new CancellationTokenSource();
            _queue = new BlockingCollection<WorkItem>();
            _listenerThread = new Thread(RunListenerThread);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void Activate()
        {
            _logger.StatringListenerThread();
            _listenerThread.Start();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void NodeDeactivating()
        {
            _logger.StoppingListenerThread();

            try
            {
                _cancellation.Cancel(throwOnFirstException: false);
            }
            catch ( Exception error )
            {
                _logger.ListenerThreadStoppedWithErrors(error);
                throw;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void Deactivate()
        {
            if ( !_listenerThread.Join(30000) )
            {
                throw _logger.ListenerThreadStopTimedOut();
            }

            _logger.ListenerThreadStopped();
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void RunListenerThread()
        {
            _logger.ListenerThreadStarted();

            while ( !_cancellation.Token.IsCancellationRequested )
            {
                WorkItem workItem;

                if ( _queue.TryTake(out workItem, 600000, _cancellation.Token) )
                {
                    var instance = _instanceContainer.GetInstanceById(workItem.WorkflowInstanceId);

                    if ( workItem.Events != null )
                    {
                        instance.DispatchAndRun(workItem.Events);
                    }
                    else
                    {
                        instance.Run();
                    }
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface ILogger : IApplicationEventLogger
        {
            [LogVerbose]
            void StatringListenerThread();
            [LogVerbose]
            void ListenerThreadStarted();
            [LogVerbose]
            void StoppingListenerThread();
            [LogVerbose]
            void ListenerThreadStopped();
            [LogCritical]
            void ListenerThreadStoppedWithErrors(Exception errors);
            [LogCritical]
            TimeoutException ListenerThreadStopTimedOut();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class WorkItem
        {
            public WorkItem(Guid workflowInstanceId, IWorkflowEvent[] events)
            {
                this.WorkflowInstanceId = workflowInstanceId;
                this.Events = events;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public Guid WorkflowInstanceId { get; private set; }
            public IWorkflowEvent[] Events { get; private set; }
        }
    }
}
