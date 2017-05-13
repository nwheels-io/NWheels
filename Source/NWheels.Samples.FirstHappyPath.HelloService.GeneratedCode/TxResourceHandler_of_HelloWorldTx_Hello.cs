using NWheels.Frameworks.Ddd.RestApi;
using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Http;
using NWheels.Platform.Rest;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using NWheels.Execution;
using System.IO;
using Newtonsoft.Json;
using System.Net;
using NWheels.Platform.Messaging;

namespace NWheels.Samples.FirstHappyPath.HelloService
{
    /// <summary>
    /// This is a prototype of class that will be generated
    /// </summary>
    public class TxResourceHandler_of_HelloWorldTx_Hello : ResourceHandlerBase
    {
        public TxResourceHandler_of_HelloWorldTx_Hello(IInvocationScheduler scheduler) 
            : base(
                  "tx/HelloWorld/Hello",
                  CreateProtocolInterfaces(scheduler))
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static IMessageProtocolInterface[] CreateProtocolInterfaces(IInvocationScheduler scheduler)
        {
            var channel = scheduler.GetInvocationChannel(null, null);

            return new IMessageProtocolInterface[] {
                new Protocol_HttpRestNWheelsV1(channel)
            };
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class Protocol_HttpRestNWheelsV1 : HttpResourceProtocolBase.HttpInterfaceBase
        {
            private readonly IInvocationChannel _invocationChannel;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public Protocol_HttpRestNWheelsV1(IInvocationChannel invocationChannel) 
                : base(HttpRestNWheelsV1Protocol.ProtocolNameString)
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
