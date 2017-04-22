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
using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace NWheels.SystemTests.Microservices
{
    [Trait("Purpose", "SystemTest")]
    public class MicroserviceHostSmokeTest
    {
        public const string TestListenerUrl = "http://localhost:5555";

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void SmokeTest()
        {
            var host = new MicroserviceHostBuilder(microserviceName: "SmokeTest")
                .UseCliDirectoryFromSource(relativeProjectDirectoryPath: "..", allowOverrideByEnvironmentVar: true)
                .UseMicroserviceFromSource(relativeProjectDirectoryPath: "..")
                .UseAutofacInjectionAdapter()
                .UseApplicationFeature<SmokeTestFeatureLoader>()
                .Build();

            host.Start();

            using (var client = new HttpClient())
            {
                var httpTask = client.GetAsync(TestListenerUrl + "/this-is-a-test", HttpCompletionOption.ResponseContentRead);
                var completed = httpTask.Wait(10000);
                Assert.True(completed, "HTTP request didn't complete within allotted timeout.");

                var response = httpTask.Result;
                response.EnsureSuccessStatusCode();
                var responseText = response.Content.ReadAsStringAsync().Result;
                responseText.Should().Be("this-is-a-test");
            }

            host.StopOrThrow(10000);
            host.AssertNoErrors();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class SmokeTestComponent : LifecycleListenerComponentBase
        {
            private IWebHost _host;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override void Activate()
            {
                _host = new WebHostBuilder()
                    .UseKestrel()
                    .UseUrls(TestListenerUrl)
                    .Configure(app => {
                        app.Run(async context => {
                            await context.Response.WriteAsync(context.Request.Path.Value.TrimStart('/'));
                        });
                    })
                    .Build();

                _host.Start();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override void MayDeactivate()
            {
                _host.Dispose();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [FeatureLoader(Name = "SmokeTest")]
        public class SmokeTestFeatureLoader : FeatureLoaderBase
        {
            public override void ContributeComponents(IComponentContainer existingComponents, IComponentContainerBuilder newComponents)
            {
                newComponents.ContributeLifecycleListener<MicroserviceHostSmokeTest.SmokeTestComponent>();
            }
        }
    }
}
