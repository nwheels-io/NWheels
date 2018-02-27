using System;
using ElectricityBilling.Domain;
using NWheels.Logging;
using NWheels.Microservices;
using NWheels.RestApi;
using NWheels.UI.Web;

namespace ElectricityBilling.AdminWebApp
{
    class Program
    {
        static int Main(string[] args)
        {
            return Microservice.RunDaemonCli("AdminWebApp", args, host => {
                host.UseLogging<ElasticStack>();
                host.UseDdd();
                host.UseRestApi<AspNetCoreSwaggerStack>();
                host.UseUidl<WebReactReduxStack>();
                host.UseApplicationFeature<AutoDiscoverAssemblyOf<ElectricityBillingContext>>();
                host.ExposeWebApp<AdminUIApp>(baseUrlPath: "/");
            });
        }
    }
}
