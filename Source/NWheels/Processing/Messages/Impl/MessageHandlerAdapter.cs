using System;
using System.Collections.Generic;
using Autofac;

namespace NWheels.Processing.Messages.Impl
{
    internal class MessageHandlerAdapter<TMessage> : IMessageHandlerAdapter
        where TMessage : class, IMessageObject
    {
        private IComponentContext _components;
        private readonly IServiceBusEventLogger _logger;

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public MessageHandlerAdapter(IComponentContext components, IServiceBusEventLogger logger)
        {
            _logger = logger;
            _components = components;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of IMessageHandlerAdapter

        public void InvokeHandleMessage(IMessageObject message)
        {
            IEnumerable<IMessageHandler<TMessage>> allActors;

            try
            {
                allActors = _components.Resolve<IEnumerable<IMessageHandler<TMessage>>>();
            }
            catch ( Exception e )
            {
                throw _logger.FailedToObtainActorInstance(typeof(TMessage).FullName, e);
            }

            var exceptions = new List<Exception>();

            foreach ( var singleActor in allActors )
            {
                InvokeActor(singleActor, message, exceptions);
            }

            if ( exceptions.Count > 0 )
            {
                throw _logger.ErrorsWhileHandlingMessage(typeof(TMessage).FullName, new AggregateException(exceptions));
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public Type MessageType
        {
            get { return typeof(TMessage); }
        }

        #endregion
        
        //-------------------------------------------------------------------------------------------------------------------------------------------------

        private void InvokeActor(IMessageHandler<TMessage> actorInstance, IMessageObject message, List<Exception> exceptions)
        {
            using ( var activity = _logger.InvokingActor(actorInstance.GetType().FullName, typeof(TMessage).FullName) )
            {
                try
                {
                    actorInstance.HandleMessage((TMessage)message);
                }
                catch ( Exception e )
                {
                    activity.Fail(e);
                    exceptions.Add(_logger.ActorFailed(actorInstance.GetType().FullName, typeof(TMessage).FullName, e));
                }
            }
        }
    }
}