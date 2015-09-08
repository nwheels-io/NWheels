using System;
using System.Collections.Generic;
using Autofac;

namespace NWheels.Processing.Messages.Impl
{
    internal class MessageHandlerAdapter<TBody> : IMessageHandlerAdapter, IEquatable<IMessageHandlerAdapter>
        where TBody : class
    {
        private readonly IComponentContext _components;
        private readonly IServiceBusEventLogger _logger;

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public MessageHandlerAdapter(IComponentContext components, IServiceBusEventLogger logger)
        {
            _components = components;
            _logger = logger;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of IMessageHandlerAdapter

        public void InvokeHandleMessage(IMessageObject message)
        {
            IEnumerable<IMessageHandler<TBody>> allActors;

            try
            {
                allActors = _components.Resolve<IEnumerable<IMessageHandler<TBody>>>();
            }
            catch ( Exception e )
            {
                throw _logger.FailedToObtainActorInstance(typeof(TBody).FullName, e);
            }

            var exceptions = new List<Exception>();

            foreach ( var singleActor in allActors )
            {
                InvokeActor(singleActor, message, exceptions);
            }

            AggregateException aggregatedError = (exceptions.Count > 0 ? new AggregateException(exceptions).Flatten() : null);
            SetMessageResult(message as ISetMessageResult, aggregatedError);

            if ( aggregatedError != null )
            {
                throw _logger.ErrorsWhileHandlingMessage(typeof(TBody).FullName, aggregatedError);
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public Type MessageBodyType
        {
            get { return typeof(TBody); }
        }

        #endregion

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of IEquatable<IMessageHandlerAdapter>

        public bool Equals(IMessageHandlerAdapter other)
        {
            return (other != null && other.MessageBodyType == this.MessageBodyType);
        }

        #endregion

        //-------------------------------------------------------------------------------------------------------------------------------------------------
        
        #region Overrides of Object

        public override bool Equals(object obj)
        {
            var other = obj as IMessageHandlerAdapter;

            if ( other != null )
            {
                return this.Equals(other);
            }
            else
            {
                return false;
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public override int GetHashCode()
        {
            return this.MessageBodyType.GetHashCode();
        }

        #endregion

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        private void InvokeActor(IMessageHandler<TBody> actorInstance, IMessageObject message, List<Exception> exceptions)
        {
            using ( var activity = _logger.InvokingActor(actorInstance.GetType().FullName, typeof(TBody).FullName) )
            {
                try
                {
                    actorInstance.HandleMessage((TBody)message.Body);
                }
                catch ( Exception e )
                {
                    activity.Fail(e);
                    exceptions.Add(_logger.ActorFailed(actorInstance.GetType().FullName, typeof(TBody).FullName, e));
                }
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        private void SetMessageResult(ISetMessageResult messageWithResult, Exception error)
        {
            if ( messageWithResult == null )
            {
                _logger.MessageDoesNotSupportContinuation();
                return;
            }

            var result = (error != null ? MessageResult.ProcessingFailed : MessageResult.Processed);
            _logger.SettingMessageResult(result, error);

            Action continuation;
            messageWithResult.SetMessageResult(result, error, out continuation);

            if ( continuation != null )
            {
                InvokeContinuationCallback(continuation);
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        private void InvokeContinuationCallback(Action continuation)
        {
            using ( var activity = _logger.InvokingContinuation(continuation.Method) )
            {
                try
                {
                    continuation();
                }
                catch ( Exception e )
                {
                    activity.Fail(e);
                    _logger.ContinuationCallbackFailed(continuation.Method, e);
                }
            }
        }
    }
}
