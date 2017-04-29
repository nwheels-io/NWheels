using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Platform.Rest
{
    public abstract class RestResourceHandlerBase : IRestResourceHandler
    {
        protected RestResourceHandlerBase(string uriPath)
        {
            this.UriPath = uriPath;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual Task HttpDelete(HttpContext context)
        {
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return Task.CompletedTask;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual Task HttpGet(HttpContext context)
        {
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return Task.CompletedTask;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual Task HttpPatch(HttpContext context)
        {
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return Task.CompletedTask;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual Task HttpPost(HttpContext context)
        {
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return Task.CompletedTask;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual Task HttpPut(HttpContext context)
        {
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return Task.CompletedTask;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string UriPath { get; }
    }
}
