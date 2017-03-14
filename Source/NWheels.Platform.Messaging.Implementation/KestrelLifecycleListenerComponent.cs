using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using NWheels.Microservices;

namespace NWheels.Platform.Messaging
{
    public class KestrelLifecycleListenerComponent : LifecycleListenerComponentBase
    {
        public override void MicroserviceActivating()
        {
            base.MicroserviceActivating();
            
            var host = new WebHostBuilder()
                .UseKestrel()
                .UseStartup<Startup>()
                .Build();

            //System.IO.Directory.GetCurrentDirectory() == C:\Work\Repo\NWheelsFork\Source\NWheels.Samples.FirstHappyPath
            //host.Run();
        }

        public class Startup
        {
            public void ConfigureServices(IServiceCollection services)
            {
            }

            public void Configure(IApplicationBuilder app)
            {
            }
        }
    }
}