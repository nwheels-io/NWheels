using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NWheels.Hosting;

namespace NWheels.Processing.Messages.Impl
{
    public class ServiceBus : LifecycleEventListenerBase, IServiceBus
    {
        private readonly IReadOnlyDictionary<Type, IMessageHandlerAdapter> _handlersByBodyType;
        private readonly IServiceBusEventLogger _logger;
        private readonly BlockingCollection<IMessageObject> _messageQueue;
        private CancellationTokenSource _stopRequest;
        private Thread _workerThread;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ServiceBus(IEnumerable<IMessageHandlerAdapter> registeredHandlers, IServiceBusEventLogger logger)
        {
            _logger = logger;
            _handlersByBodyType = registeredHandlers.ToDictionary(handler => handler.MessageBodyType);
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
            IMessageHandlerAdapter adapter;

            if ( _handlersByBodyType.TryGetValue(message.Body.GetType(), out adapter) )
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

        #region Overrides of LifecycleEventListenerBase

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
    }
}
