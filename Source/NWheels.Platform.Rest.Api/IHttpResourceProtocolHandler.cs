using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Platform.Rest
{
    public interface IHttpResourceProtocolHandler : IResourceProtocolHandler
    {
        Task HandleHttpRequest(HttpContext context);
    }
}
