using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using NWheels.Extensions;
using NWheels.Processing.Core;

namespace NWheels.Processing.Impl
{
    internal class WorkflowProcessor : IWorkflowBuilder
    {
        private readonly IWorkflowProcessorContext _context;
        private readonly Dictionary<string, IActorSite> _actorSitesByName;
        private readonly List<IActorSite> _actorSitesByPriority;
        private readonly WorkflowAwaitList<string> _awaitingActors;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public WorkflowProcessor(IWorkflowProcessorContext context)
        {
            _context = context;
            _actorSitesByName = new Dictionary<string, IActorSite>();
            _actorSitesByPriority = new List<IActorSite>();
            _awaitingActors = new WorkflowAwaitList<string>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        void IWorkflowBuilder.AddActor<TWorkItem>(string name, int priority, IWorkflowActor<TWorkItem> actor, IWorkflowRouter router)
        {
            var site = new ActorSite<TWorkItem>(this, name, priority, actor, router);
            _actorSitesByName.Add(name, site);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void EndBuildWorkflow()
        {
            _actorSitesByPriority.AddRange(_actorSitesByName.Values);
            _actorSitesByPriority.Sort((x, y) => y.Priority.CompareTo(x.Priority));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public WorkflowProcessorSnapshot TakeSnapshot()
        {
            return new WorkflowProcessorSnapshot() {
                Actors = _actorSitesByPriority.Select(site => site.TakeSnapshot()).ToArray(),
                AwaitList = WorkflowProcessorSnapshot.ActorAwaitList.TakeSnapshotOf(_awaitingActors)
            };
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void RestoreSnapshot(WorkflowProcessorSnapshot snapshot)
        {
            foreach ( var actrorSnapshot in snapshot.Actors )
            {
                _actorSitesByName[actrorSnapshot.Name].RestoreSnapshot(actrorSnapshot);
            }

            RestoreAwaitList(snapshot.AwaitList);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ProcessorResult DispatchAndRun(IEnumerable<IWorkflowEvent> receivedEvents)
        {
            foreach ( var @event in receivedEvents )
            {
                DispatchOneEvent(@event);
            }

            return Run();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ProcessorResult Run()
        {
            IActorSite nextActorToExecute;

            while ( (nextActorToExecute = TryGetNextActorToExecute()) != null )
            {
                nextActorToExecute.ExecuteOneWorkItem();
            }

            return (_awaitingActors.IsEmpty ? ProcessorResult.Completed : ProcessorResult.Suspended);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private IActorSite TryGetNextActorToExecute()
        {
            for ( int i = 0; i < _actorSitesByPriority.Count; i++ )
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

        private void DispatchOneEvent(IWorkflowEvent @event)
        {
            var awaitingActorNames = _awaitingActors.Take(@event.GetType(), @event.KeyObject);

            foreach ( var actorName in awaitingActorNames )
            {
                var site = _actorSitesByName[actorName];
                site.DispatchEvent(@event);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void RestoreAwaitList(WorkflowProcessorSnapshot.ActorAwaitList snapshot)
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

                foreach ( var awaitActorName in entry.ActorNames )
                {
                    _awaitingActors.Push(eventType, entry.EventKey, awaitActorName);
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private interface IActorSite
        {
            void EnqueueWorkItem<TWorkItem>(TWorkItem workItem);
            void ExecuteOneWorkItem();
            void DispatchEvent(IWorkflowEvent @event);
            WorkflowProcessorSnapshot.Actor TakeSnapshot();
            void RestoreSnapshot(WorkflowProcessorSnapshot.Actor snapshot);
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

            #region IWorkflowActorContext implementation

            void IWorkflowActorContext.SetResult<T>(T resultValue)
            {
                _lastActorResult = resultValue;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            void IWorkflowActorContext.EnqueueWorkItem<T>(string actorName, T workItem)
            {
                _processor.GetActorSiteByName(actorName).EnqueueWorkItem(workItem);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------
            
            void IWorkflowActorContext.AwaitEvent<TEvent>()
            {
                _processor._context.AwaitEvent(typeof(TEvent), eventKey: null);
                _lastActorAsync = true;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            void IWorkflowActorContext.AwaitEvent<TEvent, TKey>(TKey key)
            {
                _processor._context.AwaitEvent(typeof(TEvent), key);
                _lastActorAsync = true;
            }

            #endregion

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            #region IWorkflowRouterContext implementation

            T IWorkflowRouterContext.GetActorWorkItem<T>()
            {
                return (T)(object)_lastWorkItem;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            TResult IWorkflowRouterContext.GetActorResult<TResult>()
            {
                return (TResult)_lastActorResult;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------
            
            TEvent IWorkflowRouterContext.GetReceivedEvent<TEvent>()
            {
                return (TEvent)_lastReceivedEvent;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            void IWorkflowRouterContext.EnqueueWorkItem<T>(string actorName, T workItem)
            {
                _processor.GetActorSiteByName(actorName).EnqueueWorkItem(workItem);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------
            
            bool IWorkflowRouterContext.HasActorResult
            {
                get
                {
                    return (_lastActorResult != null);
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            bool IWorkflowRouterContext.HasReceivedEvent
            {
                get
                {
                    return (_lastReceivedEvent != null);
                }
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
                var workItem = _workItems.Dequeue();

                _lastWorkItem = workItem;
                _lastActorResult = null;
                _lastActorAsync = false;
                _lastReceivedEvent = null;

                _actor.Execute(this, workItem);

                if ( !_lastActorAsync )
                {
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

            public WorkflowProcessorSnapshot.Actor TakeSnapshot()
            {
                return new WorkflowProcessorSnapshot.Actor() {
                    Name = _name,
                    WorkItems = _workItems.Select(item => item.ToString()).ToArray()
                };
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void RestoreSnapshot(WorkflowProcessorSnapshot.Actor snapshot)
            {
                _workItems.Clear();

                foreach ( var workItem in snapshot.WorkItems )
                {
                    _workItems.Enqueue((TWorkItem)workItem);
                }
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
