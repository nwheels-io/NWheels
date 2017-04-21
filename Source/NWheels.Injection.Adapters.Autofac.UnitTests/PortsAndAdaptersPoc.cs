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

            var allAdaptersA = container.ResolveAll<IExampleAdapterA>().ToArray();
            var allAdaptersB = container.ResolveAll<IExampleAdapterB>().ToArray();
            var allLifecycleListeners = container.ResolveAll<ILifecycleListenerComponent>().ToArray();

            allAdaptersA.Select(a => a.QualifiedValue).Should().BeEquivalentTo("DPY_ABC", "DPY_DEF");
            allAdaptersB.Select(a => a.QualifiedValue).Should().BeEquivalentTo("DPY_ZZZ");

            allLifecycleListeners.OfType<IExampleAdapterA>().Should().BeEquivalentTo(allAdaptersA);
            allLifecycleListeners.OfType<IExampleAdapterB>().Should().BeEquivalentTo(allAdaptersB);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class ExamplePortA : InjectorPort
        {
            public string Value { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class ExamplePortB : InjectorPort
        {
            public string Value { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface IExampleAdapterA
        {
            string QualifiedValue { get; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface IExampleAdapterB
        {
            string QualifiedValue { get; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class ExampleAdapterA : LifecycleListenerComponentBase, IExampleAdapterA
        {
            private readonly ExampleDependency _dependnecy;
            private readonly string _value;

            public ExampleAdapterA(ExampleDependency dependency, ExamplePortA port)
            {
                _dependnecy = dependency;
                _value = port.Value;
            }

            public string QualifiedValue => $"{_dependnecy.Prefix}_{_value}";
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class ExampleAdapterB : LifecycleListenerComponentBase, IExampleAdapterB
        {
            private readonly ExampleDependency _dependnecy;
            private readonly string _value;

            public ExampleAdapterB(ExampleDependency dependency, ExamplePortB port)
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
                containerBuilder.RegisterComponentType<ExampleDependency>().SingleInstance();//typeof(ExampleDependency), LifeStyle.Singleton);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class ExamplePortFeatureLoader : FeatureLoaderBase
        {
            public override void ContributeComponents(IComponentContainerBuilder containerBuilder)
            {
                base.ContributeComponents(containerBuilder);

                containerBuilder.ContributePortAExample("ABC");
                containerBuilder.ContributePortAExample("DEF");
                containerBuilder.ContributePortBExample("ZZZ");
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class ExampleAdapterFeatureLoader : FeatureLoaderBase
        {
            public override void ContributeAdapterComponents(IComponentContainer input, IComponentContainerBuilder output)
            {
                base.ContributeAdapterComponents(input, output);

                var allPortsA = input.ResolveAll<ExamplePortA>();

                foreach (var port in allPortsA)
                {
                    output.RegisterComponentType<ExampleAdapterA>()
                        .WithParameter<ExamplePortA>(port)
                        .SingleInstance()
                        .ForServices<IExampleAdapterA, ILifecycleListenerComponent>();
                }

                var allPortsB = input.ResolveAll<ExamplePortB>();

                foreach (var port in allPortsB)
                {
                    output.RegisterComponentType<ExampleAdapterB>()
                        .WithParameter<ExamplePortB>(port)
                        .SingleInstance()
                        .ForServices<IExampleAdapterB, ILifecycleListenerComponent>();
                }
            }
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public static class TestContainerBuilderExtensions
    {
        public static void ContributePortAExample(this IComponentContainerBuilder containerBuilder, string value)
        {
            containerBuilder.RegisterComponentInstance<PortsAndAdaptersPoc.ExamplePortA>(
                new PortsAndAdaptersPoc.ExamplePortA {
                    Value = value
                }
            );
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static void ContributePortBExample(this IComponentContainerBuilder containerBuilder, string value)
        {
            containerBuilder.RegisterComponentInstance<PortsAndAdaptersPoc.ExamplePortB>(
                new PortsAndAdaptersPoc.ExamplePortB {
                    Value = value
                }
            );
        }
    }
}
