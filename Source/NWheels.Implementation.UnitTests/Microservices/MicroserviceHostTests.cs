using FluentAssertions;
using NWheels.Compilation;
using NWheels.Execution;
using NWheels.Injection;
using NWheels.Injection.Adapters.Autofac;
using NWheels.Microservices;
using NWheels.Microservices.Mocks;
using System;
using System.Linq;
using System.Collections.Generic;
using Xunit;

namespace NWheels.Implementation.UnitTests.Microservices
{
    public class MicroserviceHostTests : IDisposable
    {
        public MicroserviceHostTests()
        {
            _s_featureLoaderMocks = new Dictionary<Type, FeatureLoaderMock>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Dispose()
        {
            _s_featureLoaderMocks = null;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void CheckMicroserviceConfigOnConfiguring()
        {
            //-- arrange

            var host = new MicroserviceHost(CreateBootConfiguration(), new MicroserviceHostLoggerMock());
            var handler = new AssemblyLoadEventHandler();
            
            host.AssemblyLoad += handler.Handle;

            //-- act

            host.Configure();

            //-- assert

            var config = CreateBootConfiguration();

            handler.ContainerEventArgs.Should().NotBeNull();
            handler.ContainerEventArgs.AssemblyName.Should().Be(config.MicroserviceConfig.InjectionAdapter.Assembly);
            handler.ContainerEventArgs.Destination.Count.Should().Be(1);

            handler.FeatureLoaderEventArgsList.Should().HaveCount(
                1 + config.MicroserviceConfig.FrameworkModules.Length + config.MicroserviceConfig.ApplicationModules.Length);
            handler.FeatureLoaderEventArgsList.Should().ContainSingle(
                x => x.AssemblyName == config.MicroserviceConfig.ApplicationModules[0].Assembly);
            handler.FeatureLoaderEventArgsList.Should().ContainSingle(
                x => x.AssemblyName == config.MicroserviceConfig.ApplicationModules[1].Assembly);
            handler.FeatureLoaderEventArgsList.Should().ContainSingle(
                x => x.AssemblyName == config.MicroserviceConfig.FrameworkModules[0].Assembly);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void CheckDefaultFeatureLoadersDiscoveredOnConfiguring()
        {
            //-- arrange

            var logger = new MicroserviceHostLoggerMock();
            var host = new MicroserviceHost(CreateBootConfiguration(), logger);
            var handler = new AssemblyLoadEventHandler();

            host.AssemblyLoad += handler.Handle;

            //-- act

            host.Configure();

            //-- assert

            var logs = logger.TakeLog();
            logs.Should().Contain(new string[] {
                "FoundFeatureLoaderComponent(component=FirstFeatureLoader)",
                "FoundFeatureLoaderComponent(component=SecondFeatureLoader)",

                "FoundFeatureLoaderComponent(component=SeventhFeatureLoader)",
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void CheckNamedByAttributeFeatureLoadersDiscoveredOnConfiguring()
        {
            //-- arrange

            var logger = new MicroserviceHostLoggerMock();
            var host = new MicroserviceHost(CreateBootConfiguration(), logger);
            var handler = new AssemblyLoadEventHandler();

            host.AssemblyLoad += handler.Handle;

            //-- act

            host.Configure();

            //-- assert

            var logs = logger.TakeLog();
            logs.Should().Contain(new string[] {
                "FoundFeatureLoaderComponent(component=NamedFeatureLoader)"
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void CheckNamedByConventionFeatureLoadersDiscoveredOnConfiguring()
        {
            //-- arrange

            var logger = new MicroserviceHostLoggerMock();
            var host = new MicroserviceHost(CreateBootConfiguration(), logger);
            var handler = new AssemblyLoadEventHandler();

            host.AssemblyLoad += handler.Handle;

            //-- act

            host.Configure();

            //-- assert

            var logs = logger.TakeLog();
            logs.Should().Contain(new string[] {
                "FoundFeatureLoaderComponent(component=ThirdFeatureLoader)",

                "FoundFeatureLoaderComponent(component=FifthFeatureLoader)",
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void TryDiscoverUnexistedFeatureLoaderOnConfiguring()
        {
            //-- arrange

            var logger = new MicroserviceHostLoggerMock();
            var config = CreateBootConfiguration();
            config.MicroserviceConfig.ApplicationModules[0].Features[0].Name = "Abracadabra";
            var host = new MicroserviceHost(config, logger);
            var handler = new AssemblyLoadEventHandler();

            host.AssemblyLoad += handler.Handle;

            //-- act

            Action configuring = () => host.Configure();

            //-- assert

            configuring.ShouldThrow<Exception>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void TryDiscoverDuplicatedFeatureLoaderOnConfiguring()
        {
            //-- arrange

            var logger = new MicroserviceHostLoggerMock();
            var config = CreateBootConfiguration();
            var host = new MicroserviceHost(config, logger);
            var handler = new AssemblyLoadEventHandler();

            host.AssemblyLoad += handler.Handle;
            host.AssemblyLoad += (object sender, AssemblyLoadEventArgs e) =>
            {
                if (e.ImplementedInterface == typeof(IFeatureLoader))
                {
                    if (e.AssemblyName == "FirstModuleAssembly")
                    {
                        e.Destination.Add(typeof(DuplicatedFeatureLoader));
                    }
                }
            };

            //-- act

            Action configuring = () => host.Configure();

            //-- assert

            configuring.ShouldThrow<Exception>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void GetCompileRegistredComponentAfterConfiguring()
        {
            //-- arrange

            var logger = new MicroserviceHostLoggerMock();
            var host = new MicroserviceHost(CreateBootConfiguration(), logger);
            var handler = new AssemblyLoadEventHandler();

            host.AssemblyLoad += handler.Handle;

            //-- act

            host.Configure();
            var container = host.GetContainer();
            var component = container.Resolve<ICompileRegistered>();

            //-- assert

            component.Should().NotBeNull();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void CanExecuteFeatureLoadersLifecycle()
        {
            //-- arrange

            var host = new MicroserviceHost(CreateBootConfiguration(), new MicroserviceHostLoggerMock());
            var handler = new AssemblyLoadEventHandler();

            host.AssemblyLoad += handler.Handle;

            var featureLoaderLog = new List<string>();

            AddFeatureLoaderLoggingInterceptor<FirstFeatureLoader>(featureLoaderLog);
            AddFeatureLoaderLoggingInterceptor<SecondFeatureLoader>(featureLoaderLog);

            //-- act

            host.Configure();

            //-- assert

            featureLoaderLog.Should().Equal(
                $"{typeof(FirstFeatureLoader).Name}.{nameof(IFeatureLoader.ContributeConfigSections)}",
                $"{typeof(SecondFeatureLoader).Name}.{nameof(IFeatureLoader.ContributeConfigSections)}",

                $"{typeof(FirstFeatureLoader).Name}.{nameof(IFeatureLoader.ContributeConfiguration)}",
                $"{typeof(SecondFeatureLoader).Name}.{nameof(IFeatureLoader.ContributeConfiguration)}",

                $"{typeof(FirstFeatureLoader).Name}.{nameof(IFeatureLoader.ContributeComponents)}",
                $"{typeof(SecondFeatureLoader).Name}.{nameof(IFeatureLoader.ContributeComponents)}",

                $"{typeof(FirstFeatureLoader).Name}.{nameof(IFeatureLoader.ContributeAdapterComponents)}",
                $"{typeof(SecondFeatureLoader).Name}.{nameof(IFeatureLoader.ContributeAdapterComponents)}",

                $"{typeof(FirstFeatureLoader).Name}.{nameof(IFeatureLoader.CompileComponents)}",
                $"{typeof(SecondFeatureLoader).Name}.{nameof(IFeatureLoader.CompileComponents)}",

                $"{typeof(FirstFeatureLoader).Name}.{nameof(IFeatureLoader.ContributeCompiledComponents)}",
                $"{typeof(SecondFeatureLoader).Name}.{nameof(IFeatureLoader.ContributeCompiledComponents)}"
            );
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void CheckThatInjectionAdapterLoadedFirst()
        {
            //-- arrange

            var logger = new MicroserviceHostLoggerMock();
            var host = new MicroserviceHost(CreateBootConfiguration(), logger);
            var handler = new AssemblyLoadEventHandler();

            host.AssemblyLoad += handler.Handle;

            //-- act

            host.Configure();

            //-- assert

            var logs = logger.TakeLog();
            var firstLoadedComponent = logs.FirstOrDefault(x => x.StartsWith("FoundFeatureLoaderComponent"));
            firstLoadedComponent.Should().Be("FoundFeatureLoaderComponent(component=ComponentContainerBuilder)");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void CheckThatInjectionAdapterModuleFeatureLoadersWereNotLoaded()
        {
            //-- arrange

            var logger = new MicroserviceHostLoggerMock();
            var config = CreateBootConfiguration();
            var host = new MicroserviceHost(config, logger);
            Func<AssemblyLoadEventArgs, bool> checkFunc = e =>
            {
                return
                    (e.AssemblyName == config.MicroserviceConfig.InjectionAdapter.Assembly
                        && e.ImplementedInterface == typeof(IComponentContainerBuilder))
                    || (e.AssemblyName != config.MicroserviceConfig.InjectionAdapter.Assembly
                        && e.ImplementedInterface != typeof(IComponentContainerBuilder));
            };
            var handler = new AssemblyLoadEventHandler(checkFunc);

            host.AssemblyLoad += handler.Handle;

            //-- act

            Action act = () => host.Configure();

            //-- assert

            act.ShouldNotThrow<Exception>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void AdapterInjectionNotImplementedInterfaceExceptionThrown()
        {
            //-- arrange

            var logger = new MicroserviceHostLoggerMock();
            var config = CreateBootConfiguration();
            config.MicroserviceConfig.InjectionAdapter.Assembly = "AdapterInjectionNotImplementedInterface";
            var host = new MicroserviceHost(config, logger);
            var handler = new AssemblyLoadEventHandler();

            host.AssemblyLoad += handler.Handle;
            host.AssemblyLoad += (object sender, AssemblyLoadEventArgs e) =>
            {
                if (e.ImplementedInterface == typeof(IComponentContainerBuilder))
                {
                    if (e.AssemblyName == "AdapterInjectionNotImplementedInterface")
                    {
                        e.Destination.Add(typeof(String));
                    }
                }
            };

            //-- act

            Action act = () => host.Configure();

            //-- assert

            act.ShouldThrow<Exception>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void AdapterInjectionCtorWithoutArgumentExceptionThrown()
        {
            //-- arrange

            var logger = new MicroserviceHostLoggerMock();
            var config = CreateBootConfiguration();
            config.MicroserviceConfig.InjectionAdapter.Assembly = "AdapterInjectionCtorWithoutArgument";
            var host = new MicroserviceHost(config, logger);
            var handler = new AssemblyLoadEventHandler();

            host.AssemblyLoad += handler.Handle;
            host.AssemblyLoad += (object sender, AssemblyLoadEventArgs e) =>
            {
                if (e.ImplementedInterface == typeof(IComponentContainerBuilder))
                {
                    if (e.AssemblyName == "AdapterInjectionCtorWithoutArgument")
                    {
                        e.Destination.Add(typeof(ComponentContainerBuilderCtorWithoutArgument));
                    }
                }
            };

            //-- act

            Action act = () => host.Configure();

            //-- assert

            act.ShouldThrow<Exception>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void KernelModulesDefaultFeatureLoadersLoaded()
        {
            //-- arrange

            var logger = new MicroserviceHostLoggerMock();
            var host = new MicroserviceHost(CreateBootConfiguration(), logger);
            var handler = new AssemblyLoadEventHandler();

            host.AssemblyLoad += handler.Handle;

            //-- act

            host.Configure();

            //-- assert

            var logs = logger.TakeLog();
            logs.Should().Contain(new string[] {
                "FoundFeatureLoaderComponent(component=CompilationFeatureLoader)",
                "FoundFeatureLoaderComponent(component=InvocationSchedulerFeatureLoader)",
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void KernelModulesDefaultFeatureLoadersLoadedFirstAfterInjectionAdapter()
        {
            //-- arrange

            var logger = new MicroserviceHostLoggerMock();
            var host = new MicroserviceHost(CreateBootConfiguration(), logger);
            var handler = new AssemblyLoadEventHandler();

            host.AssemblyLoad += handler.Handle;

            //-- act

            host.Configure();

            //-- assert

            var logs = logger.TakeLog();
            logs.Skip(1).Take(2).OrderBy(x => x).Should().Equal(new string[] {
                "FoundFeatureLoaderComponent(component=CompilationFeatureLoader)",
                "FoundFeatureLoaderComponent(component=InvocationSchedulerFeatureLoader)",
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void NamedKernelModuleLoadedFirstInFrameworkModules()
        {
            //-- arrange

            var logger = new MicroserviceHostLoggerMock();
            var config = CreateBootConfiguration();
            config.MicroserviceConfig.FrameworkModules = new MicroserviceConfig.ModuleConfig[]
            {
                new MicroserviceConfig.ModuleConfig(){
                    Assembly = "NWheels.Implementation",
                    Features = new MicroserviceConfig.ModuleConfig.FeatureConfig[]{
                        new MicroserviceConfig.ModuleConfig.FeatureConfig(){
                            Name = "NamedKernelFeatureLoader"
                        }
                    }
                }
            };
            var host = new MicroserviceHost(config, logger);
            var handler = new AssemblyLoadEventHandler();
            
            host.AssemblyLoad += (object sender, AssemblyLoadEventArgs e) =>
            {
                if (e.ImplementedInterface == typeof(IFeatureLoader))
                {
                    if (e.AssemblyName == "NWheels.Implementation")
                    {
                        e.Destination.Add(typeof(NamedKernelFeatureLoader));
                    }
                }
            };
            host.AssemblyLoad += handler.Handle;

            //-- act

            host.Configure();

            //-- assert

            var logs = logger.TakeLog();
            logs.Skip(1 + 2).First().Should().Be(
                "FoundFeatureLoaderComponent(component=NamedKernelFeatureLoader)");
        }

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
            var mock = new FeatureLoaderMock(typeof(TFeature)) {
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

        [ThreadStatic]
        private static Dictionary<Type, FeatureLoaderMock> _s_featureLoaderMocks;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class AssemblyLoadEventHandler
        {
            Func<AssemblyLoadEventArgs, bool> _checkFunc;

            public AssemblyLoadEventHandler(Func<AssemblyLoadEventArgs, bool> checkFunc)
            {
                _checkFunc = checkFunc;
                FeatureLoaderEventArgsList = new List<AssemblyLoadEventArgs>();
            }

            public AssemblyLoadEventHandler() : this(x => true)
            { }

            public void Handle(object sender, AssemblyLoadEventArgs e)
            {
                if (_checkFunc(e))
                {
                    if (e.ImplementedInterface == typeof(IComponentContainerBuilder))
                    {
                        if(e.AssemblyName == "InjectionAdapter")
                        {
                            e.Destination.Add(typeof(ComponentContainerBuilder));
                        }
                        ContainerEventArgs = e;
                    }
                    if (e.ImplementedInterface == typeof(IFeatureLoader))
                    {
                        if (e.AssemblyName == "NWheels.Implementation")
                        {
                            e.Destination.Add(typeof(CompilationFeatureLoader));
                            e.Destination.Add(typeof(InvocationSchedulerFeatureLoader));
                        }
                        if (e.AssemblyName == "FirstModuleAssembly")
                        {
                            e.Destination.Add(typeof(FirstFeatureLoader));
                            e.Destination.Add(typeof(SecondFeatureLoader));
                            e.Destination.Add(typeof(ThirdFeatureLoader));
                            e.Destination.Add(typeof(ForthFeatureLoader));
                            e.Destination.Add(typeof(NamedFeatureLoader));
                        }
                        if (e.AssemblyName == "SecondModuleAssembly")
                        {
                            e.Destination.Add(typeof(FifthFeatureLoader));
                            e.Destination.Add(typeof(SixthFeatureLoader));
                        }
                        if (e.AssemblyName == "FrameworkModule")
                        {
                            e.Destination.Add(typeof(SeventhFeatureLoader));
                            e.Destination.Add(typeof(EighthFeatureLoader));
                        }

                        FeatureLoaderEventArgsList.Add(e);
                    }
                }
                else
                {
                    throw new Exception("AssemblyLoadEventHandler.Handle check haven't passed.");
                }
            }

            public AssemblyLoadEventArgs ContainerEventArgs { get; private set; }

            public List<AssemblyLoadEventArgs> FeatureLoaderEventArgsList { get; private set; }
        }

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

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public override void ContributeConfiguration(IComponentContainer existingComponents)
            {
                if (_s_featureLoaderMocks.TryGetValue(this.GetType(), out FeatureLoaderMock mock))
                {
                    mock.ContributeConfiguration(existingComponents);
                }
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public override void ContributeComponents(IComponentContainer existingComponents, IComponentContainerBuilder newComponents)
            {
                if (_s_featureLoaderMocks.TryGetValue(this.GetType(), out FeatureLoaderMock mock))
                {
                    mock.ContributeComponents(existingComponents, newComponents);
                }
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public override void ContributeAdapterComponents(IComponentContainer existingComponents, IComponentContainerBuilder newComponents)
            {
                if (_s_featureLoaderMocks.TryGetValue(this.GetType(), out FeatureLoaderMock mock))
                {
                    mock.ContributeAdapterComponents(existingComponents, newComponents);
                }
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public override void CompileComponents(IComponentContainer existingComponents)
            {
                if (_s_featureLoaderMocks.TryGetValue(this.GetType(), out FeatureLoaderMock mock))
                {
                    mock.CompileComponents(existingComponents);
                }
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public override void ContributeCompiledComponents(IComponentContainer existingComponents, IComponentContainerBuilder newComponents)
            {
                if (_s_featureLoaderMocks.TryGetValue(this.GetType(), out FeatureLoaderMock mock))
                {
                    mock.ContributeCompiledComponents(existingComponents, newComponents);
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private interface ICompileRegistered
        { }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class CompileRegistered : ICompileRegistered
        { }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DefaultFeatureLoader]
        private class FirstFeatureLoader : TestFeatureLoaderBase
        {
            public override void CompileComponents(IComponentContainer existingComponents)
            {
                base.CompileComponents(existingComponents);
            }

            public override void ContributeCompiledComponents(IComponentContainer existingComponents, IComponentContainerBuilder newComponents)
            {
                newComponents.RegisterComponentType<CompileRegistered>().ForService<ICompileRegistered>();
                base.ContributeCompiledComponents(existingComponents, newComponents);
            }
        }

        [DefaultFeatureLoader]
        private class SecondFeatureLoader : TestFeatureLoaderBase
        {
        }
        
        private class ThirdFeatureLoader : TestFeatureLoaderBase
        {
        }        

        private class ForthFeatureLoader : TestFeatureLoaderBase
        {
        }

        [FeatureLoader(Name = "NamedLoader")]
        private class NamedFeatureLoader : TestFeatureLoaderBase
        {
        }

        private class FifthFeatureLoader : TestFeatureLoaderBase
        {
        }

        private class SixthFeatureLoader : TestFeatureLoaderBase
        {
        }

        [DefaultFeatureLoader]
        private class SeventhFeatureLoader : TestFeatureLoaderBase
        {
        }

        private class EighthFeatureLoader : TestFeatureLoaderBase
        {
        }

        [FeatureLoader(Name = "ThirdFeatureLoader")]
        private class DuplicatedFeatureLoader : TestFeatureLoaderBase
        {
        }

        [FeatureLoader(Name = "NamedKernelFeatureLoader")]
        private class NamedKernelFeatureLoader : TestFeatureLoaderBase
        {
        }

        public class ComponentContainerBuilderCtorWithoutArgument : IComponentContainerBuilder
        {
            public IComponentRegistrationBuilder RegisterComponentInstance<TComponent>(TComponent componentInstance) where TComponent : class
            {
                throw new NotImplementedException();
            }

            public IComponentInstantiationBuilder RegisterComponentType<TComponent>()
            {
                throw new NotImplementedException();
            }

            public IComponentInstantiationBuilder RegisterComponentType(Type componentType)
            {
                throw new NotImplementedException();
            }
        }
    }
}
