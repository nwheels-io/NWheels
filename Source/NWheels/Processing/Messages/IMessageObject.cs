using System.Collections.Generic;
using System.IO;

namespace NWheels.Processing.Messages
{
    public interface IMessageObject
    {
        IReadOnlyCollection<IMessageHeader> Headers { get; }
        object Body { get; }
    }
}
