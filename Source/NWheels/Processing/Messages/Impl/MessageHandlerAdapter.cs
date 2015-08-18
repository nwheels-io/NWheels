using System;
using System.Collections.Generic;
using Autofac;

namespace NWheels.Processing.Messages.Impl
{
    internal class MessageHandlerAdapter<TBody> : IMessageHandlerAdapter
        where TBody : class
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

            if ( exceptions.Count > 0 )
            {
                throw _logger.ErrorsWhileHandlingMessage(typeof(TBody).FullName, new AggregateException(exceptions));
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public Type MessageBodyType
        {
            get { return typeof(TBody); }
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
    }
}