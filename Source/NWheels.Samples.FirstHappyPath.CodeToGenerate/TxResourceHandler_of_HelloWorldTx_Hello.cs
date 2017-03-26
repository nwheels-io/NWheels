using NWheels.Frameworks.Ddd.RestApi;
using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Http;

namespace NWheels.Samples.FirstHappyPath.CodeToGenerate
{
    public class TxResourceHandler_of_HelloWorldTx_Hello : TxResourceHandlerBase
    {
        public TxResourceHandler_of_HelloWorldTx_Hello() 
            : base("tx/HelloWorld/Hello")
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override HttpResponseMessage InternalHandlePostRequest(HttpRequestMessage request)
        {
            
        }
    }
}
