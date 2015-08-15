using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Processing.Messages
{
    public enum MessageResult
    {
        Processed,
        ProcessingFailed,
        ProcessingTimedOut,
        DeliveryFailed,
        DeliveryTimedOut
    }
}
