using System;
using System.Collections.Generic;
using System.IO;

namespace NWheels.Processing.Messages
{
    public interface IMessageObject
    {
        Guid MessageId { get; }
        DateTime CreatedAtUtc { get; }
        IReadOnlyCollection<IMessageHeader> Headers { get; }
        object Body { get; }
    }
}
