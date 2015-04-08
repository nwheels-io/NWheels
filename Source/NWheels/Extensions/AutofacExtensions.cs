using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using NWheels.Configuration;
using NWheels.DataObjects;
using NWheels.Entities;
using NWheels.UI;
using NWheels.UI.Endpoints;
using NWheels.Processing;

namespace NWheels.Extensions
{
    public static class AutofacExtensions
    {
        public static AutofacExtensions.NWheelsFeatureRegistrations NWheelsFeatures(this ContainerBuilder builder)
        {
            return new NWheelsFeatureRegistrations(builder);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static EntityFeatureRegistrations Entities(this NWheelsFeatureRegistrations features)
        {
            return new EntityFeatureRegistrations(((IHaveContainerBuilder)features).Builder);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static UIFeatureRegistrations UI(this NWheelsFeatureRegistrations features)
        {
            return new UIFeatureRegistrations(((IHaveContainerBuilder)features).Builder);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static void WebEndpoint<TApp>(this UIAppEndpointRegistrations<TApp> registration) 
            where TApp : class, IUiApplication
        {
            ((IHaveContainerBuilder)registration).Builder.RegisterType<WebAppEndpoint<TApp>>().As<IWebAppEndpoint>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static TService ResolveAuto<TService>(this IComponentContext container)
            where TService : class
        {
            return container.Resolve<Auto<TService>>().Instance;
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

        //---------------------------------------------------------------------------------------------------------------------------------------------------------

        public interface IHaveContainerBuilder
        {
            ContainerBuilder Builder { get; }
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------

        public class NWheelsFeatureRegistrations : IHaveContainerBuilder
        {
            private readonly ContainerBuilder _builder;

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public NWheelsFeatureRegistrations(ContainerBuilder builder)
            {
                _builder = builder;
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            ContainerBuilder IHaveContainerBuilder.Builder
            {
                get { return _builder; }
            }
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------

        public class EntityFeatureRegistrations
        {
            private readonly ContainerBuilder _builder;

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public EntityFeatureRegistrations(ContainerBuilder builder)
            {
                _builder = builder;
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public EntityConcretizationRegistration<TGeneral> Concretize<TGeneral>() where TGeneral : class
            {
                return new EntityConcretizationRegistration<TGeneral>(_builder);
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public EntityMixinRegistration<TPart> Mix<TPart>() where TPart : class
            {
                return new EntityMixinRegistration<TPart>(_builder);
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public void DataRepository<TRepo>(bool initializeStorageOnStartup = false) where TRepo : class, IApplicationDataRepository
            {
                _builder.RegisterInstance(new ApplicationDataRepositoryRegistration<TRepo>(initializeStorageOnStartup)).As<ApplicationDataRepositoryRegistration>();
            }
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------

        public class EntityConcretizationRegistration<TGeneral> where TGeneral : class
        {
            private readonly ContainerBuilder _builder;

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public EntityConcretizationRegistration(ContainerBuilder builder)
            {
                _builder = builder;
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public void With<TConcrete>() where TConcrete : class, TGeneral
            {
                var concretization = new ConcretizationRegistration(typeof(TGeneral), typeof(TConcrete));
                _builder.RegisterInstance(concretization).As<ConcretizationRegistration>();
            }
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------

        public class EntityMixinRegistration<TPart> where TPart : class
        {
            private readonly ContainerBuilder _builder;

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public EntityMixinRegistration(ContainerBuilder builder)
            {
                _builder = builder;
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public void Into<TEntity>() where TEntity : class
            {
                var mixin = new MixinRegistration(typeof(TEntity), typeof(TPart));
                _builder.RegisterInstance(mixin).As<MixinRegistration>();
            }
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------
        
        public class UIFeatureRegistrations
        {
            private readonly ContainerBuilder _builder;

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public UIFeatureRegistrations(ContainerBuilder builder)
            {
                _builder = builder;
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public UIAppEndpointRegistrations<TApp> Application<TApp>() where TApp : class, IUiApplication
            {
                _builder.RegisterType<TApp>().As<TApp, IUiApplication>();
                return new UIAppEndpointRegistrations<TApp>(_builder);
            }
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------

        public class UIAppEndpointRegistrations<TApp> : IHaveContainerBuilder
            where TApp : class, IUiApplication
        {
            private readonly ContainerBuilder _builder;

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public UIAppEndpointRegistrations(ContainerBuilder builder)
            {
                _builder = builder;
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            ContainerBuilder IHaveContainerBuilder.Builder
            {
                get { return _builder; }
            }
        }
    }
}
