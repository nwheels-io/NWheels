using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Processing.Messages
{
    public interface IServiceBus
    {
        void EnqueueMessage(IMessageObject message);
    }
}
