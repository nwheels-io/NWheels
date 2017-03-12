using NWheels.Platform.Rest;
using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Http;

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
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected abstract void OnReadInput(HttpRequestMessage request);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected abstract void OnInvokeTx();

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected abstract void OnWriteOutput(HttpResponseMessage response);
    }
}
