using FluentAssertions;
using NWheels.Injection;
using NWheels.Injection.Adapters.Autofac;
using NWheels.Microservices;
using NWheels.Microservices.Mocks;
using System;
using System.Collections.Generic;
using Xunit;

namespace NWheels.Implementation.UnitTests.Microservices
{
    public class MicroserviceHostTests
    {
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
                config.MicroserviceConfig.FrameworkModules.Length + config.MicroserviceConfig.ApplicationModules.Length);
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

        private class AssemblyLoadEventHandler
        {
            public AssemblyLoadEventHandler()
            {
                FeatureLoaderEventArgsList = new List<AssemblyLoadEventArgs>();
            }

            public void Handle(object sender, AssemblyLoadEventArgs e)
            {
                if (e.ImplementedInterface == typeof(IComponentContainerBuilder))
                {
                    e.Destination.Add(typeof(ComponentContainerBuilder));
                    ContainerEventArgs = e;
                }
                if (e.ImplementedInterface == typeof(IFeatureLoader))
                {
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

            public AssemblyLoadEventArgs ContainerEventArgs { get; private set; }

            public List<AssemblyLoadEventArgs> FeatureLoaderEventArgsList { get; private set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class TestFeatureLoaderBase : FeatureLoaderBase
        {
            public override void ContributeComponents(IComponentContainerBuilder containerBuilder)
            {
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
            public override void CompileComponents(IInternalComponentContainer input)
            {
                base.CompileComponents(input);
            }

            public override void ContributeCompiledComponents(IInternalComponentContainer input, IComponentContainerBuilder output)
            {
                output.Register<ICompileRegistered, CompileRegistered>();
                base.ContributeCompiledComponents(input, output);
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
    }
}
