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
                //.UseContentRoot(@"C:\Work\Repo\NWheelsFork\Source")
                //.UseUrls("http://*:5000;http://localhost:5001;https://hostname:5002")
                .UseUrls("http://*:5000")
                .Configure(app => {
                    /*var trackPackageRouteHandler = new RouteHandler(context =>
                    {
                        var request = new HttpRequestMessage(new HttpMethod(context.Request.Method), new Uri(context.Request.QueryString.Value));
                        var response = _restApiService.HandleApiRequest(request);
                        context.Response.
                        //return context.Response();            
                    });

                    var routeBuilder = new RouteBuilder(appBuilder, trackPackageRouteHandler);
                    var routes = routeBuilder.Build();
                    appBuilder.UseRouter(routes);*/
                    app.UseMiddleware<RestApiMiddleware>();
                })
                .Build();
            
            //System.IO.Directory.GetCurrentDirectory() == C:\Work\Repo\NWheelsFork\Source\NWheels.Samples.FirstHappyPath
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