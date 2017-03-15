#if false
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Platform.Messaging
{
    public interface IMiddleware
    {
        Task Invoke(HttpContext httpContext);
    }
}
#endif