using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Autofac;
using NWheels.Hosting;

namespace NWheels.Processing.Messages.Impl
{
    public class ServiceBus : LifecycleEventListenerBase, IServiceBus
    {
        //private readonly IReadOnlyDictionary<Type, IMessageHandlerAdapter> _initialListHandlersByBodyType;
        private readonly IComponentContext _components;
        private readonly IServiceBusEventLogger _logger;
        private readonly BlockingCollection<IMessageObject> _messageQueue;
        private readonly Hashtable _adaptersByBodyType;
        private readonly Object _writeSyncObject;
        private CancellationTokenSource _stopRequest;
        private Thread _workerThread;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ServiceBus(
            IComponentContext components,
            IEnumerable<IMessageHandlerAdapter> registeredHandlers,
            IEnumerable<MessageTypeRegistration> registeredBodyTypes,
            IServiceBusEventLogger logger)
        {
            _components = components;
            _logger = logger;
            _writeSyncObject = new object();

            IReadOnlyDictionary<Type, IMessageHandlerAdapter>  initialListHandlersByBodyType = MapRegisteredBodyTypes(
                registeredHandlers.Distinct().ToDictionary(handler => handler.MessageBodyType),
                registeredBodyTypes);

            _adaptersByBodyType = new Hashtable();
            foreach (var entry in initialListHandlersByBodyType)
            {
                _adaptersByBodyType.Add(entry.Key, entry.Value);
            }

            _messageQueue = new BlockingCollection<IMessageObject>();
            _stopRequest = new CancellationTokenSource();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of IServiceBus

        public void EnqueueMessage(IMessageObject message)
        {
            var stopRequestCopy = _stopRequest;

            if ( stopRequestCopy == null || _stopRequest.IsCancellationRequested )
            {
                throw new InvalidOperationException();
            }

            _messageQueue.Add(message);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void DispatchMessageOnCurrentThread(IMessageObject message)
        {
            IMessageHandlerAdapter adapter = (IMessageHandlerAdapter)_adaptersByBodyType[message.Body.GetType()];
            if (adapter != null)
            {
                adapter.InvokeHandleMessage(message);
            }
            else
            {
                _logger.NoSubscribersFound(message.Body.GetType().FullName);
            }
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void SubscribeActor(object actorInstance)
        {
            lock ( _writeSyncObject )
            {
                var messageHandlerInterfaces = GetMessageHandlerInterfaces(actorInstance.GetType()).ToArray();

                foreach ( var handlerInterface in messageHandlerInterfaces )
                {
                    var messageBodyType = handlerInterface.GetGenericArguments()[0];
                    IMessageHandlerAdapter adapter = (IMessageHandlerAdapter)_adaptersByBodyType[messageBodyType];

                    _logger.DynamicSubscribeActor(actorType: actorInstance.GetType(), messageType: messageBodyType);

                    if ( adapter == null )
                    {
                        _adaptersByBodyType[messageBodyType] = adapter = CreateMessageHandlerAdapter(_components, handlerInterface);
                    }

                    adapter.RegisterMessageHandler(actorInstance);
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of LifecycleEventListenerBase

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void Load()
        {
            base.Load();
            foreach ( object o in _adaptersByBodyType.Values )
            {
                IMessageHandlerAdapter adapter = (IMessageHandlerAdapter)o;
                adapter.Initialize();
            }
        }

        public override void Activate()
        {
            _stopRequest = new CancellationTokenSource();
            _workerThread = new Thread(RunWorkerThread);
            _workerThread.Start();
        }


        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void Deactivate()
        {
            _stopRequest.Cancel();
            
            if ( !_workerThread.Join(10000) )
            {
                _logger.ServiceBusDidNotStopInTimelyFashion();
                _workerThread.Abort();
            }
            
            _workerThread = null;
            _stopRequest.Dispose();
            _stopRequest = null;
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        private void RunWorkerThread()
        {
            try
            {
                _logger.WorkerThreadStarted();

                while ( !_stopRequest.IsCancellationRequested )
                {
                    IMessageObject message;

                    try
                    {
                        if ( _messageQueue.TryTake(out message, Timeout.Infinite, _stopRequest.Token) )
                        {
                            if ( !_stopRequest.IsCancellationRequested )
                            {
                                DispatchMessage(message);
                            }
                        }
                    }
                    catch ( OperationCanceledException )
                    {
                        _logger.ListenerCanceled();
                    }
                }

                _logger.WorkerThreadStopped();
            }
            catch ( Exception e )
            {
                try
                {
                    _logger.WorkerThreadTerminatedWithUnhandledException(e);
                }
                // ReSharper disable once EmptyGeneralCatchClause
                catch
                {
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void DispatchMessage(IMessageObject message)
        {
            using ( var activity = _logger.DispatchingMessageObject(message.GetType().FullName) )
            {
                try
                {
                    DispatchMessageOnCurrentThread(message);
                }
                catch ( Exception e )
                {
                    activity.Fail(e);
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private Dictionary<Type, IMessageHandlerAdapter> MapRegisteredBodyTypes(
            Dictionary<Type, IMessageHandlerAdapter> map,
            IEnumerable<MessageTypeRegistration> bodyTypeRegistrations)
        {
            foreach ( var registration in bodyTypeRegistrations )
            {
                var hierarchy = registration.GetBodyTypeHierarchyTopDown();

                for ( int i = 1 ; i < hierarchy.Count ; i++ )
                {
                    var baseType = hierarchy[i - 1];
                    var derivedType = hierarchy[i];

                    IMessageHandlerAdapter baseAdapter;

                    if ( map.TryGetValue(baseType, out baseAdapter) && !map.ContainsKey(derivedType) )
                    {
                        map.Add(derivedType, baseAdapter);
                    }
                }
            }

            return map;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private Hashtable MapRegisteredBodyTypes_hash(
            Dictionary<Type, IMessageHandlerAdapter> map,
            IEnumerable<MessageTypeRegistration> bodyTypeRegistrations)
        {
            foreach (var registration in bodyTypeRegistrations)
            {
                var hierarchy = registration.GetBodyTypeHierarchyTopDown();

                for (int i = 1; i < hierarchy.Count; i++)
                {
                    var baseType = hierarchy[i - 1];
                    var derivedType = hierarchy[i];

                    IMessageHandlerAdapter baseAdapter;

                    if (map.TryGetValue(baseType, out baseAdapter) && !map.ContainsKey(derivedType))
                    {
                        map.Add(derivedType, baseAdapter);
                    }
                }
            }

            Hashtable result = new Hashtable();
            foreach ( var entry in map )
            {
                result.Add(entry.Key, entry.Value);
            }
            return result;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static IEnumerable<Type> GetMessageHandlerInterfaces(Type actorType)
        {
            return actorType.GetInterfaces().Where(IsMessageHandlerInterface);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static IMessageHandlerAdapter CreateMessageHandlerAdapter(IComponentContext components, Type messageHandlerInterface)
        {
            var messageType = messageHandlerInterface.GetGenericArguments()[0];
            var adapterClosedType = typeof(MessageHandlerAdapter<>).MakeGenericType(messageType);

            return (IMessageHandlerAdapter)Activator.CreateInstance(adapterClosedType, components, components.Resolve<IServiceBusEventLogger>());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static bool IsMessageHandlerInterface(Type interfaceType)
        {
            return (interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == typeof(IMessageHandler<>));
        }
    }
}
