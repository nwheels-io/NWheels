#if false

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace NWheels.Platform.Rest
{
    public class DefaultHttpRequestReader : IHttpRequestReader
    {
        public T DeserializeBodyJson<T>(HttpRequestMessage request)
            where T : new()
        {
            var jsonString = request.Content.ReadAsStringAsync().Result;
            var data = new T();
            JsonConvert.PopulateObject(jsonString, data);
            return data;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static readonly DefaultHttpRequestReader _s_instance = new DefaultHttpRequestReader();

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static DefaultHttpRequestReader Instance => _s_instance;
    }
}

#endif