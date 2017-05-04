#if false

using FluentAssertions;
using Microsoft.AspNetCore.Http;
using NWheels.Injection;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
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
            var httpContext = CreateTestRequest(HttpMethod.Get, new Uri("http://test.com/first"));

            //-- act

            restApiService.HandleHttpRequest(httpContext).Wait();
            var response = httpContext.Response;

            //-- assert

            response.Should().NotBeNull();
            response.StatusCode.Should().Be((int)HttpStatusCode.PartialContent);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void SuccessfulPost()
        {
            //-- arrange

            var restApiService = new RestApiService(new TestComponentContainer());
            var httpContext = CreateTestRequest(HttpMethod.Post, new Uri("http://test.com/second"));

            //-- act

            restApiService.HandleHttpRequest(httpContext).Wait();
            var response = httpContext.Response;

            //-- assert

            response.Should().NotBeNull();
            response.StatusCode.Should().Be((int)HttpStatusCode.Created);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void SuccessfulPut()
        {
            //-- arrange

            var restApiService = new RestApiService(new TestComponentContainer());
            var httpContext = CreateTestRequest(HttpMethod.Put, new Uri("http://test.com/third"));

            //-- act

            restApiService.HandleHttpRequest(httpContext).Wait();
            var response = httpContext.Response;

            //-- assert

            response.Should().NotBeNull();
            response.StatusCode.Should().Be((int)HttpStatusCode.OK);
        }
        
        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void SuccessfulPatch()
        {
            //-- arrange

            var restApiService = new RestApiService(new TestComponentContainer());
            var httpContext = CreateTestRequest(new HttpMethod("PATCH"), new Uri("http://test.com/first"));

            //-- act

            restApiService.HandleHttpRequest(httpContext).Wait();
            var response = httpContext.Response;

            //-- assert

            response.Should().NotBeNull();
            response.StatusCode.Should().Be((int)HttpStatusCode.OK);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void SuccessfulDelete()
        {
            //-- arrange

            var restApiService = new RestApiService(new TestComponentContainer());
            var httpContext = CreateTestRequest(HttpMethod.Delete, new Uri("http://test.com/second"));

            //-- act

            restApiService.HandleHttpRequest(httpContext).Wait();
            var response = httpContext.Response;

            //-- assert

            response.Should().NotBeNull();
            response.StatusCode.Should().Be((int)HttpStatusCode.OK);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void BadRequestOnPost()
        {
            //-- arrange

            var restApiService = new RestApiService(new TestComponentContainer());
            var httpContext = CreateTestRequest(HttpMethod.Post, new Uri("http://test.com/third"));

            //-- act

            restApiService.HandleHttpRequest(httpContext).Wait();
            var response = httpContext.Response;

            //-- assert

            response.Should().NotBeNull();
            response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void NotSupportedMethod()
        {
            //-- arrange

            var restApiService = new RestApiService(new TestComponentContainer());
            var httpContext = CreateTestRequest(new HttpMethod("TEST"), new Uri("http://test.com/third"));

            //-- act

            restApiService.HandleHttpRequest(httpContext).Wait();
            var response = httpContext.Response;

            //-- assert

            response.Should().NotBeNull();
            response.StatusCode.Should().Be((int)HttpStatusCode.NotImplemented);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void NotSupportedUri()
        {
            //-- arrange

            var restApiService = new RestApiService(new TestComponentContainer());
            var httpContext = CreateTestRequest(HttpMethod.Get, new Uri("http://test.com/test"));

            //-- act

            restApiService.HandleHttpRequest(httpContext).Wait();
            var response = httpContext.Response;

            //-- assert

            response.Should().NotBeNull();
            response.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private HttpContext CreateTestRequest(HttpMethod method, Uri uri)
        {
            var context = new DefaultHttpContext();
            context.Request.Method = method.Method;
            context.Request.Path = uri.AbsolutePath;
            return context;
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

            public TService ResolveNamed<TService>(string name)
            {
                throw new NotImplementedException();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public IEnumerable<TService> ResolveAll<TService>()
            {
                IEnumerable<TService> result = null;

                if (typeof(TService) == typeof(IResourceHandler))
                {
                    var handlers = new List<IResourceHandler>() {
                        new FirstTestRestResourceHandler(),
                        new SecondTestRestResourceHandler(),
                        new ThirdTestRestResourceHandler()
                    };
                    result = (IEnumerable<TService>)handlers;
                }

                return result;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public IEnumerable<Type> GetAllServiceTypes(Type baseType)
            {
                throw new NotImplementedException();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class FirstTestRestResourceHandler : ResourceHandlerBase
        {
            public FirstTestRestResourceHandler() 
                : base("/first")
            {
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override Task HttpGet(HttpContext context)
            {
                context.Response.StatusCode = (int)HttpStatusCode.PartialContent;
                return Task.CompletedTask;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override Task HttpPatch(HttpContext context)
            {
                context.Response.StatusCode = (int)HttpStatusCode.OK;
                return Task.CompletedTask;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class SecondTestRestResourceHandler : ResourceHandlerBase
        {
            public SecondTestRestResourceHandler() 
                : base("/second")
            {
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override Task HttpPost(HttpContext context)
            {
                context.Response.StatusCode = (int)HttpStatusCode.Created;
                return Task.CompletedTask;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override Task HttpDelete(HttpContext context)
            {
                context.Response.StatusCode = (int)HttpStatusCode.OK;
                return Task.CompletedTask;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class ThirdTestRestResourceHandler : ResourceHandlerBase
        {
            public ThirdTestRestResourceHandler() 
                : base("/third")
            {
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override Task HttpPut(HttpContext context)
            {
                context.Response.StatusCode = (int)HttpStatusCode.OK;
                return Task.CompletedTask;
            }
        }
    }
}

#endif