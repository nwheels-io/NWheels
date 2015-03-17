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

        public static void RegisterPascalCaseRelationalMappingConvention(this ContainerBuilder builder, bool usePluralTableNames = true)
        {
            builder.RegisterInstance(new RelationalMappingConventionDefault(RelationalMappingConventionDefault.ConventionType.PascalCase, usePluralTableNames));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static void RegisterUnderscoreRelationalMappingConvention(this ContainerBuilder builder, bool usePluralTableNames = true)
        {
            builder.RegisterInstance(new RelationalMappingConventionDefault(RelationalMappingConventionDefault.ConventionType.Underscore, usePluralTableNames));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static void RegisterCustomRelationalMappingConvention<TConvention>(this ContainerBuilder builder, bool singleInstance = true)
            where TConvention : IRelationalMappingConvention
        {
            var registration = builder.RegisterType<TConvention>().As<IRelationalMappingConvention>();

            if ( singleInstance )
            {
                registration.SingleInstance();
            }
            else
            {
                registration.InstancePerDependency();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static void RegisterRelationalMappingFineTune<TEntity>(this ContainerBuilder builder, Action<IRelationalMappingFineTune<TEntity>> fineTuneAction)
            where TEntity : class
        {
            var fineTuner = new RelationalMappingFineTuner<TEntity>(fineTuneAction);
            builder.RegisterInstance<RelationalMappingFineTuner<TEntity>>(fineTuner);
        }
    }
}
