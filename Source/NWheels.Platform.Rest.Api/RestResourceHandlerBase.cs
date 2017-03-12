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
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        HttpResponseMessage IRestResourceHandler.Post(HttpRequestMessage request)
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        HttpResponseMessage IRestResourceHandler.Put(HttpRequestMessage request)
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        HttpResponseMessage IRestResourceHandler.Patch(HttpRequestMessage request)
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        HttpResponseMessage IRestResourceHandler.Delete(HttpRequestMessage request)
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string UriPath { get; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual HttpResponseMessage OnGet(HttpRequestMessage request)
        {
            return new HttpResponseMessage(HttpStatusCode.BadRequest);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual HttpResponseMessage OnPost(HttpRequestMessage request)
        {
            return new HttpResponseMessage(HttpStatusCode.BadRequest);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual HttpResponseMessage OnPut(HttpRequestMessage request)
        {
            return new HttpResponseMessage(HttpStatusCode.BadRequest);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual HttpResponseMessage OnPatch(HttpRequestMessage request)
        {
            return new HttpResponseMessage(HttpStatusCode.BadRequest);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual HttpResponseMessage OnDelete(HttpRequestMessage request)
        {
            return new HttpResponseMessage(HttpStatusCode.BadRequest);
        }
    }
}
