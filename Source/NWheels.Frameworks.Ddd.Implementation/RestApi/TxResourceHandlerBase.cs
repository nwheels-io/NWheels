using NWheels.Platform.Rest;
using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Http;
using NWheels.Execution;

namespace NWheels.Frameworks.Ddd.RestApi
{
    public abstract class TxResourceHandlerBase : RestResourceHandlerBase
    {
        protected TxResourceHandlerBase(string uriPath)
            : base(uriPath)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override HttpResponseMessage OnPost(HttpRequestMessage request)
        {
            return InternalHandlePostRequest(request);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal protected abstract HttpResponseMessage InternalHandlePostRequest(HttpRequestMessage request);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override IHttpRequestReader RequestReader => DefaultHttpRequestReader.Instance;
        protected override IHttpResponseWriter ResponseWriter => DefaultHttpResponseWriter.Instance;
    }
}
