using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using NWheels.Concurrency;
using NWheels.Entities;
using NWheels.Extensions;
using NWheels.Hosting;
using NWheels.Processing.Workflows.Core;

namespace NWheels.Processing.Workflows.Impl
{
    internal class WorkflowEngine : LifecycleEventListenerBase, IWorkflowEngine, IWorkflowInstanceContainer
    {
        private readonly IComponentContext _components;
        private readonly IFramework _framework;
        private readonly IWorkflowReadyQueue _queue;
        private readonly IWorkflowEngineLogger _logger;
        private readonly Dictionary<Type, WorkflowTypeRegistration> _registrationsByCodeBehindType;
        private readonly IResourceLock _instancesLock;
        private readonly Dictionary<Guid, WorkflowInstanceContext> _instanceContextsById;
        private readonly ConcurrentDictionary<Type, WorkflowCodeBehindAdapter> _adaptersByCodeBehindType;
        private readonly WorkflowAwaitList<Guid> _awaitingWorkflows;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public WorkflowEngine(
            IComponentContext components, 
            IFramework framework, 
            IWorkflowReadyQueue queue, 
            IWorkflowEngineLogger logger,
            IEnumerable<WorkflowTypeRegistration> registrations)
        {
            _framework = framework;
            _queue = queue;
            _logger = logger;
            _components = components;
            _registrationsByCodeBehindType = registrations.ToDictionary(r => r.CodeBehindType);
            _instancesLock = _framework.NewLock(ResourceLockMode.Exclusive, "WorkflowEngine.Instances");
            _instanceContextsById = new Dictionary<Guid, WorkflowInstanceContext>();
            _adaptersByCodeBehindType = new ConcurrentDictionary<Type, WorkflowCodeBehindAdapter>();
            _awaitingWorkflows = new WorkflowAwaitList<Guid>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IWorkflowInstance StartWorkflow<TCodeBehind, TDataEntity>(TDataEntity initialData)
            where TCodeBehind : class, IWorkflowCodeBehind
            where TDataEntity : class, IWorkflowInstanceEntity
        {
            return StartWorkflow(typeof(TCodeBehind), initialData);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IWorkflowInstance StartWorkflow(Type codeBehindType, IWorkflowInstanceEntity initialData)
        {
            var registration = FindWorkflowTypeRegistration(codeBehindType);
            var context = WorkflowInstanceContext.CreateNew(this, registration, initialData);

            using ( _instancesLock.AcquireWriteAccess("CreateWorkflow", holdDurationMs: 1) )
            {
                _instanceContextsById.Add(initialData.WorkflowInstanceId, context);
            }

            _queue.EnqueueRun(context.InstanceId);
            return context;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void DispatchEvent(IWorkflowEvent receivedEvent)
        {
            var awaitingIds = _awaitingWorkflows.Take(receivedEvent);
            var arrayOfSingleEvent = new[] { receivedEvent };

            foreach ( var singleId in awaitingIds )
            {
                _queue.EnqueueDispatchAndRun(singleId, arrayOfSingleEvent);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void DispatchEvents(IEnumerable<IWorkflowEvent> receivedEvents)
        {
            var eventsPerInstanceId = new Dictionary<Guid, LinkedList<IWorkflowEvent>>();

            foreach ( var singleEvent in receivedEvents )
            {
                var awaitingIds = _awaitingWorkflows.Take(singleEvent);

                foreach ( var singleId in awaitingIds )
                {
                    eventsPerInstanceId.GetOrAdd(singleId).AddLast(singleEvent);
                }
            }

            foreach ( var instanceEvents in eventsPerInstanceId )
            {
                _queue.EnqueueDispatchAndRun(instanceEvents.Key, instanceEvents.Value.ToArray());
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void SendTrigger<TTrigger>(TTrigger trigger, Guid stateMachineInstanceId, object context = null)
        {
            DispatchEvent(new StateMachineTriggerEvent<TTrigger>(stateMachineInstanceId, trigger, context));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool TryGetWorkflow(Guid instanceId, out IWorkflowInstance instance)
        {
            using ( _instancesLock.AcquireReadAccess("TryGetWorkflow", holdDurationMs: 1) )
            {
                WorkflowInstanceContext context;

                if ( _instanceContextsById.TryGetValue(instanceId, out context) )
                {
                    instance = context;
                    return true;
                }
                else
                {
                    instance = null;
                    return false;
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IWorkflowInstance[] GetCurrentWorkflows()
        {
            using ( _instancesLock.AcquireReadAccess("TryGetWorkflow", holdDurationMs: 1) )
            {
                return _instanceContextsById.Values.Cast<IWorkflowInstance>().ToArray();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        IWorkflowInstanceController IWorkflowInstanceContainer.GetInstanceById(Guid instanceId)
        {
            IWorkflowInstance instance;

            if ( !TryGetWorkflow(instanceId, out instance) )
            {
                throw _logger.InstanceNotFound(instanceId);
            }

            return (IWorkflowInstanceController)instance;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void NodeActivating()
        {
            using ( _logger.RecoveringExistingInstances() )
            {
                RecoverExistingInstances();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void RecoverExistingInstances()
        {
            using ( _instancesLock.AcquireWriteAccess("RecoverExistingInstances", holdDurationMs: 30000) )
            {
                foreach ( var registration in _registrationsByCodeBehindType.Values )
                {
                    IUnitOfWork unitOfWork;
                    var instanceRecordsToRecover = registration.GetDataEntityQuery(_framework, out unitOfWork).ToArray();
                    unitOfWork.Dispose();

                    foreach ( var instanceData in instanceRecordsToRecover )
                    {
                        var context = WorkflowInstanceContext.RecoverExisting(this, registration, instanceData);
                        _instanceContextsById.Add(context.InstanceId, context);
                    }
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private WorkflowCodeBehindAdapter GetCodeBehindAdapter(Type codeBehindType)
        {
            return _adaptersByCodeBehindType.GetOrAdd(codeBehindType, valueFactory: CreateCodeBehindAdapter);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private WorkflowCodeBehindAdapter CreateCodeBehindAdapter(Type codeBehindType)
        {
            var registration = FindWorkflowTypeRegistration(codeBehindType);
            return WorkflowCodeBehindAdapter.Create(_components, registration.CodeBehindType, registration.DataEntityType);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private WorkflowTypeRegistration FindWorkflowTypeRegistration(Type codeBehindType)
        {
            WorkflowTypeRegistration registration;

            if ( !_registrationsByCodeBehindType.TryGetValue(codeBehindType, out registration) )
            {
                throw _logger.CodeBehindTypeNotRegistered(codeBehindType.FullName);
            }

            return registration;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void AwaitEvent(Type eventType, object eventKey, Guid workflowInstanceId, TimeSpan timeout)
        {
            var timeoutAtUtc = _framework.UtcNow.Add(timeout);
            _awaitingWorkflows.Push(eventType, eventKey, workflowInstanceId, timeoutAtUtc);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Thread Safety:
        ///     An instance of WorkflowInstanceContext will be called Run() and DispatchAndRun() by one thread at a time.
        ///     Thus, Run() and DispatchAndRun() need no synchronization between them.
        ///     The rest of the operations are read-only, but can be invoked simultaneously by multiple threads. 
        /// </summary>
        private class WorkflowInstanceContext : IWorkflowInstanceContext, IWorkflowInstance, IWorkflowInstanceController
        {
            private readonly WorkflowEngine _ownerEngine;
            private readonly WorkflowTypeRegistration _registration;
            private readonly IWorkflowInstanceEntity _instanceData;
            private readonly WorkflowCodeBehindAdapter _codeBehindAdapter;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private WorkflowInstanceContext(WorkflowEngine ownerEngine, WorkflowTypeRegistration registration, IWorkflowInstanceEntity instanceData)
            {
                _ownerEngine = ownerEngine;
                _registration = registration;
                _instanceData = instanceData;
                _codeBehindAdapter = ownerEngine.GetCodeBehindAdapter(registration.CodeBehindType);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public Guid InstanceId
            {
                get { return _instanceData.WorkflowInstanceId; }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            void IWorkflowInstanceContext.AwaitEvent(Type eventType, object eventKey, TimeSpan timeout)
            {
                _ownerEngine.AwaitEvent(eventType, eventKey, _instanceData.WorkflowInstanceId, timeout);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            IComponentContext IWorkflowInstanceContext.Components
            {
                get { return _ownerEngine._components; }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            IFramework IWorkflowInstanceContext.Framework
            {
                get { return _ownerEngine._framework; }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            IWorkflowInstanceEntity IWorkflowInstanceContext.InstanceData
            {
                get { return _instanceData; }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------
            
            WorkflowCodeBehindAdapter IWorkflowInstanceContext.CodeBehindAdapter
            {
                get { return _codeBehindAdapter; }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            WorkflowState IWorkflowInstance.State
            {
                get { return _instanceData.WorkflowState; }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            DateTime IWorkflowInstance.CreatedAtUtc
            {
                get { return _instanceData.CreatedAtUtc; }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            DateTime IWorkflowInstance.StateChangedAtUtc
            {
                get { return _instanceData.UpdatedAtUtc; }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            Type IWorkflowInstance.CodeBehindType
            {
                get { return _registration.CodeBehindType; }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            TimeSpan IWorkflowInstance.TotalTime
            {
                get { return _instanceData.TotalTime; }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            TimeSpan IWorkflowInstance.TotalExecutionTime
            {
                get { return _instanceData.TotalExecutionTime; }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            TimeSpan IWorkflowInstance.TotalSuspensionTime
            {
                get { return _instanceData.TotalSuspensionTime; }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            int IWorkflowInstance.TotalSuspensionCount
            {
                get { return _instanceData.TotalSuspensionCount; }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            WorkflowState IWorkflowInstanceController.Run()
            {
                var instance = new WorkflowInstance(this);
                return instance.Run();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            WorkflowState IWorkflowInstanceController.DispatchAndRun(IEnumerable<IWorkflowEvent> receivedEvents)
            {
                var instance = new WorkflowInstance(this);
                return instance.DispatchAndRun(receivedEvents);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            IWorkflowInstance IWorkflowInstanceController.InstanceContext
            {
                get
                {
                    return this;
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public static WorkflowInstanceContext CreateNew(
                WorkflowEngine ownerEngine, WorkflowTypeRegistration registration, IWorkflowInstanceEntity initialData)
            {
                initialData.WorkflowInstanceId = ownerEngine._framework.NewGuid();
                initialData.WorkflowState = WorkflowState.Created;
                initialData.CreatedAtUtc = ownerEngine._framework.UtcNow;
                initialData.UpdatedAtUtc = initialData.CreatedAtUtc;
                initialData.CodeBehindClrType = registration.CodeBehindType;

                return new WorkflowInstanceContext(ownerEngine, registration, initialData);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public static WorkflowInstanceContext RecoverExisting(
                WorkflowEngine ownerEngine, WorkflowTypeRegistration registration, IWorkflowInstanceEntity persistedData)
            {
                return new WorkflowInstanceContext(ownerEngine, registration, persistedData);
            }
        }
    }
}
