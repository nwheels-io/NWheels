using NWheels.Frameworks.Ddd.RestApi;
using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Http;
using NWheels.Platform.Rest;
using NWheels.Samples.FirstHappyPath.Domain;

namespace NWheels.Samples.FirstHappyPath.CodeToGenerate
{
    /// <summary>
    /// This is a prototype of class that will be generated
    /// </summary>
    public class TxResourceHandler_of_HelloWorldTx_Hello : TxResourceHandlerBase
    {
        private readonly HelloWorldTx _tx;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TxResourceHandler_of_HelloWorldTx_Hello(HelloWorldTx tx) 
            : base("tx/HelloWorld/Hello")
        {
            _tx = tx;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override HttpResponseMessage InternalHandlePostRequest(HttpRequestMessage request)
        {
            // here we should only handle the happy path
            // RestResourceHandlerBase implements proper error handling, for the case something goes wrong

            HelloRequestBody requestBody = RequestReader.DeserializeBodyJson<HelloRequestBody>(request);
            var result = _tx.Hello(requestBody.Name).Result;
            var responseBody = new HelloResponseBody();
            responseBody.Result = result;
            return ResponseWriter.CreateWithJsonBody<HelloResponseBody>(responseBody);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private struct HelloRequestBody
        {
            public string Name;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private struct HelloResponseBody
        {
            public string Result;
        }
    }
}
