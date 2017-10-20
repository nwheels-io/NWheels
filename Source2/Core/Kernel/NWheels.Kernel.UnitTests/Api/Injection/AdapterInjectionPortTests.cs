using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using NWheels.Testability;
using Xunit;
using FluentAssertions;
using NWheels.Kernel.Api.Injection;
using NWheels.Kernel.Api.Exceptions;

namespace NWheels.Kernel.UnitTests.Api.Injection
{
    using NWheels.Kernel.Runtime.Injection;
    using PortsAndAdapters;

    public class AdapterInjectionPortTests : TestBase.UnitTest
    {
        [Fact]
        public void OneProgrammingModel_OnePort_OneAdapter()
        {
            //-- arrange

            var consumingModelFeature = new PortsAndAdapters.ConsumingModel.Runtime.ConsumingModelFeature();
            var supplierAdapterFeature = new PortsAndAdapters.SupplyingModel.Adapters.Abcd.AbcdFeature();
            var appFeature = new PortsAndAdapters.App.Module.FirstAppFeature();
            var unusedPortFeature = new PortsAndAdapters.App.Module.UnusedPortFeature();

            IComponentContainer container = LoadFeatures(consumingModelFeature, supplierAdapterFeature, appFeature, unusedPortFeature);

            //-- act

            var model = container.Resolve<PortsAndAdapters.App.Module.IFirstAppSpecificInterface>();

            //-- assert

            model.Should().NotBeNull();

            var generatedModel = model.Should()
                .BeOfType<PortsAndAdapters.ConsumingModel.GeneratedCode.ConsumingModel_of_App_Module_FirstAppSpecificInterface>().Which;

            var adapter = generatedModel.Supplier.Should()
                .BeOfType<PortsAndAdapters.SupplyingModel.Adapters.Abcd.AbcdAdapter>().Which;

            adapter.Configuration.Should().BeSameAs(appFeature.Configuration);
            adapter.Configuration.Value.Should().Be("111");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void GenericInstantiationsOfProgrammingModel_PortPerInstance_OneAdapterType()
        {
            //-- arrange

            var consumingModelFeature = new PortsAndAdapters.ConsumingModel.Runtime.ConsumingModelFeature();
            var supplierAdapterFeature = new PortsAndAdapters.SupplyingModel.Adapters.Abcd.AbcdFeature();
            var appFeature1 = new PortsAndAdapters.App.Module.FirstAppFeature();
            var appFeature2 = new PortsAndAdapters.App.Module.SecondAppFeature();

            IComponentContainer container = LoadFeatures(consumingModelFeature, supplierAdapterFeature, appFeature1, appFeature2);

            //-- act

            var model1 = container.Resolve<PortsAndAdapters.App.Module.IFirstAppSpecificInterface>();
            var model2 = container.Resolve<PortsAndAdapters.App.Module.ISecondAppSpecificInterface>();

            //-- assert

            model1.Should().NotBeNull();

            var generatedModel1 = model1.Should()
                .BeOfType<PortsAndAdapters.ConsumingModel.GeneratedCode.ConsumingModel_of_App_Module_FirstAppSpecificInterface>().Which;
            var adapter1 = generatedModel1.Supplier.Should()
                .BeOfType<PortsAndAdapters.SupplyingModel.Adapters.Abcd.AbcdAdapter>().Which;
            adapter1.Configuration.Should().BeSameAs(appFeature1.Configuration);
            adapter1.Configuration.Value.Should().Be("111");

            model2.Should().NotBeNull();

            var generatedModel2 = model2.Should()
                .BeOfType<PortsAndAdapters.ConsumingModel.GeneratedCode.ConsumingModel_of_App_Module_SecondAppSpecificInterface>().Which;
            var adapter2 = generatedModel2.Supplier.Should()
                .BeOfType<PortsAndAdapters.SupplyingModel.Adapters.Abcd.AbcdAdapter>().Which;
            adapter2.Configuration.Should().BeSameAs(appFeature2.Configuration);
            adapter2.Configuration.Value.Should().Be("222");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void MultipleInstancesOfProgrammingModel_PortPerInstance_DifferentAdapterTypes()
        {
            //-- arrange

            var consumingModelFeature = new PortsAndAdapters.ConsumingModel.Runtime.ConsumingModelFeature();
            var adapterFeature1 = new PortsAndAdapters.SupplyingModel.Adapters.Abcd.AbcdFeature();
            var adapterFeature2 = new PortsAndAdapters.SupplyingModel.Adapters.Efgh.EfghFeature();
            var appFeature1 = new PortsAndAdapters.App.Module.FirstAppFeature();
            var appFeature2 = new PortsAndAdapters.App.Module.SecondAppFeature() { SuppressAbcd = true };

            IComponentContainer container = LoadFeatures(consumingModelFeature, adapterFeature1, adapterFeature2, appFeature1, appFeature2);

            //-- act

            var model1 = container.Resolve<PortsAndAdapters.App.Module.IFirstAppSpecificInterface>();
            var model2 = container.Resolve<PortsAndAdapters.App.Module.ISecondAppSpecificInterface>();

            //-- assert

            model1.Should().NotBeNull();

            var generatedModel1 = model1.Should()
                .BeOfType<PortsAndAdapters.ConsumingModel.GeneratedCode.ConsumingModel_of_App_Module_FirstAppSpecificInterface>().Which;
            var adapter1 = generatedModel1.Supplier.Should()
                .BeOfType<PortsAndAdapters.SupplyingModel.Adapters.Abcd.AbcdAdapter>().Which;
            adapter1.Configuration.Should().BeSameAs(appFeature1.Configuration);
            adapter1.Configuration.Value.Should().Be("111");

            model2.Should().NotBeNull();

            var generatedModel2 = model2.Should()
                .BeOfType<PortsAndAdapters.ConsumingModel.GeneratedCode.ConsumingModel_of_App_Module_SecondAppSpecificInterface>().Which;
            var adapter2 = generatedModel2.Supplier.Should()
                .BeOfType<PortsAndAdapters.SupplyingModel.Adapters.Efgh.EfghAdapter>().Which;
            adapter2.Configuration.Should().BeSameAs(appFeature2.Configuration);
            adapter2.Configuration.Value.Should().Be("222");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        // TODO: test programming model with multiple ports

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        private static IInternalComponentContainer LoadFeatures(params IFeatureLoader[] featureList)
        {
            var container = new ComponentContainerBuilder().CreateComponentContainer();

            var builder1 = new ComponentContainerBuilder(container);
            forEachFeature(f => f.ContributeComponents(container, builder1));
            container.Merge(builder1);

            var builder2 = new ComponentContainerBuilder(container);
            forEachFeature(f => f.ContributeAdapterComponents(container, builder2));
            container.Merge(builder2);

            var builder3 = new ComponentContainerBuilder(container);
            forEachFeature(f => f.CompileComponents(container));
            forEachFeature(f => f.ContributeCompiledComponents(container, builder3));
            container.Merge(builder3);
            return container;

            void forEachFeature(Action<IFeatureLoader> action)
            {
                foreach (var feature in featureList)
                {
                    action(feature);
                }
            }
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------
    
    namespace PortsAndAdapters
    {
        namespace App.Module
        {
            using ConsumingModel.Api;
            using SupplyingModel.Api;

            public interface IFirstAppSpecificInterface
            {
            }

            public interface ISecondAppSpecificInterface
            {
            }

            public class FirstAppFeature : AdvancedFeature
            {
                public override void ContributeComponents(IComponentContainer existingComponents, IComponentContainerBuilder newComponents)
                {
                    newComponents.ContributeConsumingModelFor<IFirstAppSpecificInterface>(configureSupplier: (config) => {
                        config.Value = "111";
                        this.Configuration = config;
                    });
                }
                public SupplierConfiguration Configuration { get; set; }
            }

            public class SecondAppFeature : AdvancedFeature
            {
                public override void ContributeComponents(IComponentContainer existingComponents, IComponentContainerBuilder newComponents)
                {
                    newComponents.ContributeConsumingModelFor<ISecondAppSpecificInterface>(configureSupplier: (config) => {
                        config.Value = "222";
                        config.SuppressAbcd = this.SuppressAbcd;
                        this.Configuration = config;
                    });
                }
                public SupplierConfiguration Configuration { get; set; }
                public bool SuppressAbcd { get; set; }
            }

            public class UnusedPortFeature : AdvancedFeature
            {
                public override void ContributeComponents(IComponentContainer existingComponents, IComponentContainerBuilder newComponents)
                {
                    var unusedPort = new SupplierAdapterInjectionPort(newComponents, new SupplierConfiguration() {
                        Value = "ZZZ"
                    });

                    newComponents.RegisterAdapterPort(unusedPort);
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        namespace ConsumingModel
        {
            namespace Api
            {
                using SupplyingModel.Api;
                using ConsumingModel.Runtime;

                public static class ComponentContainerBuilderExtensions
                {
                    public static IComponentContainerBuilder ContributeConsumingModelFor<TAppSpecificInterface>(
                        this IComponentContainerBuilder builder, 
                        Action<SupplierConfiguration> configureSupplier = null)
                    {
                        var configuration = new SupplierConfiguration();
                        configureSupplier?.Invoke(configuration);
                        
                        var supplierPort = new SupplierAdapterInjectionPort(builder, configuration);
                        builder.RegisterAdapterPort(supplierPort);

                        var registration = new ConsumingModelRegistration(typeof(TAppSpecificInterface), supplierPort);
                        builder.RegisterComponentInstance(registration);

                        return builder;
                    }                     
                }

                public class ConsumingModelRegistration
                {
                    public ConsumingModelRegistration(Type appSpecificModelType, SupplierAdapterInjectionPort supplierPort)
                    {
                        this.AppSpecificModelType = appSpecificModelType;
                        this.SupplierPort = supplierPort;
                    }
                    public Type AppSpecificModelType { get; }
                    public SupplierAdapterInjectionPort SupplierPort { get; }
                }
            }

            namespace Runtime
            {
                using System.Linq;
                using ConsumingModel.Api;
                using SupplyingModel.Api;

                public class ConsumingModelFeature : AdvancedFeature
                {
                    private readonly Dictionary<Type, Type> _generatedTypeByAppSpecificType = new Dictionary<Type, Type>();

                    public override void CompileComponents(IComponentContainer existingComponents)
                    {
                        var allRegistrations = existingComponents.ResolveAll<ConsumingModelRegistration>();

                        foreach (var registration in allRegistrations)
                        {
                            GenerateAppSpecificModel(registration.AppSpecificModelType);
                        }
                    }

                    public override void ContributeCompiledComponents(IComponentContainer existingComponents, IComponentContainerBuilder newComponents)
                    {
                        var allRegistrations = existingComponents.ResolveAll<ConsumingModelRegistration>();

                        foreach (var registration in allRegistrations)
                        {
                            var generatedComponentType = _generatedTypeByAppSpecificType[registration.AppSpecificModelType];

                            newComponents
                                .RegisterComponentType(generatedComponentType)
                                .WithAdapterParameter(registration.SupplierPort)
                                .InstancePerDependency()
                                .ForServices(registration.AppSpecificModelType);
                        }
                    }

                    private void GenerateAppSpecificModel(Type appSpecificType)
                    {
                        if (_generatedTypeByAppSpecificType.ContainsKey(appSpecificType))
                        {
                            return;
                        }

                        // here ConsumingModel_of_Xxxxxx classes are generated...
                        // below is mockup code...
                        if (appSpecificType == typeof(App.Module.IFirstAppSpecificInterface))
                        {
                            _generatedTypeByAppSpecificType[appSpecificType] = typeof(GeneratedCode.ConsumingModel_of_App_Module_FirstAppSpecificInterface);
                        }
                        else if (appSpecificType == typeof(App.Module.ISecondAppSpecificInterface))
                        {
                            _generatedTypeByAppSpecificType[appSpecificType] = typeof(GeneratedCode.ConsumingModel_of_App_Module_SecondAppSpecificInterface);
                        }
                    }
                }
            }

            namespace GeneratedCode
            {
                public class ConsumingModel_of_App_Module_FirstAppSpecificInterface : App.Module.IFirstAppSpecificInterface
                {
                    public ConsumingModel_of_App_Module_FirstAppSpecificInterface(SupplyingModel.Api.IAdapterToSupplier supplier)
                    {
                        this.Supplier = supplier;
                    }
                    public SupplyingModel.Api.IAdapterToSupplier Supplier { get; }
                }

                public class ConsumingModel_of_App_Module_SecondAppSpecificInterface : App.Module.ISecondAppSpecificInterface
                {
                    public ConsumingModel_of_App_Module_SecondAppSpecificInterface(SupplyingModel.Api.IAdapterToSupplier supplier)
                    {
                        this.Supplier = supplier;
                    }
                    public SupplyingModel.Api.IAdapterToSupplier Supplier { get; }
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        namespace SupplyingModel
        {
            namespace Api
            {
                public interface IAdapterToSupplier
                {
                }

                public class SupplierConfiguration
                {
                    public string Value { get; set; }
                    public bool SuppressAbcd { get; set; }
                }

                public class SupplierAdapterInjectionPort : AdapterInjectionPort<IAdapterToSupplier, SupplierConfiguration>
                {
                    public SupplierAdapterInjectionPort(
                        IComponentContainerBuilder containerBuilder, 
                        SupplierConfiguration defaultConfiguration) 
                        : base(containerBuilder, defaultConfiguration)
                    {
                    }
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        namespace SupplyingModel.Adapters.Abcd
        {
            using SupplyingModel.Api;

            public class AbcdFeature : AdvancedFeature
            {
                public override void ContributeAdapterComponents(IComponentContainer existingComponents, IComponentContainerBuilder newComponents)
                {
                    var allPorts = existingComponents.ResolveAll<SupplierAdapterInjectionPort>();

                    foreach (var port in allPorts.Where(p => !p.Configuration.SuppressAbcd))
                    {
                        port.Assign<AbcdAdapter>(newComponents);
                    }
                }
            }

            public class AbcdAdapter : IAdapterToSupplier
            {
                public AbcdAdapter(SupplierConfiguration configuration)
                {
                    Configuration = configuration;
                }
                public SupplierConfiguration Configuration { get; }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        namespace SupplyingModel.Adapters.Efgh
        {
            using SupplyingModel.Api;

            public class EfghFeature : AdvancedFeature
            {
                public override void ContributeAdapterComponents(IComponentContainer existingComponents, IComponentContainerBuilder newComponents)
                {
                    newComponents.RegisterComponentType<EfghAdapter>().InstancePerDependency();

                    var allPorts = existingComponents.ResolveAll<SupplierAdapterInjectionPort>();

                    foreach (var port in allPorts.Where(p => p.AdapterComponentType == null))
                    {
                        port.Assign<EfghAdapter>(newComponents);
                    }
                }
            }

            public class EfghAdapter : IAdapterToSupplier
            {
                public EfghAdapter(SupplierConfiguration configuration)
                {
                    Configuration = configuration;
                }
                public SupplierConfiguration Configuration { get; }
            }
        }
    }
}
