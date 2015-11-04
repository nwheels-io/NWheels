using System;

namespace NWheels.Processing.Messages.Impl
{
    public interface IMessageHandlerAdapter
    {
        void Initialize();
        void InvokeHandleMessage(IMessageObject message);
        Type MessageBodyType { get; }
        void RegisterMessageHandler(object actorInstance);
    }
}