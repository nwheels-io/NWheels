using System;
using System.Collections.Generic;
using FluentAssertions;
using NWheels.Microservices;
using NWheels.Microservices.Api;
using System.IO;
using System.Xml.Serialization;
using Xunit;
using System.Xml.Linq;
using NWheels.Testability;
using NWheels.Microservices.Runtime;
using NWheels.Kernel.Api.Logging;
using System.Linq;
using NWheels.Kernel.Api.Injection;
using NWheels.Kernel.Runtime.Injection;
using NWheels.Microservices.Api.Exceptions;

namespace NWheels.Microservices.UnitTests.Runtime
{
    public class MutableBootConfigurationTests : TestBase.UnitTest
    {
        [Fact]
        public void Constructor_InitialState()
        {
            //-- act

            var bootConfig = new MutableBootConfiguration();

            //-- assert

            bootConfig.MicroserviceName.Should().BeNull();
            bootConfig.IsPrecompiledMode.Should().BeFalse();
            bootConfig.IsBatchJobMode.Should().BeFalse();
            bootConfig.ClusterName.Should().BeNull();
            bootConfig.ClusterPartition.Should().BeNull();
            bootConfig.AssemblyLocationMap.Should().BeNull();
            bootConfig.LogLevel.Should().Be(LogLevel.Info);

            bootConfig.EnvironmentVariables.Should().NotBeNull();
            bootConfig.EnvironmentVariables.Should().BeEmpty();

            bootConfig.BootComponents.Should().NotBeNull();

            bootConfig.FrameworkModules.Should().NotBeNull();
            bootConfig.FrameworkModules.Should().BeEmpty();

            bootConfig.ApplicationModules.Should().NotBeNull();
            bootConfig.ApplicationModules.Should().BeEmpty();

            bootConfig.CustomizationModules.Should().NotBeNull();
            bootConfig.CustomizationModules.Should().BeEmpty();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        [Fact]
        public void AddFeaturesByNames_ModuleNotListed_ModuleAdded()
        {
            //-- arrange

            var bootConfig = new MutableBootConfiguration();

            //-- act

            bootConfig.AddFeatures(bootConfig.ApplicationModules, "M1", "F1", "F2");

            //-- assert 

            bootConfig.ApplicationModules.Count.Should().Be(1);
            bootConfig.ApplicationModules[0].AssemblyName.Should().Be("M1");
            bootConfig.ApplicationModules[0].RuntimeAssembly.Should().BeNull();
            bootConfig.ApplicationModules[0].ModuleName.Should().Be("M1");
            bootConfig.ApplicationModules[0].Features.Should().NotBeNull();
            bootConfig.ApplicationModules[0].Features.Count.Should().Be(2);
            bootConfig.ApplicationModules[0].Features[0].FeatureName.Should().Be("F1");
            bootConfig.ApplicationModules[0].Features[0].FeatureLoaderRuntimeType.Should().BeNull();
            bootConfig.ApplicationModules[0].Features[1].FeatureName.Should().Be("F2");
            bootConfig.ApplicationModules[0].Features[1].FeatureLoaderRuntimeType.Should().BeNull();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        [Fact]
        public void AddFeaturesByNames_ModuleAlreadyListed_FeaturesAdded()
        {
            //-- arrange

            var bootConfig = new MutableBootConfiguration();
            bootConfig.AddFeatures(bootConfig.ApplicationModules, "M1", "F1", "F2");

            //-- act

            bootConfig.AddFeatures(bootConfig.ApplicationModules, "M1", "F3", "F4");

            //-- assert 

            bootConfig.ApplicationModules.Count.Should().Be(1);
            bootConfig.ApplicationModules[0].AssemblyName.Should().Be("M1");
            bootConfig.ApplicationModules[0].Features.Count.Should().Be(4);
            bootConfig.ApplicationModules[0].Features.Select(f => f.FeatureName).Should().Equal("F1", "F2", "F3", "F4");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        [Fact]
        public void AddFeaturesByNames_MixListedAndNotListedModules()
        {
            //-- arrange

            var bootConfig = new MutableBootConfiguration();
            bootConfig.AddFeatures(bootConfig.ApplicationModules, "M1", "F1");
            bootConfig.AddFeatures(bootConfig.ApplicationModules, "M2", "F2");

            //-- act

            bootConfig.AddFeatures(bootConfig.ApplicationModules, "M2", "F3", "F4");
            bootConfig.AddFeatures(bootConfig.ApplicationModules, "M3", "F5");

            //-- assert 

            bootConfig.ApplicationModules.Select(m => m.AssemblyName).Should().Equal("M1", "M2", "M3");
            bootConfig.ApplicationModules[0].Features.Select(f => f.FeatureName).Should().Equal("F1");
            bootConfig.ApplicationModules[1].Features.Select(f => f.FeatureName).Should().Equal("F2", "F3", "F4");
            bootConfig.ApplicationModules[2].Features.Select(f => f.FeatureName).Should().Equal("F5");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        [Fact]
        public void AddFeaturesByNames_DuplicateFeaturesIgnored()
        {
            //-- arrange

            var bootConfig = new MutableBootConfiguration();
            bootConfig.AddFeatures(bootConfig.ApplicationModules, "M1", "F1");
            bootConfig.AddFeatures(bootConfig.ApplicationModules, "M2", "F2", "F3");

            //-- act

            bootConfig.AddFeatures(bootConfig.ApplicationModules, "M1", "F1");
            bootConfig.AddFeatures(bootConfig.ApplicationModules, "M2", "F4", "F3", "F2");

            //-- assert 

            bootConfig.ApplicationModules.Select(m => m.AssemblyName).Should().Equal("M1", "M2");
            bootConfig.ApplicationModules[0].Features.Select(f => f.FeatureName).Should().Equal("F1");
            bootConfig.ApplicationModules[1].Features.Select(f => f.FeatureName).Should().Equal("F2", "F3", "F4");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        [Fact]
        public void AddFeaturesByLoaderTypes_NoModulesListed_ModuleAndFeaturesAdded()
        {
            //-- arrange

            var bootConfig = new MutableBootConfiguration();
            var assembly1 = typeof(NWheels.Kernel.Api.Injection.IFeatureLoader).Assembly;

            //-- act

            bootConfig.AddFeatures(bootConfig.ApplicationModules, assembly1, typeof(FeatureLoader1), typeof(FeatureLoader2));

            //-- assert 

            bootConfig.ApplicationModules.Count.Should().Be(1);
            bootConfig.ApplicationModules[0].RuntimeAssembly.Should().BeSameAs(assembly1);
            bootConfig.ApplicationModules[0].AssemblyName.Should().BeNull();
            bootConfig.ApplicationModules[0].ModuleName.Should().Be("NWheels.Kernel");
            bootConfig.ApplicationModules[0].Features.Should().NotBeNull();
            bootConfig.ApplicationModules[0].Features.Count.Should().Be(2);
            bootConfig.ApplicationModules[0].Features[0].FeatureLoaderRuntimeType.Should().BeSameAs(typeof(FeatureLoader1));
            bootConfig.ApplicationModules[0].Features[0].FeatureName.Should().Be("F1");
            bootConfig.ApplicationModules[0].Features[1].FeatureLoaderRuntimeType.Should().BeSameAs(typeof(FeatureLoader2));
            bootConfig.ApplicationModules[0].Features[1].FeatureName.Should().Be("F2");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        [Fact]
        public void AddFeaturesByLoaderTypes_MixListedAndNotListedModules()
        {
            //-- arrange

            var bootConfig = new MutableBootConfiguration();
            var assembly1 = typeof(NWheels.Kernel.Api.Injection.IFeatureLoader).Assembly;
            var assembly2 = typeof(NWheels.Microservices.Api.ILifecycleComponent).Assembly;
            var assembly3 = this.GetType().Assembly;

            bootConfig.AddFeatures(bootConfig.ApplicationModules, assembly1, typeof(FeatureLoader1));
            bootConfig.AddFeatures(bootConfig.ApplicationModules, assembly2, typeof(FeatureLoader2));

            //-- act

            bootConfig.AddFeatures(bootConfig.ApplicationModules, assembly2, typeof(FeatureLoader3), typeof(FeatureLoader4));
            bootConfig.AddFeatures(bootConfig.ApplicationModules, assembly3, typeof(FeatureLoader5));

            //-- assert 

            bootConfig.ApplicationModules.Select(m => m.ModuleName).Should()
                .Equal("NWheels.Kernel", "NWheels.Microservices", "NWheels.Microservices.UnitTests");
            
            bootConfig.ApplicationModules[0].Features.Select(f => f.FeatureName).Should().Equal("F1");
            bootConfig.ApplicationModules[0].Features.Select(f => f.FeatureLoaderRuntimeType).Should()
                .Equal(typeof(FeatureLoader1));

            bootConfig.ApplicationModules[1].Features.Select(f => f.FeatureName).Should().Equal("F2", "F3", "F4");
            bootConfig.ApplicationModules[1].Features.Select(f => f.FeatureLoaderRuntimeType).Should()
                .Equal(typeof(FeatureLoader2), typeof(FeatureLoader3), typeof(FeatureLoader4));

            bootConfig.ApplicationModules[2].Features.Select(f => f.FeatureName).Should().Equal("F5");
            bootConfig.ApplicationModules[2].Features.Select(f => f.FeatureLoaderRuntimeType).Should()
                .Equal(typeof(FeatureLoader5));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        [Fact]
        public void AddFeaturesByLoaderTypes_DuplicateLoadersIgnored()
        {
            //-- arrange

            var assembly1 = typeof(NWheels.Kernel.Api.Injection.IFeatureLoader).Assembly;
            var assembly2 = typeof(NWheels.Microservices.Api.ILifecycleComponent).Assembly;
            var assembly3 = this.GetType().Assembly;

            var bootConfig = new MutableBootConfiguration();
            bootConfig.AddFeatures(bootConfig.ApplicationModules, assembly1, typeof(FeatureLoader1));
            bootConfig.AddFeatures(bootConfig.ApplicationModules, assembly2, typeof(FeatureLoader2), typeof(FeatureLoader3));

            //-- act

            bootConfig.AddFeatures(bootConfig.ApplicationModules, assembly1, typeof(FeatureLoader1));
            bootConfig.AddFeatures(bootConfig.ApplicationModules, assembly2, typeof(FeatureLoader4), typeof(FeatureLoader3), typeof(FeatureLoader2));
            bootConfig.AddFeatures(bootConfig.ApplicationModules, assembly3, typeof(FeatureLoader5), typeof(FeatureLoader5));

            //-- assert 

            bootConfig.ApplicationModules.Select(m => m.RuntimeAssembly).Should().Equal(assembly1, assembly2, assembly3);
            bootConfig.ApplicationModules[0].Features.Select(f => f.FeatureName).Should().Equal("F1");
            bootConfig.ApplicationModules[1].Features.Select(f => f.FeatureName).Should().Equal("F2", "F3", "F4");
            bootConfig.ApplicationModules[2].Features.Select(f => f.FeatureName).Should().Equal("F5");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        [Theory]
        [InlineData(LogLevel.Critical, false)]
        [InlineData(LogLevel.Error, false)]
        [InlineData(LogLevel.Warning, false)]
        [InlineData(LogLevel.Info, false)]
        [InlineData(LogLevel.Verbose, false)]
        [InlineData(LogLevel.Debug, true)]
        public void IsDebugMode_TrueIfLogLevelIsDebug(LogLevel logLevel, bool expectedIsDebugMode)
        {
            //-- arrange

            var bootConfig = new MutableBootConfiguration();
            bootConfig.LogLevel = logLevel;

            //-- act

            var actualIsDebugMode = bootConfig.IsDebugMode;

            //-- assert 

            actualIsDebugMode.Should().Be(expectedIsDebugMode);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        [Theory]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("C1", true)]
        public void IsClusteredMode_TrueIfClusterNameSpecified(string clusterName, bool expectedIsClusteredMode)
        {
            //-- arrange

            var bootConfig = new MutableBootConfiguration();
            bootConfig.ClusterName = clusterName;

            //-- act

            var actualIsClusteredMode = bootConfig.IsClusteredMode;

            //-- assert 

            actualIsClusteredMode.Should().Be(expectedIsClusteredMode);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void CanRegisterBootComponents()
        {
            //-- arrange

            var bootConfig = new MutableBootConfiguration();

            var registeredComponent = new ComponentA();
            var containerBuilder = new ComponentContainerBuilder();

            //-- act

            bootConfig.BootComponents.Register(builder => builder.RegisterComponentInstance(registeredComponent));
            ((IBootConfiguration)bootConfig).BootComponents.Contribute(containerBuilder);

            //-- assert

            var container = containerBuilder.CreateComponentContainer();
            var resolvedComponent = container.Resolve<ComponentA>();

            resolvedComponent.Should().BeSameAs(registeredComponent);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void Validate_MicroserviceNameSpecified_Success()
        {
            //-- arrange

            var bootConfig = new MutableBootConfiguration();
            bootConfig.MicroserviceName = "TestService";

            //-- act & assert

            bootConfig.Validate();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void Validate_MicroserviceNameNull_Throw()
        {
            //-- arrange

            var bootConfig = new MutableBootConfiguration();
            Action act = () => bootConfig.Validate();

            //-- act & assert

            act.ShouldThrow<BootConfigurationException>().Where(exc => exc.Reason == nameof(BootConfigurationException.MicroserviceNameNotSpecified));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void Validate_MicroserviceNameEmpty_Throw()
        {
            //-- arrange

            var bootConfig = new MutableBootConfiguration();
            bootConfig.MicroserviceName = string.Empty;

            Action act = () => bootConfig.Validate();

            //-- act & assert

            act.ShouldThrow<BootConfigurationException>().Where(exc => exc.Reason == nameof(BootConfigurationException.MicroserviceNameNotSpecified));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void Validate_NoFrameworkModules_KernelModuleAdded()
        {
            //-- arrange

            var bootConfig = new MutableBootConfiguration();
            bootConfig.MicroserviceName = "TestService";

            //-- act

            bootConfig.Validate();

            //-- assert

            bootConfig.FrameworkModules.Select(m => m.RuntimeAssembly).Should().Equal(MutableBootConfiguration.KernelAssembly);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void Validate_KernelModuleNotListed_InsertedFirst()
        {
            //-- arrange

            var bootConfig = new MutableBootConfiguration();
            bootConfig.MicroserviceName = "TestService";
            bootConfig.AddFeatures(bootConfig.FrameworkModules, this.GetType().Assembly);

            //-- act

            bootConfig.Validate();

            //-- assert

            bootConfig.FrameworkModules.Count.Should().Be(2);
            bootConfig.FrameworkModules[0].RuntimeAssembly.Should().BeSameAs(MutableBootConfiguration.KernelAssembly);
            bootConfig.FrameworkModules[1].RuntimeAssembly.Should().BeSameAs(this.GetType().Assembly);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void Validate_KernelModuleNotListedFirst_Throw()
        {
            //-- arrange

            var bootConfig = new MutableBootConfiguration();
            bootConfig.MicroserviceName = "TestService";
            bootConfig.AddFeatures(bootConfig.FrameworkModules, this.GetType().Assembly);
            bootConfig.AddFeatures(bootConfig.FrameworkModules, MutableBootConfiguration.KernelAssembly);

            //-- act & assert

            Action act = () => bootConfig.Validate();
            var exception = act.ShouldThrow<BootConfigurationException>().Which;

            //-- assert

            exception.Should().NotBeNull();
            exception.Reason.Should().Be(nameof(BootConfigurationException.KernelModuleItemInvalidLocation));
            exception.ModuleName.Should().Be(MutableBootConfiguration.KernelAssemblyName);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Theory]
        [InlineData(nameof(MutableBootConfiguration.FrameworkModules), nameof(MutableBootConfiguration.ApplicationModules))]
        [InlineData(nameof(MutableBootConfiguration.FrameworkModules), nameof(MutableBootConfiguration.CustomizationModules))]
        [InlineData(nameof(MutableBootConfiguration.ApplicationModules), nameof(MutableBootConfiguration.CustomizationModules))]
        public void Validate_ModuleListedInMultipleCollections_Throw(string collectionName1, string collectionName2)
        {
            //-- arrange

            var assembly1 = typeof(NWheels.Microservices.Api.ILifecycleComponent).Assembly;
            var assembly2 = typeof(NWheels.Testability.TestBase).Assembly;
            var assembly3 = this.GetType().Assembly;

            var bootConfig = new MutableBootConfiguration();
            
            var collection1 = (List<MutableBootConfiguration.ModuleConfiguration>)
                bootConfig.GetType().GetProperty(collectionName1).GetValue(bootConfig);
            var collection2 = (List<MutableBootConfiguration.ModuleConfiguration>)
                bootConfig.GetType().GetProperty(collectionName2).GetValue(bootConfig);

            bootConfig.MicroserviceName = "TestService";

            bootConfig.AddFeatures(collection1, assembly1);
            bootConfig.AddFeatures(collection1, assembly3);

            bootConfig.AddFeatures(collection2, assembly2); 
            bootConfig.AddFeatures(collection2, assembly3); // assembly3 is listed twice 

            //-- act & assert

            Action act = () => bootConfig.Validate();
            var exception = act.ShouldThrow<BootConfigurationException>().Which;

            //-- assert

            exception.Should().NotBeNull();
            exception.Reason.Should().Be(nameof(BootConfigurationException.ModuleListedMultipleTimes));
            exception.ModuleName.Should().Be(assembly3.GetName().Name);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void Validate_KernelModuleListedFirst_NoChange()
        {
            //-- arrange

            var bootConfig = new MutableBootConfiguration();
            bootConfig.MicroserviceName = "TestService";
            bootConfig.AddFeatures(bootConfig.FrameworkModules, MutableBootConfiguration.KernelAssembly);
            bootConfig.AddFeatures(bootConfig.FrameworkModules, this.GetType().Assembly);

            //-- act

            bootConfig.Validate();

            //-- assert

            bootConfig.FrameworkModules.Select(m => m.RuntimeAssembly).Should().Equal(
                MutableBootConfiguration.KernelAssembly,
                this.GetType().Assembly);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void Validate_AssemblyMapAssigned_NoChange()
        {
            //-- arrange

            var bootConfig = new MutableBootConfiguration();
            bootConfig.MicroserviceName = "TestService";

            var customAssemblyMap = new AssemblyLocationMap();
            customAssemblyMap.AddDirectory("dir1");
            customAssemblyMap.AddDirectory("dir2");

            bootConfig.AssemblyLocationMap = customAssemblyMap;

            //-- act

            bootConfig.Validate();

            //-- assert

            bootConfig.AssemblyLocationMap.Should().NotBeNull();
            bootConfig.AssemblyLocationMap.Should().BeSameAs(customAssemblyMap);
            bootConfig.AssemblyLocationMap.Directories.Should().Equal("dir1", "dir2");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void Validate_NoAssemblyMap_DefaultMapSupplied()
        {
            //-- arrange

            var bootConfig = new MutableBootConfiguration();
            bootConfig.MicroserviceName = "TestService";

            //-- act

            bootConfig.Validate();

            //-- assert

            bootConfig.AssemblyLocationMap.Should().NotBeNull();
            bootConfig.AssemblyLocationMap.FilePathByAssemblyName.Should().NotBeNull();
            bootConfig.AssemblyLocationMap.FilePathByAssemblyName.Should().BeEmpty();
            bootConfig.AssemblyLocationMap.Directories.Should().NotBeNull();
            bootConfig.AssemblyLocationMap.Directories.Should().Contain(AppDomain.CurrentDomain.BaseDirectory);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Theory]
        [InlineData(typeof(IFeatureLoader), null, true)]
        [InlineData(typeof(FeatureLoader1), null, false)]
        [InlineData(null, "NWheels.Kernel", true)]
        [InlineData(null, "My.Assembly", false)]
        public void ModuleConfiguration_IsKernelAssembly(Type typeFromAssembly, string assemblyName, bool expectedIsKernelModule)
        {
            //-- arrange

            MutableBootConfiguration.ModuleConfiguration module;

            if (typeFromAssembly != null)
            {
                module = new MutableBootConfiguration.ModuleConfiguration(typeFromAssembly.Assembly);
            }
            else
            {
                module = new MutableBootConfiguration.ModuleConfiguration(assemblyName);
            }

            //-- act

            var actualIsKernelModule = module.IsKernelModule;

            //-- assert

            actualIsKernelModule.Should().Be(expectedIsKernelModule);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [FeatureLoader(Name = "F1")]
        public class FeatureLoader1 { } 
        
        [FeatureLoader(Name = "F2")]
        public class FeatureLoader2 { } 
        
        [FeatureLoader(Name = "F3")]
        public class FeatureLoader3 { } 
        
        [FeatureLoader(Name = "F4")]
        public class FeatureLoader4 { } 
        
        [FeatureLoader(Name = "F5")]
        public class FeatureLoader5 { } 

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        public class ComponentA { }
    }
}
