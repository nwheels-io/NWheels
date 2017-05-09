using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Platform.Messaging
{
    public interface IHttpMessageProtocolInterface : IMessageProtocolInterface
    {
        Task HandleRequest(HttpContext context);
    }
}
