using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Hapil.Testing.NUnit;
using NUnit.Framework;
using NWheels.Endpoints;
using NWheels.Extensions;
using NWheels.Testing;

namespace NWheels.Stacks.AspNet.Tests
{
    [TestFixture]
    public class WebApiControllerFactoryTests : DynamicTypeUnitTestBase
    {
        [Test]
        public void CanGenerateController()
        {
            //-- arrange

            var factory = new WebApiControllerFactory(base.DyamicModule);
            Type controllerType = null;

            Framework.UpdateComponents(containerBuilder => {
                containerBuilder.RegisterType<TestHandler>();
                controllerType = factory.CreateControllerType(typeof(TestHandler));
                containerBuilder.RegisterType(controllerType);
                containerBuilder.NWheelsFeatures().Logging().RegisterLogger<IWebApplicationLogger>();
            });

            //-- act
            
            dynamic controllerAsDynamic = Framework.Components.Resolve(controllerType);

            var response1 = (HttpResponseMessage)controllerAsDynamic.FirstOperation();
            var response2 = (HttpResponseMessage)controllerAsDynamic.SecondOperation();

            //-- assert

            Assert.That(response1, Is.Not.Null);
            Assert.That(response1.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            //Assert.That(GetResponseContentString(response1), Is.StringContaining("FIRST-REPLY"));
            
            Assert.That(response2, Is.Not.Null);
            Assert.That(response2.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            //Assert.That(GetResponseContentString(response2), Is.StringContaining("SECOND-REPLY"));
        }

        //private string GetResponseContentString(HttpResponseMessage response)
        //{
        //    //var readTask = response.Content.ReadAsAsync<string>();
        //    //readTask.Wait();
        //    //return readTask.Result;
        //    //Stream receiveStream = response.Content.
        //    //StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8);
        //    //txtBlock.Text = readStream.ReadToEnd();            
        //}

        public class TestHandler
        {
            [HttpOperation(Route = "first")]
            public HttpResponseMessage FirstOperation(HttpRequestMessage request)
            {
                return new HttpResponseMessage(HttpStatusCode.OK) {
                    Content = new StringContent("FIRST-REPLY")
                };
            }

            [HttpOperation(Route = "second")]
            public HttpResponseMessage SecondOperation(HttpRequestMessage request)
            {
                return new HttpResponseMessage(HttpStatusCode.OK) {
                    Content = new StringContent("SECOND-REPLY")
                };
            }
        }
    }
}
