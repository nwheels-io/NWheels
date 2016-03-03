using System;
using System.Collections.Generic;
using NWheels.Extensions;
using NWheels.Processing;

namespace NWheels.Stacks.UI.WpfCaliburnAvalon.CqrsApp
{
    public abstract class CqrsAppModelBase : IDisposable
    {
        private readonly ICqrsClient _client;
        private readonly bool _willReceiveSnapshot;
        private readonly Dictionary<Type, IEventDispatcher> _dispatchersByEventType;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected CqrsAppModelBase(ICqrsClient client, bool willReceiveSnapshot)
        {
            _client = client;
            _willReceiveSnapshot = willReceiveSnapshot;
            _dispatchersByEventType = new Dictionary<Type, IEventDispatcher>();

            // ReSharper disable once DoNotCallOverridableMethodsInConstructor
            RegisterEvents(new EventRegistry(this));

            if (!_willReceiveSnapshot)
            {
                IsSnapshotComplete = true;
            }

            _client.EventsReceived += OnEventReceived;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of IDisposable

        public virtual void Dispose()
        {
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool IsSnapshotComplete { get; private set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Progress CurrentSnapshotProgress { get; private set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public event EventHandler SnapshotProgress;
        public event EventHandler SnapshotCompleted;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected abstract void RegisterEvents(IEventRegistry registry);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual void OnEventReceived(IList<IPushEvent> pushEvents)
        {
            foreach (var e in pushEvents)
            {
                IEventDispatcher dispatcher;

                if (_dispatchersByEventType.TryGetValue(e.GetType(), out dispatcher))
                {
                    dispatcher.DispatchEvent(e);
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected void NotifySnapshotCompleted(int? receivedEvents = null)
        {
            this.CurrentSnapshotProgress = new Progress(receivedEvents.GetValueOrDefault(0), receivedEvents.GetValueOrDefault(0));
            this.IsSnapshotComplete = true;

            if (SnapshotCompleted != null)
            {
                SnapshotCompleted(this, EventArgs.Empty);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected void NotifySnapshotPorgress(int? totalEvents = null, int? receivedEvents = null)
        {
            this.CurrentSnapshotProgress = new Progress(totalEvents.GetValueOrDefault(0), receivedEvents.GetValueOrDefault(0));

            if (SnapshotProgress != null)
            {
                SnapshotProgress(this, EventArgs.Empty);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void RegisterEventHandler<TEvent>(Action<TEvent> handler)
        {
            var dispatcher = (EventDispatcher<TEvent>)_dispatchersByEventType.GetOrAdd(typeof(TEvent), t => new EventDispatcher<TEvent>());
            dispatcher.Handlers += handler;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected interface IEventRegistry
        {
            IEventRegistry On<TEvent>(Action<TEvent> handler);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private interface IEventDispatcher
        {
            void DispatchEvent(IPushEvent e);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class EventRegistry : IEventRegistry
        {
            private readonly CqrsAppModelBase _owner;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public EventRegistry(CqrsAppModelBase owner)
            {
                _owner = owner;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            #region Implementation of IEventRegistry

            public IEventRegistry On<TEvent>(Action<TEvent> handler)
            {
                _owner.RegisterEventHandler<TEvent>(handler);
                return this;
            }

            #endregion
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class EventDispatcher<TEvent> : IEventDispatcher
        {
            #region Implementation of IEventDispatcher

            public void DispatchEvent(IPushEvent e)
            {
                Handlers((TEvent)e);
            }

            #endregion

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public Action<TEvent> Handlers { get; set; }
        }
    }
}
