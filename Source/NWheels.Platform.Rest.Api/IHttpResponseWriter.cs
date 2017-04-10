using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace NWheels.Platform.Rest
{
    public interface IHttpResponseWriter
    {
        HttpResponseMessage CreateWithJsonBody<T>(T data);
    }
}
