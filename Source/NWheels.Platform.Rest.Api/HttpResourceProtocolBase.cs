using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Net;
using System.Net.Http;
using NWheels.Platform.Messaging;

namespace NWheels.Platform.Rest
{
    public abstract class HttpResourceProtocolBase : MessageProtocol<IHttpMessageProtocolInterface>
    {
        public static readonly HttpMethod PatchMethod = new HttpMethod("PATCH");

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected HttpResourceProtocolBase(string protocolName)
            : base(protocolName)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override IHttpMessageProtocolInterface GetConcreteInterface()
        {
            throw new NotSupportedException(
                "This is an abstract protocol. Generated resource handlers provide concrete implementation per speciifc resource.");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override bool IsConcreteProtocol => false;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public abstract class HttpInterfaceBase : IHttpMessageProtocolInterface
        {
            protected HttpInterfaceBase(string protocolName)
            {
                this.ProtocolInterface = typeof(IHttpMessageProtocolInterface);
                this.ProtocolName = protocolName;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public Task HandleRequest(HttpContext context)
            {
                try
                {
                    var method = context.Request.Method;

                    if (method == HttpMethod.Get.Method)
                    {
                        return HttpGet(context);
                    }
                    else if (method == HttpMethod.Post.Method)
                    {
                        return HttpPost(context);
                    }
                    else if (method == HttpMethod.Put.Method)
                    {
                        return HttpPut(context);
                    }
                    else if (method == PatchMethod.Method)
                    {
                        return HttpPatch(context);
                    }
                    else if (method == HttpMethod.Delete.Method)
                    {
                        return HttpDelete(context);
                    }
                    else
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    }
                }
                catch (Exception)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                }

                return Task.CompletedTask;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public Type ProtocolInterface { get; }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public string ProtocolName { get; }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected virtual Task HttpDelete(HttpContext context)
            {
                return BadRequest(context);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected virtual Task HttpGet(HttpContext context)
            {
                return BadRequest(context);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected virtual Task HttpPatch(HttpContext context)
            {
                return BadRequest(context);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected virtual Task HttpPost(HttpContext context)
            {
                return BadRequest(context);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected virtual Task HttpPut(HttpContext context)
            {
                return BadRequest(context);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected virtual Task BadRequest(HttpContext context)
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Task.CompletedTask;
            }
        }
    }
}
