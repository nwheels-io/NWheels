using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Platform.Messaging
{
    public interface IEndpoint
    {
        string Name { get; }
        string ProtocolFamily { get; }
        IEnumerable<string> ListenUrls { get; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IEndpoint<TContext> : IEndpoint
    {
        void Subscribe(Func<TContext, Task> handler);
    }
}
