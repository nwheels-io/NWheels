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
    public abstract class HttpResourceProtocolBase : MessageProtocolBase, IHttpMessageProtocolInterface
    {
        private static readonly HttpMethod _s_patchMethod = new HttpMethod("PATCH");

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected HttpResourceProtocolBase(string protocolName)
            : base(typeof(IHttpMessageProtocolInterface), protocolName)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        Task IHttpMessageProtocolInterface.HandleRequest(HttpContext context)
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
                else if (method == _s_patchMethod.Method)
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

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual Task HttpDelete(HttpContext context)
        {
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return Task.CompletedTask;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual Task HttpGet(HttpContext context)
        {
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return Task.CompletedTask;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual Task HttpPatch(HttpContext context)
        {
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return Task.CompletedTask;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual Task HttpPost(HttpContext context)
        {
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return Task.CompletedTask;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual Task HttpPut(HttpContext context)
        {
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return Task.CompletedTask;
        }
    }
}
