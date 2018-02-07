using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace NWheels.Testability
{
    public static class HttpAssert
    {
        public static Task<string> MakeLocalHttpRequest(
            int port,
            HttpMethod method,
            string path,
            string content = null,
            string contentType = "application/json",
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
            string expectedContentType = "application/json",
            TimeSpan? timeout = null)
        {
            using (var handler = new HttpClientHandler())
            {
                var origin = $"http://localhost:{port}";
                return MakeRequest(handler, origin, method, path, content, contentType, expectedStatusCode, expectedContentType, timeout);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static async Task<string> MakeRequest(
            HttpMessageHandler handler,
            string origin,
            HttpMethod method,
            string path,
            string content = null,
            string contentType = "application/json",
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
            string expectedContentType = "application/json",
            TimeSpan? timeout = null)
        {
            using (var client = new HttpClient(handler))
            {
                var requestUri = $"{origin}/{path.TrimStart('/')}";
                var request = new HttpRequestMessage(method, requestUri);

                if (method != HttpMethod.Get)
                {
                    request.Content = new StringContent(content, Encoding.UTF8, contentType);
                }

                var httpTask = client.SendAsync(request, HttpCompletionOption.ResponseContentRead, CancellationToken.None);
                var effectiveTimeout = timeout.GetValueOrDefault(TimeSpan.FromSeconds(10));

                if (await Task.WhenAny(httpTask, Task.Delay(effectiveTimeout)) != httpTask)
                {
                    Assert.True(false, "HTTP request didn't complete within allotted timeout.");
                }

                var response = httpTask.Result;
                response.StatusCode.Should().Be(expectedStatusCode, because: $"requet must complete with status {expectedStatusCode}");

                if (expectedContentType != null)
                {
                    response.Content?.Headers?.ContentType?.MediaType.Should().Be(expectedContentType, because: $"response must be '{expectedContentType}'");
                }

                var responseText = response?.Content?.ReadAsStringAsync().Result;
                return responseText;
            }
        }
    }
}
