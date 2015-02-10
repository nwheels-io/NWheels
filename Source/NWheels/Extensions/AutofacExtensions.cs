using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using NWheels.Configuration;
using NWheels.Entities;
using NWheels.UI;
using NWheels.UI.Endpoints;
using NWheels.Processing;

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

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static void RegisterApplicationJob<TJob>(this ContainerBuilder builder) 
            where TJob : IApplicationJob
        {
            builder.RegisterType<TJob>().As<TJob, IApplicationJob>().InstancePerDependency();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static void RegisterConfigSection<TSection>(this ContainerBuilder builder)
            where TSection : class, IConfigurationSection
        {
            builder.RegisterType<ConfigSectionRegistration<TSection>>().As<IConfigSectionRegistration>().InstancePerDependency();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static void RegisterEntityRelationalMapping<TEntity>(this ContainerBuilder builder, Action<IEntityRelationalMapping<TEntity>> configurator)
            where TEntity : class
        {
            var configuration = new EntityRelationalMappingConfiguration<TEntity>(configurator);
            builder.RegisterInstance<EntityRelationalMappingConfiguration<TEntity>>(configuration);
        }
    }
}
