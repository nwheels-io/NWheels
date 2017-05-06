using NWheels.Injection;                                                                                                                                                        
using NWheels.Injection.Adapters.Autofac;
using NWheels.Microservices;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Implementation.UnitTests.Microservices
{
    public class MicroserviceHostTests2
    {
        // Test matrix:
        // -------+--------------------------------+----------------
        // Module | Type        | Default features | Named features
        // -------+-------------+------------------+----------------
        // A      | Framework   | 2                | 0
        // B      | Framework   | 2                | 2
        // C      | Framework   | 0                | 2
        // -------+-------------+------------------+----------------
        // D      | Application | 2                | 0
        // E      | Application | 2                | 2
        // F      | Application | 0                | 2

        //-----------------------------------------------------------------------------------------------------------------------------------------------------


        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private BootConfiguration CreateBootConfiguration()
        {
            return new BootConfiguration()
            {
                ConfigsDirectory = "ConfigsDirectory",
                MicroserviceConfig = new MicroserviceConfig()
                {
                    Name = "MicroserviceName",
                    InjectionAdapter = new MicroserviceConfig.InjectionAdapterElement()
                    {
                        Assembly = "InjectionAdapter"
                    },
                    ApplicationModules = new MicroserviceConfig.ModuleConfig[]
                    {
                        new MicroserviceConfig.ModuleConfig()
                        {
                            Assembly = "FirstModuleAssembly",
                            Features = new MicroserviceConfig.ModuleConfig.FeatureConfig[]
                            {
                                new MicroserviceConfig.ModuleConfig.FeatureConfig()
                                {
                                    Name = "ThirdFeatureLoader"
                                },
                                new MicroserviceConfig.ModuleConfig.FeatureConfig()
                                {
                                    Name = "NamedLoader"
                                }
                            }
                        },
                        new MicroserviceConfig.ModuleConfig()
                        {
                            Assembly = "SecondModuleAssembly",
                            Features = new MicroserviceConfig.ModuleConfig.FeatureConfig[]
                            {
                                new MicroserviceConfig.ModuleConfig.FeatureConfig()
                                {
                                    Name = "FifthFeatureLoader"
                                }
                            }
                        }
                    },
                    FrameworkModules = new MicroserviceConfig.ModuleConfig[]
                    {
                        new MicroserviceConfig.ModuleConfig()
                        {
                            Assembly = "FrameworkModule",
                            Features = new MicroserviceConfig.ModuleConfig.FeatureConfig[]{ }
                        }
                    }
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

        private void AddFeatureLoaderLoggingInterceptor<TFeature>(List<string> log, Action<FeatureLoaderMock> setup = null)
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
                FeatureLoaderEventArgsList = new List<AssemblyLoadEventArgs>();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void Handle(object sender, AssemblyLoadEventArgs e)
            {
                if (e.ImplementedInterface == typeof(IComponentContainerBuilder) || e.ImplementedInterface == typeof(IFeatureLoader))
                {
                    FeatureLoaderEventArgsList.Add(e);

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
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public List<AssemblyLoadEventArgs> FeatureLoaderEventArgsList { get; }
        }
    }
}
