using NWheels.Microservices;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using NWheels.Injection;
using NWheels.Testability.Microservices;
using System.Net.Http;
using FluentAssertions;
using System.Linq;

namespace NWheels.SystemTests.Microservices
{
    [Trait("Purpose", "SystemTest")]
    public class MicroserviceHostSmokeTest
    {
        [Fact]
        public void SmokeTest()
        {
            var testListenerUrl = "http://localhost:5555";

            var hostController = new MicroserviceHostBuilder(microserviceName: "SmokeTest")
                .UseCliDirectoryFromEnvironment()
                .UseMicroserviceFromSource("..")
                .UseAutofacInjectionAdapter()
                .UseApplicationFeature<SmokeTestFeatureLoader>()
                .GetHostController();

            hostController.Start();

            var exceptions = new List<Exception>();

            try
            {
                using (var client = new HttpClient())
                {
                    var httpTask = client.GetAsync(testListenerUrl + "/this-is-a-test", HttpCompletionOption.ResponseContentRead);
                    var completed = httpTask.Wait(10000);
                    Assert.True(completed, "HTTP request didn't complete within allotted timeout.");

                    var response = httpTask.Result;
                    response.EnsureSuccessStatusCode();
                    var responseText = response.Content.ReadAsStringAsync().Result;
                    responseText.Should().Be("this-is-a-test");
                }
            }
            catch (Exception e)
            {
                exceptions.Add(e);
            }
            finally
            {
                try
                {
                    var stopped = hostController.Stop(10000, out int exitCode);
                    Assert.True(stopped, "Microservice host didn't stop within allotted timeout.");
                    exitCode.Should().Be(0, "microservice exit code");
                }
                catch (Exception e)
                {
                    exceptions.Add(e);
                }
            }

            if (exceptions.Count > 0)
            {
                throw new AggregateException(string.Join(" ; ", exceptions.Select(e => e.Message)), exceptions).Flatten();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class SmokeTestComponent : LifecycleListenerComponentBase
        {
            public override void Activate()
            {

            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override void MayDeactivate()
            {

            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [FeatureLoader(Name = "SmokeTest")]
        public class SmokeTestFeatureLoader : FeatureLoaderBase
        {
            public override void ContributeComponents(IComponentContainerBuilder containerBuilder)
            {
                containerBuilder.ContributeLifecycleListener<MicroserviceHostSmokeTest.SmokeTestComponent>();
            }
        }
    }
}
