using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace NWheels.Platform.Messaging
{
    public class HttpMessageConverter
    {
        public static async Task WriteAsync(HttpResponseMessage from, HttpResponse to)
        {
            using (from)
            {
                to.StatusCode = (int)from.StatusCode;

                //var responseFeature = context.HttpContext.Features.Get<IHttpResponseFeature>();
                //if (responseFeature != null)
                //{
                //    responseFeature.ReasonPhrase = responseMessage.ReasonPhrase;
                //}

                var responseHeaders = from.Headers;

                // Ignore the Transfer-Encoding header if it is just "chunked".
                // We let the host decide about whether the response should be chunked or not.
                if (responseHeaders.TransferEncodingChunked == true &&
                    responseHeaders.TransferEncoding.Count == 1)
                {
                    responseHeaders.TransferEncoding.Clear();
                }

                foreach (var header in responseHeaders)
                {
                    to.Headers.Append(header.Key, header.Value.ToArray());
                }

                if (from.Content != null)
                {
                    var contentHeaders = from.Content.Headers;

                    // Copy the response content headers only after ensuring they are complete.
                    // We ask for Content-Length first because HttpContent lazily computes this
                    // and only afterwards writes the value into the content headers.
                    var unused = contentHeaders.ContentLength;

                    foreach (var header in contentHeaders)
                    {
                        to.Headers.Append(header.Key, header.Value.ToArray());
                    }

                    await from.Content.CopyToAsync(to.Body);
                }
            }
        }
    }
}
