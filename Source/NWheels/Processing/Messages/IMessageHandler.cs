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

    public interface IMessageHandler<TBody> : IMessageHandler
        where TBody : class
    {
        void HandleMessage(TBody message);
    }
}
