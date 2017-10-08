using FluentAssertions;
using NWheels.Kernel.Api;
using NWheels.Microservices.Api;
using NWheels.Testability;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Xunit;
using NWheels.Microservices.Runtime;
using NWheels.Kernel.Api.Injection;
using NWheels.Microservices.Api.Exceptions;

namespace NWheels.Microservices.UnitTests.Runtime
{
    public class DefaultModuleLoaderTests : TestBase.UnitTest 
    {
        [Fact]
        public void GetBootFeatureLoaders_ModuleNotConfigured_NotLoaded()
        {
            // F{}, A{M2}, C{} -> M2.D1, M2.D2

            //-- arrange

            var bootConfig = MakeBootConfig(
                applicationModules: MakeModuleConfigList(
                    (Module: "M2", Features: null)
                )
            );

            var loaderUnderTest = new BootFeatureTestModuleLoader(bootConfig);

            //-- act

            var featureLoaders = loaderUnderTest.GetBootFeatureLoaders();

            //-- assert

            var featureList = MakeFeatureListString(featureLoaders);
            featureList.Should().Be("M2_D1;M2_D2");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void GetBootFeatureLoaders_MultipleModulesConfigured_LoadedInConfiguredOrder()
        {
            // F{}, A{M2, M1}, C{} -> M2.D1, M2.D2, M1.D1, M1.D2

            //-- arrange

            var bootConfig = MakeBootConfig(
                applicationModules: MakeModuleConfigList(
                    (Module: "M2", Features: null), 
                    (Module: "M1", Features: null)
                )
            );

            var loaderUnderTest = new BootFeatureTestModuleLoader(bootConfig);

            //-- act

            var featureLoaders = loaderUnderTest.GetBootFeatureLoaders();

            //-- assert

            var featureList = MakeFeatureListString(featureLoaders);
            featureList.Should().Be("M2_D1;M2_D2;M1_D1;M1_D2");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void GetBootFeatureLoaders_ModuleConfiguredWithFeatures_DefaultThenNamedFeaturesLoadedInConfiguredOrder()
        {
            // F{}, A{M1{N2, N1}, M2{N1, N2}}, C{} -> M1.D1, M1.D2, M1.N2, M1.N1, M2.D1, M2.D2, M2.N1, M2.N2

            //-- arrange

            var bootConfig = MakeBootConfig(
                applicationModules: MakeModuleConfigList(
                    (Module: "M1", Features: new[] { "N2", "N1" }),
                    (Module: "M2", Features: new[] { "N1", "N2" })
                )
            );

            var loaderUnderTest = new BootFeatureTestModuleLoader(bootConfig);

            //-- act

            var featureLoaders = loaderUnderTest.GetBootFeatureLoaders();

            //-- assert

            var featureList = MakeFeatureListString(featureLoaders);
            featureList.Should().Be("M1_D1;M1_D2;M1_N2;M1_N1;M2_D1;M2_D2;M2_N1;M2_N2");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void GetBootFeatureLoaders_NamedFeatureListedTwice_Throw()
        {
            // F{}, A{M1{N1, N1}}, C{} -> X

            //-- arrange

            var bootConfig = MakeBootConfig(
                applicationModules: MakeModuleConfigList(
                    (Module: "M1", Features: new[] { "N1", "N1" })
                )
            );

            var loaderUnderTest = new BootFeatureTestModuleLoader(bootConfig);

            //-- act

            Action act = () => loaderUnderTest.GetBootFeatureLoaders();

            //-- assert

            var exception = act.ShouldThrow<ModuleLoaderException>().Which;

            exception.Reason.Should().Be(nameof(ModuleLoaderException.DuplicateNamedFeature));
            exception.ModuleName.Should().Be("M1");
            exception.FeatureName.Should().Be("N1");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void GetBootFeatureLoaders_NamedFeatureDoesNotExist_Throw()
        {
            // F{}, A{M1{ZZZ}}, C{} -> X

            //-- arrange

            var bootConfig = MakeBootConfig(
                applicationModules: MakeModuleConfigList(
                    (Module: "M1", Features: new[] { "ZZZ" })
                )
            );

            var loaderUnderTest = new BootFeatureTestModuleLoader(bootConfig);

            //-- act

            Action act = () => loaderUnderTest.GetBootFeatureLoaders();

            //-- assert

            var exception = act.ShouldThrow<ModuleLoaderException>().Which;

            exception.Reason.Should().Be(nameof(ModuleLoaderException.NamedFeatureDoesNotExist));
            exception.ModuleName.Should().Be("M1");
            exception.FeatureName.Should().Be("ZZZ");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void GetBootFeatureLoaders_ModuleCollectionLoadOrder_FrameworkApplicationCustomization()
        {
            // F{M3}, A{M2}, C{M1} -> M3.D1, M3.D2, M2.D1, M2.D2, M1.D1, M1.D2

            //-- arrange

            var bootConfig = MakeBootConfig(
                frameworkModules: MakeModuleConfigList(
                    (Module: "M3", Features: null)
                ),
                applicationModules: MakeModuleConfigList(
                    (Module: "M2", Features: null)
                ),
                customizationModules: MakeModuleConfigList(
                    (Module: "M1", Features: null)
                )
            );

            var loaderUnderTest = new BootFeatureTestModuleLoader(bootConfig);

            //-- act

            var featureLoaders = loaderUnderTest.GetBootFeatureLoaders();

            //-- assert

            var featureList = MakeFeatureListString(featureLoaders);
            featureList.Should().Be("M3_D1;M3_D2;M2_D1;M2_D2;M1_D1;M1_D2");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private IBootConfiguration MakeBootConfig(
            MutableBootConfiguration.ModuleConfiguration[] frameworkModules = null,
            MutableBootConfiguration.ModuleConfiguration[] applicationModules = null,
            MutableBootConfiguration.ModuleConfiguration[] customizationModules = null)
        {
            var bootConfig = new MutableBootConfiguration();

            if (frameworkModules != null)
            {
                bootConfig.FrameworkModules.AddRange(frameworkModules);
            }

            if (applicationModules != null)
            {
                bootConfig.ApplicationModules.AddRange(applicationModules);
            }

            if (customizationModules != null)
            {
                bootConfig.CustomizationModules.AddRange(customizationModules);
            }

            return bootConfig;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private MutableBootConfiguration.ModuleConfiguration[] MakeModuleConfigList(params (string Module, string[] Features)[] modulesAndFeatures)
        {
            return modulesAndFeatures.Select(m => {
                var assemblyName = (m.Module != "KR" ? m.Module : MutableBootConfiguration.KernelAssemblyName);
                var moduleConfig = new MutableBootConfiguration.ModuleConfiguration(assemblyName);

                if (m.Features != null)
                {
                    moduleConfig.Features.AddRange(m.Features.Select(f => new MutableBootConfiguration.FeatureConfiguration(f)));
                }

                return moduleConfig;
            }).ToArray();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private string MakeFeatureListString(IEnumerable<IFeatureLoader> features)
        {
            return string.Join(
                ";", 
                features.Select(f => f.GetType().Name.Split('+').Last()));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class BootFeatureTestModuleLoader : DefaultModuleLoader
        {
            public BootFeatureTestModuleLoader(IBootConfiguration bootConfig) 
                : base(bootConfig)
            {
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override IEnumerable<Type> GetModulePublicTypes(IModuleConfiguration moduleConfig)
            {
                // each module has two default feature loaders (D1 and D2), and two named feature loaders (N1 and N2)
                // each module has also JustAClassXX that mimics types which are not feature loaders
                // we intentionally mimic different order of types in each module

                if (moduleConfig.IsKernelModule)
                {
                    return new[] { typeof(JustAClassKR), typeof(KR_N2), typeof(KR_D1), typeof(KR_N1), typeof(KR_D2) };
                }

                switch (moduleConfig.ModuleName)
                {
                    case "M1": // module M1
                        return new[] { typeof(JustAClassM1), typeof(M1_D1), typeof(M1_N1), typeof(M1_N2), typeof(M1_D2) };
                    case "M2": // module M2
                        return new[] { typeof(JustAClassM2), typeof(M2_N2), typeof(M2_N1), typeof(M2_D1), typeof(M2_D2) };
                    case "M3": // module M3
                        return new[] { typeof(JustAClassM3), typeof(M3_D1), typeof(M3_D2), typeof(M3_N1), typeof(M3_N2) };
                }

                throw new Exception($"Unexpected mock module name: {moduleConfig.ModuleName}");
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DefaultFeatureLoader]
        public class KR_D1 : FeatureLoaderBase
        {
        }
        [DefaultFeatureLoader]
        public class KR_D2 : FeatureLoaderBase
        {
        }
        [FeatureLoader(Name = "N1")]
        public class KR_N1 : FeatureLoaderBase
        {
        }
        [FeatureLoader(Name = "N2")]
        public class KR_N2 : FeatureLoaderBase
        {
        }
        [DefaultFeatureLoader]
        public class M1_D1 : FeatureLoaderBase
        {
        }
        [DefaultFeatureLoader]
        public class M1_D2 : FeatureLoaderBase
        {
        }
        [FeatureLoader(Name = "N1")]
        public class M1_N1 : FeatureLoaderBase
        {
        }
        [FeatureLoader(Name = "N2")]
        public class M1_N2 : FeatureLoaderBase
        {
        }
        [DefaultFeatureLoader]
        public class M2_D1 : FeatureLoaderBase
        {
        }
        [DefaultFeatureLoader]
        public class M2_D2 : FeatureLoaderBase
        {
        }
        [FeatureLoader(Name = "N1")]
        public class M2_N1 : FeatureLoaderBase
        {
        }
        [FeatureLoader(Name = "N2")]
        public class M2_N2 : FeatureLoaderBase
        {
        }
        [DefaultFeatureLoader]
        public class M3_D1 : FeatureLoaderBase
        {
        }
        [DefaultFeatureLoader]
        public class M3_D2 : FeatureLoaderBase
        {
        }
        [FeatureLoader(Name = "N1")]
        public class M3_N1 : FeatureLoaderBase
        {
        }
        [FeatureLoader(Name = "N2")]
        public class M3_N2 : FeatureLoaderBase
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class JustAClassKR { }
        public class JustAClassM1 { }
        public class JustAClassM2 { }
        public class JustAClassM3 { }
    }

#if false
    public class MicroserviceHostTests2
    {
        public MicroserviceHostTests2()
        {
            _s_featureLoaderMocks = new Dictionary<Type, FeatureLoaderMock>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Dispose()
        {
            _s_featureLoaderMocks = null;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        // Test matrix:
        // -------+-------------+------------------+----------------
        // Module | Type        | Default features | Named features
        // -------+-------------+------------------+----------------
        // KERNEL | Kernel      | 2                | 1
        // -------+-------------+------------------+----------------
        // A      | Framework   | 2                | 0
        // B      | Framework   | 2                | 2
        // C      | Framework   | 0                | 2
        // -------+-------------+------------------+----------------
        // D      | Application | 2                | 0
        // E      | Application | 2                | 2
        // F      | Application | 0                | 2

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void CanLoadInjectionAdapter()
        {
            //-- arrange

            var bootConfig = CreateBootConfiguration(
                frameworkModules: new MicroserviceConfig.ModuleConfig[0], 
                applicationModules: new MicroserviceConfig.ModuleConfig[0]);

            var assemlyLoadHandler = new AssemblyLoadEventHandler();
            var logger = new MicroserviceHostLoggerMock();
            var microserviceUnderTest = new MicroserviceHost(bootConfig, logger);
            microserviceUnderTest.AssemblyLoad += assemlyLoadHandler.Handle;

            //-- act

            microserviceUnderTest.Configure();

            //-- assert

            assemlyLoadHandler.EventList.Count.Should().Be(2);

            assemlyLoadHandler.EventList[0].ImplementedInterface.Should().Be(typeof(IComponentContainerBuilder));
            assemlyLoadHandler.EventList[0].AssemblyName.Should().Be("InjectionAdapter");

            assemlyLoadHandler.EventList[1].ImplementedInterface.Should().Be(typeof(IFeatureLoader));
            assemlyLoadHandler.EventList[1].AssemblyName.Should().Be("NWheels.Implementation");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void EmptyBootConfig_DefaultKernelFeaturesLoaded()
        {
            //-- arrange

            var bootConfig = CreateBootConfiguration(
                frameworkModules: new MicroserviceConfig.ModuleConfig[0],
                applicationModules: new MicroserviceConfig.ModuleConfig[0]);

            var assemlyLoadHandler = new AssemblyLoadEventHandler();
            var logger = new MicroserviceHostLoggerMock();
            var microserviceUnderTest = new MicroserviceHost(bootConfig, logger);
            microserviceUnderTest.AssemblyLoad += assemlyLoadHandler.Handle;

            var featureLog = new List<string>();
            InterceptAllFeatureLoadersLogs(featureLog);

            //-- act

            microserviceUnderTest.Configure();

            //-- assert

            featureLog.Should().Contain($"{typeof(Kernel_FeatureOne).Name}.{nameof(IFeatureLoader.ContributeComponents)}");
            featureLog.Should().Contain($"{typeof(Kernel_FeatureTwo).Name}.{nameof(IFeatureLoader.ContributeComponents)}");
            featureLog.Should().NotContain($"{typeof(Kernel_FeatureThree).Name}.{nameof(IFeatureLoader.ContributeComponents)}");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void FrameworkModule_DefaultKernelFeaturesLoaded()
        {
            //-- arrange

            var bootConfig = CreateBootConfiguration(
                frameworkModules: new MicroserviceConfig.ModuleConfig[0],
                applicationModules: new MicroserviceConfig.ModuleConfig[0]);

            var assemlyLoadHandler = new AssemblyLoadEventHandler();
            var logger = new MicroserviceHostLoggerMock();
            var microserviceUnderTest = new MicroserviceHost(bootConfig, logger);
            microserviceUnderTest.AssemblyLoad += assemlyLoadHandler.Handle;

            var featureLog = new List<string>();
            InterceptAllFeatureLoadersLogs(featureLog);

            //-- act

            microserviceUnderTest.Configure();

            //-- assert

            featureLog.Should().Contain($"{typeof(Kernel_FeatureOne).Name}.{nameof(IFeatureLoader.ContributeComponents)}");
            featureLog.Should().Contain($"{typeof(Kernel_FeatureTwo).Name}.{nameof(IFeatureLoader.ContributeComponents)}");
            featureLog.Should().NotContain($"{typeof(Kernel_FeatureThree).Name}.{nameof(IFeatureLoader.ContributeComponents)}");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private BootConfiguration CreateBootConfiguration(
            MicroserviceConfig.ModuleConfig[] frameworkModules, 
            MicroserviceConfig.ModuleConfig[] applicationModules)
        {
            return new BootConfiguration() {
                ConfigsDirectory = "ConfigsDirectory",
                MicroserviceConfig = new MicroserviceConfig() {
                    Name = "MicroserviceName",
                    InjectionAdapter = new MicroserviceConfig.InjectionAdapterElement() {
                        Assembly = "InjectionAdapter"
                    },
                    ApplicationModules = applicationModules,
                    FrameworkModules = frameworkModules
                },
                EnvironmentConfig = new EnvironmentConfig()
                {
                    Name = "EnvironmentName",
                    Variables = new EnvironmentConfig.VariableConfig[]
                    {
                        new EnvironmentConfig.VariableConfig()
                        {
                            Name = "FirstVariable",
                            Value = "FirstValue"
                        },
                        new EnvironmentConfig.VariableConfig()
                        {
                            Name = "SecondVariable",
                            Value = "SecondValue"
                        },
                        new EnvironmentConfig.VariableConfig()
                        {
                            Name = "ThirdVariable",
                            Value = "ThirdValue"
                        }
                    }
                }
            };
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void AddFeatureLoaderMock<TFeature>(Action<FeatureLoaderMock> setup)
            where TFeature : IFeatureLoader
        {
            var mock = new FeatureLoaderMock(typeof(TFeature));
            setup(mock);
            _s_featureLoaderMocks[typeof(TFeature)] = mock;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void InterceptAllFeatureLoadersLogs(List<string> log, Action<FeatureLoaderMock> setup = null)
        {
            InterceptFeatureLoaderLogs<Kernel_FeatureOne>(log, setup);
            InterceptFeatureLoaderLogs<Kernel_FeatureTwo>(log, setup);
            InterceptFeatureLoaderLogs<Kernel_FeatureThree>(log, setup);
            InterceptFeatureLoaderLogs<Framework_ModuleA_FeatureOne>(log, setup);
            InterceptFeatureLoaderLogs<Framework_ModuleA_FeatureTwo>(log, setup);
            InterceptFeatureLoaderLogs<Framework_ModuleB_FeatureOne>(log, setup);
            InterceptFeatureLoaderLogs<Framework_ModuleB_FeatureTwo>(log, setup);
            InterceptFeatureLoaderLogs<Framework_ModuleB_FeatureThree>(log, setup);
            InterceptFeatureLoaderLogs<Framework_ModuleB_FeatureFour>(log, setup);
            InterceptFeatureLoaderLogs<Framework_ModuleC_FeatureOne>(log, setup);
            InterceptFeatureLoaderLogs<Framework_ModuleC_FeatureTwo>(log, setup);
            InterceptFeatureLoaderLogs<Application_ModuleD_FeatureOne>(log, setup);
            InterceptFeatureLoaderLogs<Application_ModuleD_FeatureTwo>(log, setup);
            InterceptFeatureLoaderLogs<Application_ModuleE_FeatureOne>(log, setup);
            InterceptFeatureLoaderLogs<Application_ModuleE_FeatureTwo>(log, setup);
            InterceptFeatureLoaderLogs<Application_ModuleE_FeatureThree>(log, setup);
            InterceptFeatureLoaderLogs<Application_ModuleE_FeatureFour>(log, setup);
            InterceptFeatureLoaderLogs<Application_ModuleF_FeatureOne>(log, setup);
            InterceptFeatureLoaderLogs<Application_ModuleF_FeatureTwo>(log, setup);
        }
     
        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void InterceptFeatureLoaderLogs<TFeature>(List<string> log, Action<FeatureLoaderMock> setup = null)
            where TFeature : IFeatureLoader
        {
            var mock = new FeatureLoaderMock(typeof(TFeature))
            {
                OnContributeConfigSections = (t, from) => {
                    log.Add($"{typeof(TFeature).Name}.{nameof(IFeatureLoader.ContributeConfigSections)}");
                },
                OnContributeConfiguration = (t, from) => {
                    log.Add($"{typeof(TFeature).Name}.{nameof(IFeatureLoader.ContributeConfiguration)}");
                },
                OnContributeComponents = (t, from, to) => {
                    log.Add($"{typeof(TFeature).Name}.{nameof(IFeatureLoader.ContributeComponents)}");
                },
                OnContributeAdapterComponents = (t, from, to) => {
                    log.Add($"{typeof(TFeature).Name}.{nameof(IFeatureLoader.ContributeAdapterComponents)}");
                },
                OnCompileComponents = (t, from) => {
                    log.Add($"{typeof(TFeature).Name}.{nameof(IFeatureLoader.CompileComponents)}");
                },
                OnContributeCompiledComponents = (t, from, to) => {
                    log.Add($"{typeof(TFeature).Name}.{nameof(IFeatureLoader.ContributeCompiledComponents)}");
                },
            };

            setup?.Invoke(mock);
            _s_featureLoaderMocks[typeof(TFeature)] = mock;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [ThreadStatic]
        private static Dictionary<Type, FeatureLoaderMock> _s_featureLoaderMocks;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class TestFeatureLoaderBase : FeatureLoaderBase
        {
            public override void ContributeConfigSections(IComponentContainerBuilder newComponents)
            {
                if (_s_featureLoaderMocks.TryGetValue(this.GetType(), out FeatureLoaderMock mock))
                {
                    mock.ContributeConfigSections(newComponents);
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override void ContributeConfiguration(IComponentContainer existingComponents)
            {
                if (_s_featureLoaderMocks.TryGetValue(this.GetType(), out FeatureLoaderMock mock))
                {
                    mock.ContributeConfiguration(existingComponents);
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override void ContributeComponents(IComponentContainer existingComponents, IComponentContainerBuilder newComponents)
            {
                if (_s_featureLoaderMocks.TryGetValue(this.GetType(), out FeatureLoaderMock mock))
                {
                    mock.ContributeComponents(existingComponents, newComponents);
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override void ContributeAdapterComponents(IComponentContainer existingComponents, IComponentContainerBuilder newComponents)
            {
                if (_s_featureLoaderMocks.TryGetValue(this.GetType(), out FeatureLoaderMock mock))
                {
                    mock.ContributeAdapterComponents(existingComponents, newComponents);
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override void CompileComponents(IComponentContainer existingComponents)
            {
                if (_s_featureLoaderMocks.TryGetValue(this.GetType(), out FeatureLoaderMock mock))
                {
                    mock.CompileComponents(existingComponents);
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override void ContributeCompiledComponents(IComponentContainer existingComponents, IComponentContainerBuilder newComponents)
            {
                if (_s_featureLoaderMocks.TryGetValue(this.GetType(), out FeatureLoaderMock mock))
                {
                    mock.ContributeCompiledComponents(existingComponents, newComponents);
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class FeatureLoaderMock : FeatureLoaderBase
        {
            private readonly Type _featureLoaderType;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public FeatureLoaderMock(Type featureLoaderType)
            {
                _featureLoaderType = featureLoaderType;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override void ContributeConfigSections(IComponentContainerBuilder newComponents)
            {
                OnContributeConfigSections?.Invoke(_featureLoaderType, newComponents);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override void ContributeConfiguration(IComponentContainer existingComponents)
            {
                OnContributeConfiguration?.Invoke(_featureLoaderType, existingComponents);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override void ContributeComponents(IComponentContainer existingComponents, IComponentContainerBuilder newComponents)
            {
                OnContributeComponents?.Invoke(_featureLoaderType, existingComponents, newComponents);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override void ContributeAdapterComponents(IComponentContainer existingComponents, IComponentContainerBuilder newComponents)
            {
                OnContributeAdapterComponents?.Invoke(_featureLoaderType, existingComponents, newComponents);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override void CompileComponents(IComponentContainer existingComponents)
            {
                OnCompileComponents?.Invoke(_featureLoaderType, existingComponents);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override void ContributeCompiledComponents(IComponentContainer existingComponents, IComponentContainerBuilder newComponents)
            {
                OnContributeCompiledComponents?.Invoke(_featureLoaderType, existingComponents, newComponents);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public Action<Type, IComponentContainerBuilder> OnContributeConfigSections { get; set; }
            public Action<Type, IComponentContainer> OnContributeConfiguration { get; set; }
            public Action<Type, IComponentContainer, IComponentContainerBuilder> OnContributeComponents { get; set; }
            public Action<Type, IComponentContainer, IComponentContainerBuilder> OnContributeAdapterComponents { get; set; }
            public Action<Type, IComponentContainer> OnCompileComponents { get; set; }
            public Action<Type, IComponentContainer, IComponentContainerBuilder> OnContributeCompiledComponents { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DefaultFeatureLoader]
        private class Kernel_FeatureOne : TestFeatureLoaderBase
        {
        }

        [DefaultFeatureLoader]
        private class Kernel_FeatureTwo : TestFeatureLoaderBase
        {
        }

        [FeatureLoader(Name = "K.3")]
        private class Kernel_FeatureThree : TestFeatureLoaderBase
        {
        }

        [DefaultFeatureLoader]
        private class Framework_ModuleA_FeatureOne : TestFeatureLoaderBase
        {
        }

        [DefaultFeatureLoader]
        private class Framework_ModuleA_FeatureTwo : TestFeatureLoaderBase
        {
        }

        [DefaultFeatureLoader]
        private class Framework_ModuleB_FeatureOne : TestFeatureLoaderBase
        {
        }

        [DefaultFeatureLoader]
        private class Framework_ModuleB_FeatureTwo : TestFeatureLoaderBase
        {
        }

        [FeatureLoader(Name = "B.3")]
        private class Framework_ModuleB_FeatureThree : TestFeatureLoaderBase
        {
        }

        [FeatureLoader(Name = "B.4")]
        private class Framework_ModuleB_FeatureFour : TestFeatureLoaderBase
        {
        }

        [FeatureLoader(Name = "C.1")]
        private class Framework_ModuleC_FeatureOne : TestFeatureLoaderBase
        {
        }

        [FeatureLoader(Name = "C.2")]
        private class Framework_ModuleC_FeatureTwo : TestFeatureLoaderBase
        {
        }

        [DefaultFeatureLoader]
        private class Application_ModuleD_FeatureOne : TestFeatureLoaderBase
        {
        }

        [DefaultFeatureLoader]
        private class Application_ModuleD_FeatureTwo : TestFeatureLoaderBase
        {
        }

        [DefaultFeatureLoader]
        private class Application_ModuleE_FeatureOne : TestFeatureLoaderBase
        {
        }

        [DefaultFeatureLoader]
        private class Application_ModuleE_FeatureTwo : TestFeatureLoaderBase
        {
        }

        [FeatureLoader(Name = "E.3")]
        private class Application_ModuleE_FeatureThree : TestFeatureLoaderBase
        {
        }

        [FeatureLoader(Name = "E.4")]
        private class Application_ModuleE_FeatureFour : TestFeatureLoaderBase
        {
        }

        [FeatureLoader(Name = "F.1")]
        private class Application_ModuleF_FeatureOne : TestFeatureLoaderBase
        {
        }

        [FeatureLoader(Name = "F.2")]
        private class Application_ModuleF_FeatureTwo : TestFeatureLoaderBase
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class AssemblyLoadEventHandler
        {
            private readonly Dictionary<string, Type[]> _featureTypesByModuleName = new Dictionary<string, Type[]> {
                ["NWheels.Implementation"] = new[] {
                    typeof(Kernel_FeatureOne),
                    typeof(Kernel_FeatureTwo),
                    typeof(Kernel_FeatureThree),
                },
                ["Module_A"] = new[] {
                    typeof(Framework_ModuleA_FeatureOne),
                    typeof(Framework_ModuleA_FeatureTwo),
                },
                ["Module_B"] = new[] {
                    typeof(Framework_ModuleB_FeatureOne),
                    typeof(Framework_ModuleB_FeatureTwo),
                    typeof(Framework_ModuleB_FeatureThree),
                    typeof(Framework_ModuleB_FeatureFour),
                },
                ["Module_C"] = new[] {
                    typeof(Framework_ModuleC_FeatureOne),
                    typeof(Framework_ModuleC_FeatureTwo),
                },
                ["Module_D"] = new[] {
                    typeof(Application_ModuleD_FeatureOne),
                    typeof(Application_ModuleD_FeatureTwo),
                },
                ["Module_E"] = new[] {
                    typeof(Application_ModuleE_FeatureOne),
                    typeof(Application_ModuleE_FeatureTwo),
                    typeof(Application_ModuleE_FeatureThree),
                    typeof(Application_ModuleE_FeatureFour),
                },
                ["Module_F"] = new[] {
                    typeof(Application_ModuleF_FeatureOne),
                    typeof(Application_ModuleF_FeatureTwo),
                },
            };

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public AssemblyLoadEventHandler()
            {
                EventList = new List<AssemblyLoadEventArgs>();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void Handle(object sender, AssemblyLoadEventArgs e)
            {
                if (e.ImplementedInterface == typeof(IComponentContainerBuilder) || e.ImplementedInterface == typeof(IFeatureLoader))
                {
                    EventList.Add(e);

                    if (e.ImplementedInterface == typeof(IComponentContainerBuilder))
                    {
                        e.Destination.Add(typeof(ComponentContainerBuilder));
                    }

                    if (e.ImplementedInterface == typeof(IFeatureLoader))
                    {
                        var featureLoaderTypes = _featureTypesByModuleName[e.AssemblyName];
                        e.Destination.AddRange(featureLoaderTypes);
                    }
                }
                else
                {
                    Assert.True(false, "expected AssemblyLoadEventArgs.ImplementedInterface");
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public List<AssemblyLoadEventArgs> EventList { get; }
        }
    }
#endif
}
