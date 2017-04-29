using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Platform.Rest
{
    public interface IRestResourceHandler
    {
        Task HttpGet(HttpContext context);
        Task HttpPost(HttpContext context);
        Task HttpPut(HttpContext context);
        Task HttpPatch(HttpContext context);
        Task HttpDelete(HttpContext context);

        string UriPath { get; }
    }
}
