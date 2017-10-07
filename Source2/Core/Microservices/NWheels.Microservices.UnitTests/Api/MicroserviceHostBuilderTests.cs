using System;
using FluentAssertions;
using NWheels.Microservices;
using Xunit;
using System.Reflection;
using NWheels.Microservices.Runtime;
using NWheels.Kernel.Api.Injection;
using NWheels.Testability;
using NWheels.Microservices.Api;
using System.Linq;
using System.Collections.Generic;
using NWheels.Kernel.Runtime.Injection;
using System.Threading;
using System.Xml.Linq;

namespace NWheels.Microservices.UnitTests.Api
{
    public class MicroserviceHostBuilderTests : TestBase.UnitTest
    {
        [Fact]
        public void UseFrameworkFeature_DefaultFeature_ModuleAdded()
        {
            //-- arrange

            var builder = new MicroserviceHostBuilder("TestService");

            //-- act

            builder.UseFrameworkFeature<TestDefaultFeatureLoader>();

            //-- assert

            builder.BootConfig.FrameworkModules.Select(m => m.RuntimeAssembly).Should().Equal(typeof(TestDefaultFeatureLoader).Assembly);
            builder.BootConfig.FrameworkModules[0].Features.Should().BeEmpty();

            builder.BootConfig.ApplicationModules.Should().BeEmpty();
            builder.BootConfig.CustomizationModules.Should().BeEmpty();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void UseFrameworkFeature_NamedFeature_ModuleAndFeatureAdded()
        {
            //-- arrange

            var builder = new MicroserviceHostBuilder("TestService");

            //-- act

            builder.UseFrameworkFeature<TestNamedFeatureLoader>();

            //-- assert

            builder.BootConfig.FrameworkModules.Select(m => m.RuntimeAssembly).Should().Equal(typeof(TestNamedFeatureLoader).Assembly);
            builder.BootConfig.FrameworkModules[0].Features.Select(f => f.FeatureLoaderRuntimeType).Should().Equal(typeof(TestNamedFeatureLoader));

            builder.BootConfig.ApplicationModules.Should().BeEmpty();
            builder.BootConfig.CustomizationModules.Should().BeEmpty();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void UseApplicationFeature_DefaultFeature_ModuleAdded()
        {
            //-- arrange

            var builder = new MicroserviceHostBuilder("TestService");

            //-- act

            builder.UseApplicationFeature<TestDefaultFeatureLoader>();

            //-- assert

            builder.BootConfig.ApplicationModules.Select(m => m.RuntimeAssembly).Should().Equal(typeof(TestDefaultFeatureLoader).Assembly);
            builder.BootConfig.ApplicationModules[0].Features.Should().BeEmpty();

            builder.BootConfig.FrameworkModules.Should().BeEmpty();
            builder.BootConfig.CustomizationModules.Should().BeEmpty();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void UseApplicationFeature_NamedFeature_ModuleAndFeatureAdded()
        {
            //-- arrange

            var builder = new MicroserviceHostBuilder("TestService");

            //-- act

            builder.UseApplicationFeature<TestNamedFeatureLoader>();

            //-- assert

            builder.BootConfig.ApplicationModules.Select(m => m.RuntimeAssembly).Should().Equal(typeof(TestNamedFeatureLoader).Assembly);
            builder.BootConfig.ApplicationModules[0].Features.Select(f => f.FeatureLoaderRuntimeType).Should().Equal(typeof(TestNamedFeatureLoader));

            builder.BootConfig.FrameworkModules.Should().BeEmpty();
            builder.BootConfig.CustomizationModules.Should().BeEmpty();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void UseCustomizationFeature_DefaultFeature_ModuleAdded()
        {
            //-- arrange

            var builder = new MicroserviceHostBuilder("TestService");

            //-- act

            builder.UseCustomizationFeature<TestDefaultFeatureLoader>();

            //-- assert

            builder.BootConfig.CustomizationModules.Select(m => m.RuntimeAssembly).Should().Equal(typeof(TestDefaultFeatureLoader).Assembly);
            builder.BootConfig.CustomizationModules[0].Features.Should().BeEmpty();

            builder.BootConfig.FrameworkModules.Should().BeEmpty();
            builder.BootConfig.ApplicationModules.Should().BeEmpty();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void UseCustomizationFeature_NamedFeature_ModuleAndFeatureAdded()
        {
            //-- arrange

            var builder = new MicroserviceHostBuilder("TestService");

            //-- act

            builder.UseCustomizationFeature<TestNamedFeatureLoader>();

            //-- assert

            builder.BootConfig.CustomizationModules.Select(m => m.RuntimeAssembly).Should().Equal(typeof(TestNamedFeatureLoader).Assembly);
            builder.BootConfig.CustomizationModules[0].Features.Select(f => f.FeatureLoaderRuntimeType).Should().Equal(typeof(TestNamedFeatureLoader));

            builder.BootConfig.FrameworkModules.Should().BeEmpty();
            builder.BootConfig.ApplicationModules.Should().BeEmpty();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void CanUseMicroserviceXml()
        {
            //-- arrange

            var builder = new MicroserviceHostBuilder("TestService");
            var microserviceXml = @"
                <microservice>
                    <framework-modules>
                        <module assembly='FX.M1' />
                    </framework-modules>
                    <application-modules>
                        <module assembly='App.M2' />
                    </application-modules>
                    <customization-modules>
                        <module assembly='Custom.M3' />
                    </customization-modules>
                </microservice>";

            //-- act

            builder.UseMicroserviceXml(XElement.Parse(microserviceXml));
                
            //-- assert

            builder.BootConfig.FrameworkModules.Single().ModuleName.Should().Be("FX.M1");
            builder.BootConfig.ApplicationModules.Single().ModuleName.Should().Be("App.M2");
            builder.BootConfig.CustomizationModules.Single().ModuleName.Should().Be("Custom.M3");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void CanContributeComponents()
        {
            //-- arrange

            var builder = new MicroserviceHostBuilder("TestService");
            var componentOne = new TestComponentOne();
            var componentTwo = new TestComponentTwo();
            var componentThree = new TestComponentThree();

            //-- act

            builder.ContributeComponents((existingComponents, newComponents) => {
                newComponents.RegisterComponentInstance(componentOne);
                newComponents.RegisterComponentInstance(componentTwo);
            });
            builder.ContributeComponents((existingComponents, newComponents) => newComponents.RegisterComponentInstance(componentThree));
                
            var host = builder.BuildHost();
            host.Configure(CancellationToken.None);
            
            var resolvedComponentOne = host.GetModuleComponents().Resolve<TestComponentOne>();
            var resolvedComponentTwo = host.GetModuleComponents().Resolve<TestComponentTwo>();
            var resolvedComponentThree = host.GetModuleComponents().Resolve<TestComponentThree>();

            //-- assert

            resolvedComponentOne.Should().BeSameAs(componentOne);
            resolvedComponentTwo.Should().BeSameAs(componentTwo);
            resolvedComponentThree.Should().BeSameAs(componentThree);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class TestDefaultFeatureLoader : FeatureLoaderBase
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [FeatureLoader(Name = "Named")]
        private class TestNamedFeatureLoader : FeatureLoaderBase
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class TestComponentOne
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class TestComponentTwo
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class TestComponentThree
        {
        }
    }
}
