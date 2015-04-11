using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Autofac.Builder;
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

        ////-----------------------------------------------------------------------------------------------------------------------------------------------------

        //public static IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> AspectLog<TLimit, TActivatorData, TRegistrationStyle>(
        //    this IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> registration)
        //{
        //    registration.RegistrationData.ActivatingHandlers.Add((sender, e) => {
        //        var factory = e.Context.Resolve<CallLoggingAspectFactory>();
        //    });
        //}

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static AutofacExtensions.NWheelsFeatureRegistrations NWheelsFeatures(this ContainerBuilder builder)
        {
            return new NWheelsFeatureRegistrations(builder);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static ContractFeature Contracts(this NWheelsFeatureRegistrations features)
        {
            return new ContractFeature(((IHaveContainerBuilder)features).Builder);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static EntityFeature Entities(this NWheelsFeatureRegistrations features)
        {
            return new EntityFeature(((IHaveContainerBuilder)features).Builder);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static UIFeature UI(this NWheelsFeatureRegistrations features)
        {
            return new UIFeature(((IHaveContainerBuilder)features).Builder);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static void WithWebEndpoint<TApp>(this UIAppEndpointRegistrations<TApp> registration) 
            where TApp : class, IUiApplication
        {
            ((IHaveContainerBuilder)registration).Builder.RegisterType<WebAppEndpoint<TApp>>().As<IWebAppEndpoint>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface IHaveContainerBuilder
        {
            ContainerBuilder Builder { get; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class NWheelsFeatureRegistrations : IHaveContainerBuilder
        {
            private readonly ContainerBuilder _builder;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public NWheelsFeatureRegistrations(ContainerBuilder builder)
            {
                _builder = builder;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            ContainerBuilder IHaveContainerBuilder.Builder
            {
                get { return _builder; }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class ContractFeature
        {
            private readonly ContainerBuilder _builder;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ContractFeature(ContainerBuilder builder)
            {
                _builder = builder;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ContractConcretizationRegistration<TGeneral> Concretize<TGeneral>() where TGeneral : class
            {
                return new ContractConcretizationRegistration<TGeneral>(_builder);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ContractMixinRegistration<TPart> Mix<TPart>() where TPart : class
            {
                return new ContractMixinRegistration<TPart>(_builder);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class EntityFeature
        {
            private readonly ContainerBuilder _builder;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public EntityFeature(ContainerBuilder builder)
            {
                _builder = builder;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public DataRepositoryRegistration<TRepo> RegisterDataRepository<TRepo>() where TRepo : class, IApplicationDataRepository
            {
                var registration = new DataRepositoryRegistration<TRepo>();
                _builder.RegisterInstance(registration).As<IDataRepositoryRegistration>();
                return registration;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void UsePascalCaseRelationalMappingConvention(bool usePluralTableNames = true)
            {
                _builder.RegisterInstance(new RelationalMappingConventionDefault(
                    RelationalMappingConventionDefault.ConventionType.PascalCase, 
                    usePluralTableNames));
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void UseUnderscoreRelationalMappingConvention(bool usePluralTableNames = true)
            {
                _builder.RegisterInstance(new RelationalMappingConventionDefault(
                    RelationalMappingConventionDefault.ConventionType.Underscore, 
                    usePluralTableNames));
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void RegisterCustomRelationalMappingConvention<TConvention>(bool singleInstance = true)
                where TConvention : IRelationalMappingConvention
            {
                var registration = _builder.RegisterType<TConvention>().As<IRelationalMappingConvention>();

                if ( singleInstance )
                {
                    registration.SingleInstance();
                }
                else
                {
                    registration.InstancePerDependency();
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void RegisterRelationalMappingFineTune<TEntity>(Action<IRelationalMappingFineTune<TEntity>> fineTuneAction)
                where TEntity : class
            {
                var fineTuner = new RelationalMappingFineTuner<TEntity>(fineTuneAction);
                _builder.RegisterInstance<RelationalMappingFineTuner<TEntity>>(fineTuner);
            }

        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------

        public class UIFeature
        {
            private readonly ContainerBuilder _builder;

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public UIFeature(ContainerBuilder builder)
            {
                _builder = builder;
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public UIAppEndpointRegistrations<TApp> RegisterApplication<TApp>() where TApp : class, IUiApplication
            {
                _builder.RegisterType<TApp>().As<TApp, IUiApplication>();
                return new UIAppEndpointRegistrations<TApp>(_builder);
            }
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------

        public class ContractConcretizationRegistration<TGeneral> where TGeneral : class
        {
            private readonly ContainerBuilder _builder;

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public ContractConcretizationRegistration(ContainerBuilder builder)
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

        public class ContractMixinRegistration<TPart> where TPart : class
        {
            private readonly ContainerBuilder _builder;

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public ContractMixinRegistration(ContainerBuilder builder)
            {
                _builder = builder;
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public void Into<TContract>() where TContract : class
            {
                var mixin = new MixinRegistration(typeof(TContract), typeof(TPart));
                _builder.RegisterInstance(mixin).As<MixinRegistration>();
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
