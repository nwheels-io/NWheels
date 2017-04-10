using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace NWheels.Platform.Rest
{
    public interface IHttpRequestReader
    {
        T DeserializeBodyJson<T>(HttpRequestMessage request)
            where T : new();
    }
}
