using Autofac;
using FluentAssertions;
using NWheels.Microservices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace NWheels.Injection.Adapters.Autofac.UnitTests
{
    public class PortsAndAdaptersPoc
    {
        [Fact]
        public void CanConnectsPortsAndAdapters()
        {
            //-- arrange

            var builder = new ComponentContainerBuilder();
            var features = new List<IFeatureLoader> {
                new ExampleDependencyFeatureLoader(),
                new ExamplePortFeatureLoader(),
                new ExampleAdapterFeatureLoader(),
            };

            //-- act

            features.ForEach(f => f.ContributeComponents(builder));

            var container = builder.CreateComponentContainer(isRootContainer: true);
            var adapterBuilder = new ComponentContainerBuilder();

            features.ForEach(f => f.ContributeAdapterComponents(container, adapterBuilder));

            container.Merge(adapterBuilder);

            //-- assert

            var allAdapters = container.ResolveAll<IExampleAdapter>().ToArray();
            var allLifecycleListeners = container.ResolveAll<ILifecycleListenerComponent>().ToArray();

            allAdapters.Select(a => a.QualifiedValue).Should().BeEquivalentTo("DPY_ABC", "DPY_DEF");
            allLifecycleListeners.OfType<IExampleAdapter>().Should().BeEquivalentTo(allAdapters);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class ExamplePort : InjectorPort
        {
            public string Value { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface IExampleAdapter
        {
            string QualifiedValue { get; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class ExampleAdapter : LifecycleListenerComponentBase, IExampleAdapter
        {
            private readonly ExampleDependency _dependnecy;
            private readonly string _value;

            public ExampleAdapter(ExampleDependency dependency, ExamplePort port)
            {
                _dependnecy = dependency;
                _value = port.Value;
            }

            public string QualifiedValue => $"{_dependnecy.Prefix}_{_value}";
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class ExampleDependency
        {
            public string Prefix => "DPY";
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class ExampleDependencyFeatureLoader : FeatureLoaderBase
        {
            public override void ContributeComponents(IComponentContainerBuilder containerBuilder)
            {
                base.ContributeComponents(containerBuilder);
                containerBuilder.RegisterComponent2<ExampleDependency>().SingleInstance();//typeof(ExampleDependency), LifeStyle.Singleton);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class ExamplePortFeatureLoader : FeatureLoaderBase
        {
            public override void ContributeComponents(IComponentContainerBuilder containerBuilder)
            {
                base.ContributeComponents(containerBuilder);

                containerBuilder.ContributePortExample("ABC");
                containerBuilder.ContributePortExample("DEF");
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class ExampleAdapterFeatureLoader : FeatureLoaderBase
        {
            public override void ContributeAdapterComponents(IComponentContainer input, IComponentContainerBuilder output)
            {
                base.ContributeAdapterComponents(input, output);

                var allPorts = input.ResolveAll<ExamplePort>();

                foreach (var port in allPorts)
                {
                    output.RegisterComponent2<ExampleAdapter>()
                        .WithParameter<ExamplePort>(port)
                        .SingleInstance()
                        .As<IExampleAdapter, ILifecycleListenerComponent>();
                    
                    //output.Register<IExampleAdapter, ILifecycleListenerComponent, ExampleAdapter>(LifeStyle.Singleton).WithParameter<ExamplePort>(port);
                }
            }
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public static class TestContainerBuilderExtensions
    {
        public static void ContributePortExample(this IComponentContainerBuilder containerBuilder, string value)
        {
            containerBuilder.RegisterInstance2<PortsAndAdaptersPoc.ExamplePort>(
                new PortsAndAdaptersPoc.ExamplePort {
                    Value = value
                }
            );
        }
    }
}
