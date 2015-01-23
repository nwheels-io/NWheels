using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using NWheels.UI;
using NWheels.UI.Endpoints;

namespace NWheels.Extensions
{
    public static class AutofacExtensions
    {
        public static TService ResolveAuto<TService>(this IComponentContext container)
            where TService : class
        {
            return container.Resolve<Auto<TService>>().Instance;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static void RegisterWebAppEndpoint<TApp>(this ContainerBuilder builder)
            where TApp : IUiApplication
        {
            builder.RegisterType<WebAppEndpoint<TApp>>().As<IWebAppEndpoint>();
        }
    }
}
