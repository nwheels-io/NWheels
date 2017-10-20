using System;
using System.Collections.Generic;
using System.Text;
using Autofac;
using Xunit;
using FluentAssertions;
using System.Linq;
using System.Reflection;

namespace NWheels.Kernel.UnitTests.Api.Injection
{
    public class AutofacPoc
    {
        [Fact]
        public void ResolveNamed_NotRegisteredAsService()
        {
            //-- arrange

            var builder = new ContainerBuilder();
            builder.RegisterType<ComponentA>().SingleInstance().Named<IServiceA>("A1").WithParameter(TypedParameter.From("AAA"));
            var container = builder.Build();

            //-- act

            var component = container.ResolveNamed<IServiceA>("A1");
            
            //-- assert

            component.Should().BeOfType<ComponentA>();
            ((ComponentA)component).Value.Should().Be("AAA");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        [Fact]
        public void ResolveAllForService_UnnamedRegisteredAsService()
        {
            //-- arrange

            var builder = new ContainerBuilder();
            builder.RegisterType<ComponentA>().SingleInstance().As<IServiceA>().WithParameter(TypedParameter.From("AAA"));
            builder.RegisterType<ComponentA>().SingleInstance().As<IServiceA>().WithParameter(TypedParameter.From("BBB"));
            var container = builder.Build();

            //-- act

            var components1 = container.Resolve<IEnumerable<IServiceA>>().ToArray();
            var components2 = container.Resolve<IEnumerable<IServiceA>>().ToArray();
            
            //-- assert

            components1.Length.Should().Be(2);
            components1.OfType<ComponentA>().Where(c => c.Value == "AAA").Count().Should().Be(1);
            components1.OfType<ComponentA>().Where(c => c.Value == "BBB").Count().Should().Be(1);

            components2.Should().Equal(components1);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        [Fact]
        public void ResolveAllForService_Named()
        {
            //-- arrange

            var builder = new ContainerBuilder();
            builder.RegisterType<ComponentA>().As<IServiceA>().SingleInstance().Named<IServiceA>("A1").WithParameter(TypedParameter.From("AAA"));
            builder.RegisterType<ComponentA>().As<IServiceA>().SingleInstance().Named<IServiceA>("B1").WithParameter(TypedParameter.From("BBB"));
            var container = builder.Build();

            //-- act

            var components = container.Resolve<IEnumerable<IServiceA>>().ToArray();
            
            //-- assert

            components.Length.Should().Be(2);
            components.Should().AllBeOfType<ComponentA>();
            components.OfType<ComponentA>().Where(c => c.Value == "AAA").Count().Should().Be(1);
            components.OfType<ComponentA>().Where(c => c.Value == "BBB").Count().Should().Be(1);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        [Fact]
        public void ResolveAllForService_MixedNamedUnnamed()
        {
            //-- arrange

            var builder = new ContainerBuilder();
            builder.RegisterType<ComponentA>().As<IServiceA>().SingleInstance().WithParameter(TypedParameter.From("000"));
            builder.RegisterType<ComponentA>().As<IServiceA>().SingleInstance().Named<IServiceA>("A1").WithParameter(TypedParameter.From("AAA"));
            builder.RegisterType<ComponentA>().As<IServiceA>().SingleInstance().Named<IServiceA>("B1").WithParameter(TypedParameter.From("BBB"));
            var container = builder.Build();

            //-- act

            var components = container.Resolve<IEnumerable<IServiceA>>().ToArray();
            
            //-- assert

            components.Length.Should().Be(3);
            components.Should().AllBeOfType<ComponentA>();
            components.OfType<ComponentA>().Where(c => c.Value == "000").Count().Should().Be(1);
            components.OfType<ComponentA>().Where(c => c.Value == "AAA").Count().Should().Be(1);
            components.OfType<ComponentA>().Where(c => c.Value == "BBB").Count().Should().Be(1);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        [Fact]
        public void ResolveAllForService_MixedNamedUnnamedOfMultipleTypes()
        {
            //-- arrange

            var builder = new ContainerBuilder();
            builder.RegisterType<ComponentA>().As<IServiceA>().SingleInstance().WithParameter(TypedParameter.From("ZZZ"));
            builder.RegisterType<ComponentB>().As<IServiceA>().SingleInstance().WithParameter(TypedParameter.From(0));
            builder.RegisterType<ComponentA>().As<IServiceA>().SingleInstance().Named<IServiceA>("A1").WithParameter(TypedParameter.From("AAA"));
            builder.RegisterType<ComponentB>().As<IServiceA>().SingleInstance().Named<IServiceA>("B1").WithParameter(TypedParameter.From(111));
            var container = builder.Build();

            //-- act

            var components = container.Resolve<IEnumerable<IServiceA>>().ToArray();
            
            //-- assert

            components.Length.Should().Be(4);
            components.OfType<ComponentA>().Where(c => c.Value == "ZZZ").Count().Should().Be(1);
            components.OfType<ComponentA>().Where(c => c.Value == "AAA").Count().Should().Be(1);
            components.OfType<ComponentB>().Where(c => c.Value == 0).Count().Should().Be(1);
            components.OfType<ComponentB>().Where(c => c.Value == 111).Count().Should().Be(1);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void PortsAndAdapters()
        {
            void contributeComponents(ContainerBuilder newComponents)
            {
                contributeModel(newComponents, "M1", "ABC");
                contributeModel(newComponents, "M2", 123);
            }

            void contributeModel(ContainerBuilder newComponents, string name, object serviceAConfig)
            {
                var serviceAPort = new ServiceAPort(serviceAConfig);
                newComponents.RegisterInstance(serviceAPort).Keyed<ServiceAPort>(serviceAPort.Key).As<ServiceAPort>();
                newComponents.RegisterType<ModelOne>().Named<ModelOne>(name).WithParameter(TestPortAdapterParameter.FromPort(serviceAPort));
            }

            void contributeAdapterComponents(IComponentContext existingComponents, ContainerBuilder newComponents)
            {
                var allPorts = existingComponents.Resolve<IEnumerable<ServiceAPort>>();
                
                foreach (var port in allPorts)
                {
                    if (port.Config is string)
                    {
                        port.AdapterType = typeof(ServiceAStringAdapter);
                        newComponents.RegisterType<ServiceAStringAdapter>().Keyed<IServiceA>(port.Key).WithParameter(TestPortConfigParameter.FromPort(port));
                    }
                    else if (port.Config is int)
                    {
                        port.AdapterType = typeof(ServiceAIntAdapter);
                        newComponents.RegisterType<ServiceAIntAdapter>().Keyed<IServiceA>(port.Key).WithParameter(TestPortConfigParameter.FromPort(port));
                    }
                    else
                    {
                        throw new NotSupportedException();
                    }
                }
            }

            //-- arrange

            var builder1 = new ContainerBuilder();

            contributeComponents(builder1);

            var container1 = builder1.Build();
            var builder2 = new ContainerBuilder();

            contributeAdapterComponents(container1, builder2);

            #pragma warning disable 618
            builder2.Update(container1);
            #pragma warning restore 618

            //-- act

            var model1 = container1.ResolveNamed<ModelOne>("M1");
            var model2 = container1.ResolveNamed<ModelOne>("M2");
            
            //-- assert

            model1.Adapter.Should().BeOfType<ServiceAStringAdapter>().Which.Value.Should().Be("ABC");
            model2.Adapter.Should().BeOfType<ServiceAIntAdapter>().Which.Value.Should().Be(123);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface IServiceA
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        public class ComponentA : IServiceA
        {
            public ComponentA(string value)
            {
                this.Value = value;
            }
            public string Value { get; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        public class ComponentB : IServiceA
        {
            public ComponentB(int value)
            {
                this.Value = value;
            }
            public int Value { get; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        public class ModelOne
        {
            public ModelOne(IServiceA adapter)
            {
                this.Adapter = adapter;
            }
            public IServiceA Adapter { get; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface IPort<TAdapter, TConfig>
        {
            TAdapter Resolve(IComponentContext context);
            int Key { get; }
            TConfig Config { get; }
            Type AdapterType { get; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class ServiceAPort : IPort<IServiceA, object>
        {
            private static int _s_lastKey = 0;
            public ServiceAPort(object config)
            {
                this.Key = ++_s_lastKey;
                this.Config = config;
            }
            public IServiceA Resolve(IComponentContext context)
            {
                return (IServiceA)context.ResolveKeyed(this.Key, typeof(IServiceA), TypedParameter.From<object>(this.Config));
            }
            public int Key { get; }
            public object Config { get; }
            public Type AdapterType { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        public class ServiceAStringAdapter : IServiceA
        {
            public ServiceAStringAdapter(object config)
            {
                this.Value = (string)config;
            }
            public string Value { get; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        public class ServiceAIntAdapter : IServiceA
        {
            public ServiceAIntAdapter(object config)
            {
                this.Value = (int)config;
            }
            public int Value { get; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        public static class TestPortAdapterParameter
        {
            public static Autofac.Core.Parameter FromPort<TAdapter, TConfig>(IPort<TAdapter, TConfig> port)
            {
                return new TestPortAdapterParameter<TAdapter, TConfig>(port);
            }
        }
        public class TestPortAdapterParameter<TAdapter, TConfig> : Autofac.Core.Parameter
        {
            public TestPortAdapterParameter(IPort<TAdapter, TConfig> port)
            {
                this.PortType = port.GetType();
                this.PortKey = port.Key;
            }
            public override bool CanSupplyValue(ParameterInfo pi, IComponentContext context, out Func<object> valueProvider)
            {
                if (typeof(TAdapter).IsAssignableFrom(pi.ParameterType))
                {
                    valueProvider = () => {
                        var port = (IPort<TAdapter, TConfig>)context.ResolveKeyed(PortKey, PortType);
                        return port.Resolve(context);
                    };
                    return true;
                }
                valueProvider = null;
                return false;
            }
            public Type PortType { get; }
            public int PortKey { get; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        public static class TestPortConfigParameter
        {
            public static Autofac.Core.Parameter FromPort<TAdapter, TConfig>(IPort<TAdapter, TConfig> port)
            {
                return new TestPortConfigParameter<TAdapter, TConfig>(port);
            }
        }
        public class TestPortConfigParameter<TAdapter, TConfig> : Autofac.Core.Parameter
        {
            public TestPortConfigParameter(IPort<TAdapter, TConfig> port)
            {
                this.PortType = port.GetType();
                this.PortKey = port.Key;
            }
            public override bool CanSupplyValue(ParameterInfo pi, IComponentContext context, out Func<object> valueProvider)
            {
                if (typeof(TConfig).IsAssignableFrom(pi.ParameterType))
                {
                    valueProvider = () => {
                        var port = (IPort<TAdapter, TConfig>)context.ResolveKeyed(PortKey, PortType);
                        return port.Config;
                    };
                    return true;
                }
                valueProvider = null;
                return false;
            }
            public Type PortType { get; }
            public int PortKey { get; }
        }
    }
}