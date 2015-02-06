using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using NWheels.Entities;
using NWheels.Samples.RestService.EntityFrameworkAutoImpl;
using NWheels.Samples.RestService.OwinOdataWebApiAutoImpl;

namespace NWheels.Samples.RestService
{
    public class ModuleLoader : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<OnlineStoreModelContainer>().InstancePerDependency();
            builder.RegisterType<MyRestServiceEntityRepositoryImpl>().As<IMyRestServiceEntityRepository>().InstancePerDependency();
            builder.RegisterType<ProductController>().InstancePerRequest();
            builder.RegisterType<OrderController>().InstancePerRequest();
            builder.RegisterType<OrderLineController>().InstancePerRequest();
            builder.RegisterType<DataRepositoryEndpoint<IMyRestServiceEntityRepository>>().As<IDataRepositoryEndpoint>();
        }
    }
}
