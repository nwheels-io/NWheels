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
using NWheels.Platform.Messaging;

namespace NWheels.Samples.FirstHappyPath.CodeToGenerate
{
    /// <summary>
    /// This is a prototype of class that will be generated
    /// </summary>
    public class TxResourceHandler_of_HelloWorldTx_Hello : ResourceHandlerBase
    {
        public TxResourceHandler_of_HelloWorldTx_Hello(IInvocationScheduler scheduler) 
            : base(
                  "tx/HelloWorld/Hello",
                  CreateProtocolHandlers(scheduler))
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static IMessageProtocolInterface[] CreateProtocolHandlers(IInvocationScheduler scheduler)
        {
            var channel = scheduler.GetInvocationChannel(null, null);

            return new IMessageProtocolInterface[] {
                new Protocol_Http_Rest_NWheelsApi(channel)
            };
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class Protocol_Http_Rest_NWheelsApi : HttpResourceProtocolBase
        {
            private readonly IInvocationChannel _invocationChannel;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public Protocol_Http_Rest_NWheelsApi(IInvocationChannel invocationChannel) 
                : base(protocolName: "http/rest/nwheels/v1")
            {
                _invocationChannel = invocationChannel;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected override async Task HttpPost(HttpContext context)
            {
                var invocation = new InvocationMessage_of_HelloWorldTx_Hello();

                try
                {
                    using (var reader = new StreamReader(context.Request.Body))
                    {
                        using (var json = new JsonTextReader(reader))
                        {
                            if (!json.Read() || !InvocationMessage_of_HelloWorldTx_Hello.InputMessageSerializer.DeserializeFromJson(json, ref invocation.Input))
                            {
                                //TODO: log error
                                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                                return;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    //TODO: log error
                    Console.Error.WriteLine(e.ToString());
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                }

                try
                {
                    await _invocationChannel.ScheduledInvoke(invocation);

                    using (var writer = new StreamWriter(context.Response.Body))
                    {
                        var json = new JsonTextWriter(writer);
                        InvocationMessage_of_HelloWorldTx_Hello.OutputMessageSerializer.SerializeToJson(json, ref invocation.Output);
                        json.Flush();
                        writer.Flush();
                    }
                }
                catch (Exception e)
                {
                    //TODO: log error
                    Console.Error.WriteLine(e.ToString());
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                }
            }
        }
    }
}
