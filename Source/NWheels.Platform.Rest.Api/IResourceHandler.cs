using Microsoft.AspNetCore.Http;
using NWheels.Platform.Messaging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Platform.Rest
{
    public interface IResourceHandler
    {
        TProtocolInterface GetProtocol<TProtocolInterface>(string protocolName)
            where TProtocolInterface : class, IMessageProtocolInterface;

        bool TryGetProtocol<TProtocolInterface>(string protocolName, out TProtocolInterface protocol)
            where TProtocolInterface : class, IMessageProtocolInterface;

        IEnumerable<IMessageProtocolInterface> GetAllProtocolHandlers();

        string UriPath { get; }
    }
}
