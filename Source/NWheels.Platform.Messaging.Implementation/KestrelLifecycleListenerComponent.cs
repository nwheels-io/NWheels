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
                //.UseUrls("http://localhost:5000/restapi")
                .UseUrls("http://localhost:5000")                

                // C:\Work\Repo\NWheelsFork\Source\Presentation\Web.Angular\angular2-webpack-starter\dist should be created by "npm run build:aot"
                //.UseUrls("http://localhost:5000/")        return *.html, *.js, *.css, *.txt, ....
                .Configure(app => {
                    app.UseMiddleware<RestApiMiddleware>(_restApiService);
                })
                .Build();

            _host.Start();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void MicroserviceMaybeDeactivating()
        {
            base.MicroserviceMaybeDeactivating();

            _host.Dispose();
        }
    }
}