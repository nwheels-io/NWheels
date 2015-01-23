using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Extensions;
using NWheels.Hosting;
using NWheels.Samples.BloggingPlatform.Apps;
using NWheels.UI.Endpoints;

namespace NWheels.Samples.BloggingPlatform
{
    public class ModuleLoader : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<BlogApp>();
            builder.RegisterWebAppEndpoint<BlogApp>();
        }
    }
}
