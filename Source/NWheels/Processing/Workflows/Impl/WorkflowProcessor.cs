using System;
using System.Collections.Generic;
using System.Linq;
using NWheels.Processing.Workflows.Core;

namespace NWheels.Processing.Workflows.Impl
{
    internal class WorkflowProcessor : IWorkflowBuilder
    {
        private readonly IFramework _framework;
        private readonly IWorkflowProcessorContext _context;
        private readonly Dictionary<string, IActorSite> _actorSitesByName;
        private readonly List<IActorSite> _actorSitesByPriority;
        private readonly WorkflowAwaitList<string> _awaitingActors;
        private IActorSite _initialActorSite;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public WorkflowProcessor(IFramework framework, IWorkflowProcessorContext context)
        {
            _framework = framework;
            _context = context;
            _actorSitesByName = new Dictionary<string, IActorSite>();
            _actorSitesByPriority = new List<IActorSite>();
            _awaitingActors = new WorkflowAwaitList<string>();
            _initialActorSite = null;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        void IWorkflowBuilder.AddActor<TWorkItem>(string name, int priority, IWorkflowActor<TWorkItem> actor, IWorkflowRouter router, bool isInitial)
        {
            var site = new ActorSite<TWorkItem>(this, name, priority, actor, router);
            _actorSitesByName.Add(name, site);

            if ( isInitial )
            {
                _initialActorSite = site;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void EndBuildWorkflow()
        {
            _actorSitesByPriority.AddRange(_actorSitesByName.Values);
            _actorSitesByPriority.Sort((x, y) => x.Priority.CompareTo(y.Priority));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public WorkflowProcessorSnapshot TakeSnapshot()
        {
            return new WorkflowProcessorSnapshot() {
                AwaitList = WorkflowProcessorSnapshot.AwaitListSnapshot.TakeSnapshotOf(_awaitingActors)
            };
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void RestoreSnapshot(WorkflowProcessorSnapshot snapshot)
        {
            RestoreAwaitList(snapshot.AwaitList);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ProcessorResult DispatchAndRun(IEnumerable<IWorkflowEvent> receivedEvents)
        {
            foreach ( var @event in receivedEvents )
            {
                DispatchOneEvent(@event);
            }

            return RunToSuspendOrCompletion();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ProcessorResult Run()
        {
            GetInitialActorSite().EnqueueWorkItem<object>(_context.InitialWorkItem);
            return RunToSuspendOrCompletion();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private IActorSite TryGetNextActorToExecute()
        {
            for ( int i = 0 ; i < _actorSitesByPriority.Count ; i++ )
            {
                if ( _actorSitesByPriority[i].WorkItemCount > 0 )
                {
                    return _actorSitesByPriority[i];
                }
            }

            return null;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private IActorSite GetActorSiteByName(string actorName)
        {
            return _actorSitesByName[actorName];
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private IActorSite GetInitialActorSite()
        {
            return (_initialActorSite ?? _actorSitesByPriority.First());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void DispatchOneEvent(IWorkflowEvent @event)
        {
            var awaitingActorNames = _awaitingActors.Take(@event);

            foreach ( var actorName in awaitingActorNames )
            {
                var site = _actorSitesByName[actorName];
                site.DispatchEvent(@event);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void RestoreAwaitList(WorkflowProcessorSnapshot.AwaitListSnapshot snapshot)
        {
            Dictionary<string, Type> eventTypes = new Dictionary<string, Type>();

            foreach ( var entry in snapshot.Entries )
            {
                Type eventType;

                if ( !eventTypes.TryGetValue(entry.EventClrType, out eventType) )
                {
                    eventType = Type.GetType(entry.EventClrType, throwOnError: true);
                    eventTypes.Add(entry.EventClrType, eventType);
                }

                foreach ( var awaiter in entry.Awaiters )
                {
                    _awaitingActors.Push(eventType, entry.EventKey, awaiter.ActorName, awaiter.TimeoutAtUtc);
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private ProcessorResult RunToSuspendOrCompletion()
        {
            using ( _context.Logger.ProcessorRunning() )
            {
                IActorSite nextActorToExecute;

                while ( (nextActorToExecute = TryGetNextActorToExecute()) != null )
                {
                    nextActorToExecute.ExecuteOneWorkItem();
                }

                var result = (_awaitingActors.IsEmpty ? ProcessorResult.Completed : ProcessorResult.Suspended);
                
                _context.Logger.ExitingProcessorRun(result);
                return result;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void AwaitEvent(IActorSite actor, Type eventType, object eventKey, TimeSpan timeout)
        {
            _awaitingActors.Push(eventType, eventKey, awaitId: actor.Name, timeoutAtUtc: _framework.UtcNow.Add(timeout));
            _context.AwaitEvent(eventType, eventKey, timeout);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private interface IActorSite
        {
            void EnqueueWorkItem<TWorkItem>(TWorkItem workItem);
            void ExecuteOneWorkItem();
            void DispatchEvent(IWorkflowEvent @event);
            string Name { get; }
            int Priority { get; }
            int WorkItemCount { get; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Instancing: 
        ///     Every instance of WorkflowProcessor has its own non-shared ActorSite objects;
        ///     The instances of IWorkflowActor and IWorkflowRouter can be shared among different instances of WorkflowProcessor.
        /// Thread safety:
        ///     Every instance of WorkflowProcessor is executed by one thread at a time;
        ///     Thus, ActorSite objects are never accessed concurrently by multiple threads.
        ///     Since IWorkflowActor and IWorkflowRouter objects can be shared among different WorkflowProcessor instances, 
        ///     those objects can be concurrently accessed by multiple threads. 
        ///     Best practice: make IWorkflowActor and IWorkflowRouter implementations immutable.
        /// </summary>
        private class ActorSite<TWorkItem> : IActorSite, IWorkflowActorContext, IWorkflowRouterContext
        {
            private readonly WorkflowProcessor _processor;
            private readonly string _name;
            private readonly int _priority;
            private readonly Queue<TWorkItem> _workItems;
            private readonly IWorkflowActor<TWorkItem> _actor;
            private readonly IWorkflowRouter _router;
            private TWorkItem _lastWorkItem;
            private object _lastActorResult;
            private IWorkflowEvent _lastReceivedEvent;
            private bool _lastActorAsync;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ActorSite(WorkflowProcessor processor, string name, int priority, IWorkflowActor<TWorkItem> actor, IWorkflowRouter router)
            {
                _processor = processor;
                _name = name;
                _priority = priority;
                _actor = actor;
                _router = router;
                _workItems = new Queue<TWorkItem>();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            #region IWorkflowActorSiteContext implementation

            void IWorkflowActorSiteContext.EnqueueWorkItem<T>(string actorName, T workItem)
            {
                _processor.GetActorSiteByName(actorName).EnqueueWorkItem(workItem);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            IWorkflowInstanceInfo IWorkflowActorSiteContext.WorkflowInstance
            {
                get { return _processor._context.WorkflowInstance; }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            string IWorkflowActorSiteContext.ActorName
            {
                get { return _name; }
            }

            #endregion

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            #region IWorkflowActorContext implementation

            void IWorkflowActorContext.SetResult<T>(T resultValue)
            {
                _lastActorResult = resultValue;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------
            
            void IWorkflowActorContext.AwaitEvent<TEvent>(TimeSpan timeout)
            {
                _processor._context.AwaitEvent(typeof(TEvent), eventKey: null, timeout: timeout);
                _lastActorAsync = true;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            void IWorkflowActorContext.AwaitEvent<TEvent, TKey>(TKey key, TimeSpan timeout)
            {
                _processor.AwaitEvent(this, typeof(TEvent), key, timeout);
                _lastActorAsync = true;
            }

            #endregion

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            #region IWorkflowRouterContext implementation

            bool IWorkflowRouterContext.HasActorWorkItem<T>()
            {
                return (_lastWorkItem is T);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            T IWorkflowRouterContext.GetActorWorkItem<T>()
            {
                return (T)(object)_lastWorkItem;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            bool IWorkflowRouterContext.HasActorResult<TResult>()
            {
                return (_lastActorResult is TResult);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            TResult IWorkflowRouterContext.GetActorResult<TResult>()
            {
                return (TResult)_lastActorResult;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            bool IWorkflowRouterContext.HasReceivedEvent<TEvent>()
            {
                return (_lastReceivedEvent is TEvent);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------
            
            TEvent IWorkflowRouterContext.GetReceivedEvent<TEvent>()
            {
                return (TEvent)_lastReceivedEvent;
            }

            #endregion

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            void IActorSite.EnqueueWorkItem<T>(T workItem)
            {
                var acceptedWorkItem = (TWorkItem)(object)workItem;
                _workItems.Enqueue(acceptedWorkItem);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void ExecuteOneWorkItem()
            {
                _processor._context.Logger.ExecutingActor(_name);

                var workItem = _workItems.Dequeue();

                _lastWorkItem = workItem;
                _lastActorResult = null;
                _lastActorAsync = false;
                _lastReceivedEvent = null;

                _actor.Execute(this, workItem);

                if ( !_lastActorAsync )
                {
                    _processor._context.Logger.ExecutingRouter(_name);
                    _router.Route(this);
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void DispatchEvent(IWorkflowEvent @event)
            {
                _lastReceivedEvent = @event;
                _router.Route(this);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public string Name
            {
                get { return _name; }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public int Priority
            {
                get { return _priority; }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public int WorkItemCount
            {
                get { return _workItems.Count; }
            }
        }
    }
}
