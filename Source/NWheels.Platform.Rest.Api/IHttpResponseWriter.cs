using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace NWheels.Platform.Rest
{
    public interface IHttpResponseWriter
    {
        void SerializeToJsonBody<T>(HttpResponse response, T data);
    }
}
