using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Processing.Messages
{
    public interface IMessageHandler
    {
    }

    //-----------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IMessageHandler<TMessage> : IMessageHandler
        where TMessage : class, IMessageObject
    {
        void HandleMessage(TMessage message);
    }
}
