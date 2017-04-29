#if false

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;

namespace NWheels.Platform.Rest
{
    public class DefaultHttpResponseWriter : IHttpResponseWriter
    {
        public HttpResponseMessage CreateWithJsonBody<T>(T data)
        {
            //TODO: use a faster alternative to Json.NET?
            //TODO: avoid buffering into a string

            var responseJsonString = JsonConvert.SerializeObject(data);

            return new HttpResponseMessage() {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseJsonString, Encoding.UTF8, "application/json")
            };
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static readonly DefaultHttpResponseWriter _s_instance = new DefaultHttpResponseWriter();

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static DefaultHttpResponseWriter Instance => _s_instance;
    }
}


#endif