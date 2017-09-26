using Autofac;
using FluentAssertions;
using NWheels.Kernel.Api.Injection;
using NWheels.Kernel.Runtime.Injection;
using NWheels.Testability;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace NWheels.Kernel.UnitTests.Runtime.Injection
{
    public class PortsAndAdaptersPoc : TestBase.UnitTest
    {
        [Fact]
        public void CanConnectsPortsAndAdapters()
        {
            //-- arrange

            var container = new ComponentContainerBuilder().CreateComponentContainer();
            var builder = new ComponentContainerBuilder();
            var features = new List<IFeatureLoader> {
                new ExampleDependencyFeatureLoader(),
                new ExamplePortFeatureLoader(),
                new ExampleAdapterFeatureLoader(),
            };

            //-- act

            features.ForEach(f => f.ContributeComponents(container, builder));

            container.Merge(builder);
            var adapterBuilder = new ComponentContainerBuilder();

            features.ForEach(f => f.ContributeAdapterComponents(container, adapterBuilder));

            container.Merge(adapterBuilder);

            //-- assert

            var allAdaptersA = container.ResolveAll<IExampleAdapterA>().ToArray();
            var allAdaptersB = container.ResolveAll<IExampleAdapterB>().ToArray();
            var allLifecycleListeners = container.ResolveAll<ITestLifecycleComponent>().ToArray();

            allAdaptersA.Select(a => a.QualifiedValue).Should().BeEquivalentTo("DPY_ABC", "DPY_DEF");
            allAdaptersB.Select(a => a.QualifiedValue).Should().BeEquivalentTo("DPY_ZZZ");

            allLifecycleListeners.OfType<IExampleAdapterA>().Should().BeEquivalentTo(allAdaptersA);
            allLifecycleListeners.OfType<IExampleAdapterB>().Should().BeEquivalentTo(allAdaptersB);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class ExamplePortA : InjectorPort
        {
            public ExamplePortA(IComponentContainerBuilder containerBuilder) : base(containerBuilder)
            {
            }

            public string Value { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class ExamplePortB : InjectorPort
        {
            public ExamplePortB(IComponentContainerBuilder containerBuilder) : base(containerBuilder)
            {
            }

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

        public interface ITestLifecycleComponent
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class ExampleAdapterA : ITestLifecycleComponent, IExampleAdapterA
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

        public class ExampleAdapterB : ITestLifecycleComponent, IExampleAdapterB
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
            public override void ContributeComponents(IComponentContainer existingComponents, IComponentContainerBuilder newComponents)
            {
                base.ContributeComponents(existingComponents, newComponents);
                newComponents.RegisterComponentType<ExampleDependency>().SingleInstance();//typeof(ExampleDependency), LifeStyle.Singleton);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class ExamplePortFeatureLoader : FeatureLoaderBase
        {
            public override void ContributeComponents(IComponentContainer existingComponents, IComponentContainerBuilder newComponents)
            {
                base.ContributeComponents(existingComponents, newComponents);

                newComponents.ContributePortAExample("ABC");
                newComponents.ContributePortAExample("DEF");
                newComponents.ContributePortBExample("ZZZ");
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class ExampleAdapterFeatureLoader : FeatureLoaderBase
        {
            public override void ContributeAdapterComponents(IComponentContainer existingComponents, IComponentContainerBuilder newComponents)
            {
                base.ContributeAdapterComponents(existingComponents, newComponents);

                var allPortsA = existingComponents.ResolveAll<ExamplePortA>();

                foreach (var port in allPortsA)
                {
                    newComponents.RegisterComponentType<ExampleAdapterA>()
                        .WithParameter<ExamplePortA>(port)
                        .SingleInstance()
                        .ForServices<IExampleAdapterA, ITestLifecycleComponent>();
                }

                var allPortsB = existingComponents.ResolveAll<ExamplePortB>();

                foreach (var port in allPortsB)
                {
                    newComponents.RegisterComponentType<ExampleAdapterB>()
                        .WithParameter<ExamplePortB>(port)
                        .SingleInstance()
                        .ForServices<IExampleAdapterB, ITestLifecycleComponent>();
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
                new PortsAndAdaptersPoc.ExamplePortA(containerBuilder) {
                    Value = value
                }
            );
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static void ContributePortBExample(this IComponentContainerBuilder containerBuilder, string value)
        {
            containerBuilder.RegisterComponentInstance<PortsAndAdaptersPoc.ExamplePortB>(
                new PortsAndAdaptersPoc.ExamplePortB(containerBuilder) {
                    Value = value
                }
            );
        }
    }
}
