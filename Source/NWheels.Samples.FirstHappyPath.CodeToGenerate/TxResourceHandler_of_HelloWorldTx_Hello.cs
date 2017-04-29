using NWheels.Frameworks.Ddd.RestApi;
using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Http;
using NWheels.Platform.Rest;
using NWheels.Samples.FirstHappyPath.Domain;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using NWheels.Execution;
using System.IO;
using Newtonsoft.Json;
using System.Net;

namespace NWheels.Samples.FirstHappyPath.CodeToGenerate
{
    /// <summary>
    /// This is a prototype of class that will be generated
    /// </summary>
    public class TxResourceHandler_of_HelloWorldTx_Hello : RestResourceHandlerBase
    {
        private readonly HelloWorldTx _tx;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TxResourceHandler_of_HelloWorldTx_Hello(HelloWorldTx tx) 
            : base("tx/HelloWorld/Hello")
        {
            _tx = tx;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override async Task HttpPost(HttpContext context)
        {
            try
            {
                var invocation = new InvocationMessage_of_HelloWorldTx_Hello();
                if (!TryReadRequest(context.Request, invocation))
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return;
                }

                await ((IInvocationMessage)invocation).Invoke(_tx);

                WriteResponse(invocation, context.Response);
            }
            catch //(Exception e)
            {
                //TODO: log exception
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private bool TryReadRequest(HttpRequest request, InvocationMessage_of_HelloWorldTx_Hello invocation)
        {
            using (var reader = new StreamReader(request.Body))
            {
                using (var json = new JsonTextReader(reader))
                {
                    if (!json.Read() || json.TokenType != JsonToken.StartObject)
                    {
                        return false;
                    }
                    if (!json.Read() || json.TokenType != JsonToken.PropertyName)
                    {
                        return false;
                    }
                    if (json.ReadAsString() != "name")
                    {
                        return false;
                    }
                    if (json.TokenType != JsonToken.String)
                    {
                        return false;
                    }

                    invocation.Name = json.ReadAsString();

                    if (json.TokenType != JsonToken.EndObject)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void WriteResponse(InvocationMessage_of_HelloWorldTx_Hello invocation, HttpResponse response)
        {
            using (var writer = new StreamWriter(response.Body))
            {
                using (var json = new JsonTextWriter(writer))
                {
                    json.WriteStartObject();
                    json.WritePropertyName("result");
                    json.WriteValue(invocation.Result);
                    json.WriteEndObject();
                    json.Flush();
                }

                writer.Flush();
            }
        }
    }
}
