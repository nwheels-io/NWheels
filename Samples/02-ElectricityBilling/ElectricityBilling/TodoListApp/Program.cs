using System;
using NWheels.Microservices;
using NWheels.DB.Adapters.EFCore;
using NWheels.Logging.Adapters.Elastic;
using NWheels.RestApi.Adapters.AspNetCoreSwagger;
using NWheels.UI.Adapters.WebReactRedux;
using NWheels.UI.Web;

namespace TodoListApp
{
    class Program
    {
        static int Main(string[] args)
        {
            return Microservice.RunDaemonCli("AdminWebApp", args, host => {
                host.UseLogging<ElasticStack>();
                host.UseDB<EFCoreStack>();
                host.UseDdd();
                host.UseRestApi<AspNetCoreSwaggerStack>();
                host.UseUidl<WebReactReduxStack>();
                host.UseApplicationFeature<AutoDiscoverAssemblyOf<TodoContext>>();
                host.ExposeWebApp<TodoWebApp>(baseUrlPath: "/");
            });
        }
    }
}
