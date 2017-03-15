using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using NWheels.Microservices;
using NWheels.Platform.Rest;

namespace NWheels.Platform.Messaging
{
    public class KestrelLifecycleListenerComponent : LifecycleListenerComponentBase
    {
        private readonly IRestApiService _restApiService;
        private IWebHost _host;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public KestrelLifecycleListenerComponent(IRestApiService restApiService)
        {
            _restApiService = restApiService;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void MicroserviceActivating()
        {
            base.MicroserviceActivating();
            
            _host = new WebHostBuilder()
                .UseKestrel()
                .UseUrls("http://localhost:5000")
                .Configure(app => {
                    app.UseMiddleware<RestApiMiddleware>(_restApiService);
                })
                .Build();
            
            _host.Run();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void MicroserviceMaybeDeactivating()
        {
            base.MicroserviceMaybeDeactivating();

            _host.Dispose();
        }
    }
}