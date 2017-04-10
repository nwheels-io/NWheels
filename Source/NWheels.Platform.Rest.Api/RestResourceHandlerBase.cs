using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;

namespace NWheels.Platform.Rest
{
    public abstract class RestResourceHandlerBase : IRestResourceHandler
    {
        protected RestResourceHandlerBase(string uriPath)
        {
            this.UriPath = UriPath;
        }
        
        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        HttpResponseMessage IRestResourceHandler.Get(HttpRequestMessage request)
        {
            try
            {
                return OnGet(request);
            }
            catch (Exception e)
            {
                //TODO: provide error handling logic
                Console.Error.WriteLine(e.ToString());
                return new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        HttpResponseMessage IRestResourceHandler.Post(HttpRequestMessage request)
        {
            try
            {
                return OnPost(request);
            }
            catch (Exception e)
            {
                //TODO: provide error handling logic
                Console.Error.WriteLine(e.ToString());
                return new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        HttpResponseMessage IRestResourceHandler.Put(HttpRequestMessage request)
        {
            try
            {
                return OnPut(request);
            }
            catch (Exception e)
            {
                //TODO: provide error handling logic
                Console.Error.WriteLine(e.ToString());
                return new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        HttpResponseMessage IRestResourceHandler.Patch(HttpRequestMessage request)
        {
            try
            {
                return OnPatch(request);
            }
            catch (Exception e)
            {
                //TODO: provide error handling logic
                Console.Error.WriteLine(e.ToString());
                return new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        HttpResponseMessage IRestResourceHandler.Delete(HttpRequestMessage request)
        {
            try
            {
                return OnDelete(request);
            }
            catch (Exception e)
            {
                //TODO: provide error handling logic
                Console.Error.WriteLine(e.ToString());
                return new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string UriPath { get; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal protected virtual HttpResponseMessage OnGet(HttpRequestMessage request)
        {
            return new HttpResponseMessage(HttpStatusCode.BadRequest);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal protected virtual HttpResponseMessage OnPost(HttpRequestMessage request)
        {
            return new HttpResponseMessage(HttpStatusCode.BadRequest);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal protected virtual HttpResponseMessage OnPut(HttpRequestMessage request)
        {
            return new HttpResponseMessage(HttpStatusCode.BadRequest);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal protected virtual HttpResponseMessage OnPatch(HttpRequestMessage request)
        {
            return new HttpResponseMessage(HttpStatusCode.BadRequest);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal protected virtual HttpResponseMessage OnDelete(HttpRequestMessage request)
        {
            return new HttpResponseMessage(HttpStatusCode.BadRequest);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected abstract IHttpRequestReader RequestReader { get; }
        protected abstract IHttpResponseWriter ResponseWriter { get; }
    }
}
