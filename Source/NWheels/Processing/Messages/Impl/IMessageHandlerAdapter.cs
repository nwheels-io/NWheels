using System;

namespace NWheels.Processing.Messages.Impl
{
    public interface IMessageHandlerAdapter
    {
        void InvokeHandleMessage(IMessageObject message);
        Type MessageBodyType { get; }
    }
}