using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NWheels.Testability.Extensions;
using Xunit;

namespace NWheels.Testability.Tests.Unit
{
    public class HttpAssertTests : TestBase.UnitTest
    {
        private class MockHttpHandler : HttpMessageHandler
        {
            private readonly Func<HttpRequestMessage, Task<HttpResponseMessage>> _onRequest;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public MockHttpHandler(Func<HttpRequestMessage, Task<HttpResponseMessage>> onRequest)
            {
                _onRequest = onRequest;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return _onRequest(request);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public async Task MakeRequest_Get_RequestCorrectlyComposed()
        {
            //-- arrange

            var mockHandler = new MockHttpHandler(
                request => {
                    request.RequestUri.ToString().Should().Be("https://test.host:12345/example/path");
                    request.Method.Should().Be(HttpMethod.Get);
                    request.Content.Should().BeNull();

                    return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
                });

            //-- act & assert

            await HttpAssert.MakeRequest(
                mockHandler,
                origin: "https://test.host:12345",
                method: HttpMethod.Get,
                path: "example/path",
                expectedContentType: null);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public async Task MakeRequest_NonGet_RequestCorrectlyComposed()
        {
            //-- arrange

            var mockHandler = new MockHttpHandler(
                request => {
                    request.RequestUri.ToString().Should().Be("http://test.host/example/path?q=v");
                    request.Method.Should().Be(HttpMethod.Put);
                    request.Content.Should().NotBeNull();
                    request.Content.Headers.ContentType.Should().NotBeNull();
                    request.Content.Headers.ContentType.MediaType.Should().Be("example/test");
                    request.Content.ReadAsStringAsync().Result.Should().Be("TEST-REQUEST-BODY");

                    return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
                });

            //-- act & assert

            await HttpAssert.MakeRequest(
                mockHandler,
                origin: "http://test.host",
                method: HttpMethod.Put,
                path: "example/path?q=v",
                content: "TEST-REQUEST-BODY",
                contentType: "example/test",
                expectedContentType: null);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public async Task MakeRequest_ResponseAsExpected_Pass()
        {
            //-- arrange

            var mockHandler = new MockHttpHandler(
                request => {
                    var response = new HttpResponseMessage(HttpStatusCode.Created);
                    response.Content = new StringContent("TEST-RESPONSE-BODY", Encoding.UTF8, "test/response");
                    return Task.FromResult(response);
                });

            //-- act & assert

            var responseContent = await HttpAssert.MakeRequest(
                mockHandler,
                origin: "http://test.host",
                method: HttpMethod.Post,
                path: "example/path",
                content: "TEST-REQUEST-BODY",
                contentType: "test/request",
                expectedStatusCode: HttpStatusCode.Created,
                expectedContentType: "test/response");

            responseContent.Should().Be("TEST-RESPONSE-BODY");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public async Task MakeRequest_WrongHttpStatus_Fail()
        {
            //-- arrange

            var mockHandler = new MockHttpHandler(
                request => {
                    var response = new HttpResponseMessage(HttpStatusCode.Accepted);
                    response.Content = new StringContent("TEST-RESPONSE-BODY", Encoding.UTF8, "test/response");
                    return Task.FromResult(response);
                });

            //-- act & assert

            Func<Task> act = async () => {
                await HttpAssert.MakeRequest(
                    mockHandler,
                    origin: "http://test.host",
                    method: HttpMethod.Get,
                    path: "example/path",
                    expectedStatusCode: HttpStatusCode.Created
                );
            };

            var exception = await act.ShouldThrowExceptionAsync<Xunit.Sdk.XunitException>();
            exception.Message.Should().Contain("because requet must complete with status Created");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public async Task MakeRequest_WrongResponseContentType_Fail()
        {
            //-- arrange

            var mockHandler = new MockHttpHandler(
                request => {
                    var response = new HttpResponseMessage(HttpStatusCode.OK);
                    response.Content = new StringContent("TEST-RESPONSE-BODY", Encoding.UTF8, "err/bad");
                    return Task.FromResult(response);
                });

            //-- act & assert

            Func<Task> act = async () => {
                await HttpAssert.MakeRequest(
                    mockHandler,
                    origin: "http://test.host",
                    method: HttpMethod.Get,
                    path: "example/path",
                    expectedStatusCode: HttpStatusCode.OK,
                    expectedContentType: "test/response"
                );
            };

            var exception = await act.ShouldThrowExceptionAsync<Xunit.Sdk.XunitException>();
            exception.Message.Should().Contain("because response must be 'test/response'");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public async Task MakeRequest_CompleteWithinTimeout_Pass()
        {
            //-- arrange

            var mockHandler = new MockHttpHandler(
                async request => {
                    await Task.Delay(10);
                    return new HttpResponseMessage(HttpStatusCode.OK) {
                        Content = new StringContent("TEST-RESPONSE-BODY", Encoding.UTF8, "test/response")
                    };
                });

            //-- act & assert

            var clock = Stopwatch.StartNew();

            var responseContent = await HttpAssert.MakeRequest(
                mockHandler,
                origin: "http://test.host",
                method: HttpMethod.Post,
                path: "example/path",
                content: "TEST-REQUEST-BODY",
                contentType: "test/request",
                expectedStatusCode: HttpStatusCode.OK,
                expectedContentType: "test/response",
                timeout: TimeSpan.FromMilliseconds(20000));

            Console.WriteLine($"MakeRequest_CompleteWithinTimeout_Pass: elapsed time = {clock.Elapsed}");

            responseContent.Should().Be("TEST-RESPONSE-BODY");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public async Task MakeRequest_TakeLongerThanTimeout_Fail()
        {
            //-- arrange

            var mockHandler = new MockHttpHandler(
                async request => {
                    await Task.Delay(200);
                    return new HttpResponseMessage(HttpStatusCode.OK);
                });

            //-- act & assert

            Func<Task> act = async () => {
                await HttpAssert.MakeRequest(
                    mockHandler,
                    origin: "http://test.host",
                    method: HttpMethod.Post,
                    path: "example/path",
                    content: "TEST-REQUEST-BODY",
                    contentType: "test/request",
                    expectedStatusCode: HttpStatusCode.OK,
                    expectedContentType: null,
                    timeout: TimeSpan.FromMilliseconds(10));
            };

            var exception = await act.ShouldThrowExceptionAsync<Xunit.Sdk.XunitException>();
            exception.Message.Should().Contain("request didn't complete within allotted timeout");
        }
    }
}
