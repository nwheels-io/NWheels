using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace NWheels.Platform.Rest
{
    public interface IRestResourceHandler
    {
        HttpResponseMessage Get(HttpRequestMessage request);
        HttpResponseMessage Post(HttpRequestMessage request);
        HttpResponseMessage Put(HttpRequestMessage request);
        HttpResponseMessage Patch(HttpRequestMessage request);
        HttpResponseMessage Delete(HttpRequestMessage request);

        string UriPath { get; }
    }
}
