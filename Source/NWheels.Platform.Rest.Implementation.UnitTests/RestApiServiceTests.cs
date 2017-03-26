using FluentAssertions;
using NWheels.Injection;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using Xunit;

namespace NWheels.Platform.Rest.Implementation.UnitTests
{
    public class RestApiServiceTests
    {
        [Fact]
        public void SuccessfulGet()
        {
            //-- arrange

            var restApiService = new RestApiService(new TestComponentContainer());            
            var request = new HttpRequestMessage(HttpMethod.Get, new Uri("http://test.com/first"));

            //-- act

            var response = restApiService.HandleApiRequest(request);

            //-- assert

            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.PartialContent);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void SuccessfulPost()
        {
            //-- arrange

            var restApiService = new RestApiService(new TestComponentContainer());
            var request = new HttpRequestMessage(HttpMethod.Post, new Uri("http://test.com/second"));

            //-- act

            var response = restApiService.HandleApiRequest(request);

            //-- assert

            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void SuccessfulPut()
        {
            //-- arrange

            var restApiService = new RestApiService(new TestComponentContainer());
            var request = new HttpRequestMessage(HttpMethod.Put, new Uri("http://test.com/third"));

            //-- act

            var response = restApiService.HandleApiRequest(request);

            //-- assert

            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
        
        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void SuccessfulPatch()
        {
            //-- arrange

            var restApiService = new RestApiService(new TestComponentContainer());
            var request = new HttpRequestMessage(new HttpMethod("PATCH"), new Uri("http://test.com/first"));

            //-- act

            var response = restApiService.HandleApiRequest(request);

            //-- assert

            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void SuccessfulDelete()
        {
            //-- arrange

            var restApiService = new RestApiService(new TestComponentContainer());
            var request = new HttpRequestMessage(HttpMethod.Delete, new Uri("http://test.com/second"));

            //-- act

            var response = restApiService.HandleApiRequest(request);

            //-- assert

            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void BadRequestOnPost()
        {
            //-- arrange

            var restApiService = new RestApiService(new TestComponentContainer());
            var request = new HttpRequestMessage(HttpMethod.Post, new Uri("http://test.com/third"));

            //-- act

            var response = restApiService.HandleApiRequest(request);

            //-- assert

            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void NotSupportedMethod()
        {
            //-- arrange

            var restApiService = new RestApiService(new TestComponentContainer());
            var request = new HttpRequestMessage(new HttpMethod("TEST"), new Uri("http://test.com/third"));

            //-- act

            var response = restApiService.HandleApiRequest(request);

            //-- assert

            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.NotImplemented);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void NotSupportedUri()
        {
            //-- arrange

            var restApiService = new RestApiService(new TestComponentContainer());
            var request = new HttpRequestMessage(HttpMethod.Get, new Uri("http://test.com/test"));

            //-- act

            var response = restApiService.HandleApiRequest(request);

            //-- assert

            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class TestComponentContainer : IComponentContainer
        {
            public void Dispose()
            {
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public TService Resolve<TService>()
            {
                throw new NotImplementedException();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public IEnumerable<TService> ResolveAll<TService>()
            {
                IEnumerable<TService> result = null;

                if (typeof(TService) == typeof(IRestResourceHandler))
                {
                    var handlers = new List<IRestResourceHandler>() {
                        new FirstTestRestResourceHandler(),
                        new SecondTestRestResourceHandler(),
                        new ThirdTestRestResourceHandler()
                    };
                    result = (IEnumerable<TService>)handlers;
                }

                return result;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private abstract class TestRestResourceHandlerBase : IRestResourceHandler
        {
            public abstract string UriPath { get; }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public virtual HttpResponseMessage Delete(HttpRequestMessage request)
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public virtual HttpResponseMessage Get(HttpRequestMessage request)
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public virtual HttpResponseMessage Patch(HttpRequestMessage request)
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public virtual HttpResponseMessage Post(HttpRequestMessage request)
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public virtual HttpResponseMessage Put(HttpRequestMessage request)
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class FirstTestRestResourceHandler : TestRestResourceHandlerBase
        {
            public override string UriPath => "/first";

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override HttpResponseMessage Get(HttpRequestMessage request)
            {
                return new HttpResponseMessage()
                {
                    RequestMessage = request,
                    StatusCode = HttpStatusCode.PartialContent
                };
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override HttpResponseMessage Patch(HttpRequestMessage request)
            {
                return new HttpResponseMessage()
                {
                    RequestMessage = request,
                    StatusCode = HttpStatusCode.OK
                };
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class SecondTestRestResourceHandler : TestRestResourceHandlerBase
        {
            public override string UriPath => "/second";

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override HttpResponseMessage Post(HttpRequestMessage request)
            {
                return new HttpResponseMessage()
                {
                    RequestMessage = request,
                    StatusCode = HttpStatusCode.Created
                };
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override HttpResponseMessage Delete(HttpRequestMessage request)
            {
                return new HttpResponseMessage()
                {
                    RequestMessage = request,
                    StatusCode = HttpStatusCode.OK
                };
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class ThirdTestRestResourceHandler : TestRestResourceHandlerBase
        {
            public override string UriPath => "/third";

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override HttpResponseMessage Put(HttpRequestMessage request)
            {
                return new HttpResponseMessage()
                {
                    RequestMessage = request,
                    StatusCode = HttpStatusCode.OK
                };
            }
        }
    }
}
