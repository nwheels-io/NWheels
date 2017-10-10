using System;
using System.Linq;
using System.Collections.Generic;
using NWheels.Kernel.Api.Injection;
using NWheels.Kernel.Runtime.Injection;
using NWheels.Microservices.Api;
using NWheels.Microservices.Runtime;
using NWheels.Testability;
using Xunit;
using FluentAssertions;
using System.Threading;
using System.Xml.Linq;

namespace NWheels.Microservices.UnitTests.Runtime
{
    public class MicroserviceHostTests : TestBase.UnitTest
    {
        [Fact]
        public void CanConfigure()
        {
            //-- arrange

            var featureLoadersLog = new TestFeatureLoaderLog();
            var host = new MicroserviceHostBuilder("Test")
                .UseBootComponents(builder => {
                    builder.RegisterComponentType<TestModuleLoader>().ForService<IModuleLoader>();
                    builder.RegisterComponentInstance(featureLoadersLog);
                })
                .UseMicroserviceXml(XElement.Parse(@"
                    <microservice>
                        <framework-modules>
                            <module assembly='FxM1'><feature name='FX2'/><feature name='FX3'/></module>
                        </framework-modules>
                        <application-modules>
                            <module assembly='AppM1'><feature name='A2'/></module>
                            <module assembly='AppM2'><feature name='A3'/></module>
                        </application-modules>
                        <customization-modules>
                            <module assembly='CustM1'><feature name='C2'/><feature name='C3'/></module>
                        </customization-modules>
                    </microservice>
                "))
                .BuildHost();

            //-- act

            host.Configure(CancellationToken.None);

            //-- assert

            host.CurrentState.Should().Be(MicroserviceState.Configured);

            featureLoadersLog.Messages.Should().Equal(
                #region Expected log messages
                // step 1: feature loaders are instantiated
                "FrameworkFeatureOne:.ctor",        // Default from Kernel
                "FrameworkFeatureTwo:.ctor",        // FX2 from FxM1
                "FrameworkFeatureThree:.ctor",      // FX3 from FxM1
                "ApplicationFeatureOne:.ctor",      // Default from AppM1
                "ApplicationFeatureTwo:.ctor",      // A2 from AppM1 
                "ApplicationFeatureThree:.ctor",    // A3 from AppM2
                "CustomizationFeatureOne:.ctor",    // C1 from CustM1
                "CustomizationFeatureTwo:.ctor",    // C2 from CustM1
                "CustomizationFeatureThree:.ctor",  // C3 from CustM1
                // step pre-2: BeforeContributeConfigSections is invoked on feature loader phase extensions
                // this only applies to feature loaders that have phase extension
                "FrameworkFeatureTwoPhaseExtension:BeforeContributeConfigSections",
                "ApplicationFeatureTwoPhaseExtension:BeforeContributeConfigSections",
                "CustomizationFeatureTwoPhaseExtension:BeforeContributeConfigSections",
                // step 2: ContributeConfigSections is invoked on feature loaders
                "FrameworkFeatureOne:ContributeConfigSections",
                "FrameworkFeatureTwo:ContributeConfigSections",
                "FrameworkFeatureThree:ContributeConfigSections",
                "ApplicationFeatureOne:ContributeConfigSections",
                "ApplicationFeatureTwo:ContributeConfigSections",
                "ApplicationFeatureThree:ContributeConfigSections",
                "CustomizationFeatureOne:ContributeConfigSections",
                "CustomizationFeatureTwo:ContributeConfigSections",
                "CustomizationFeatureThree:ContributeConfigSections",
                // step pre-3: BeforeContributeConfiguration is invoked on feature loader phase extensions
                // this only applies to feature loaders that have phase extension
                "FrameworkFeatureTwoPhaseExtension:BeforeContributeConfiguration",
                "ApplicationFeatureTwoPhaseExtension:BeforeContributeConfiguration",
                "CustomizationFeatureTwoPhaseExtension:BeforeContributeConfiguration",
                // step 3: ContributeConfiguration is invoked on feature loaders
                "FrameworkFeatureOne:ContributeConfiguration",
                "FrameworkFeatureTwo:ContributeConfiguration",
                "FrameworkFeatureThree:ContributeConfiguration",
                "ApplicationFeatureOne:ContributeConfiguration",
                "ApplicationFeatureTwo:ContributeConfiguration",
                "ApplicationFeatureThree:ContributeConfiguration",
                "CustomizationFeatureOne:ContributeConfiguration",
                "CustomizationFeatureTwo:ContributeConfiguration",
                "CustomizationFeatureThree:ContributeConfiguration",
                // step pre-4: BeforeContributeComponents is invoked on feature loader phase extensions
                // this only applies to feature loaders that have phase extension
                "FrameworkFeatureTwoPhaseExtension:BeforeContributeComponents",
                "ApplicationFeatureTwoPhaseExtension:BeforeContributeComponents",
                "CustomizationFeatureTwoPhaseExtension:BeforeContributeComponents",
                // step 3: ContributeComponents is invoked on feature loaders
                "FrameworkFeatureOne:ContributeComponents",
                "FrameworkFeatureTwo:ContributeComponents",
                "FrameworkFeatureThree:ContributeComponents",
                "ApplicationFeatureOne:ContributeComponents",
                "ApplicationFeatureTwo:ContributeComponents",
                "ApplicationFeatureThree:ContributeComponents",
                "CustomizationFeatureOne:ContributeComponents",
                "CustomizationFeatureTwo:ContributeComponents",
                "CustomizationFeatureThree:ContributeComponents",
                // step pre-5: BeforeContributeAdapterComponents is invoked on feature loader phase extensions
                // this only applies to feature loaders that have phase extension
                "FrameworkFeatureTwoPhaseExtension:BeforeContributeAdapterComponents",
                "ApplicationFeatureTwoPhaseExtension:BeforeContributeAdapterComponents",
                "CustomizationFeatureTwoPhaseExtension:BeforeContributeAdapterComponents",
                // step 5: ContributeAdapterComponents is invoked on feature loaders
                "FrameworkFeatureOne:ContributeAdapterComponents",
                "FrameworkFeatureTwo:ContributeAdapterComponents",
                "FrameworkFeatureThree:ContributeAdapterComponents",
                "ApplicationFeatureOne:ContributeAdapterComponents",
                "ApplicationFeatureTwo:ContributeAdapterComponents",
                "ApplicationFeatureThree:ContributeAdapterComponents",
                "CustomizationFeatureOne:ContributeAdapterComponents",
                "CustomizationFeatureTwo:ContributeAdapterComponents",
                "CustomizationFeatureThree:ContributeAdapterComponents"
                #endregion
            );
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void CanCompile()
        {
            //-- arrange

            var featureLoadersLog = new TestFeatureLoaderLog();
            var host = new MicroserviceHostBuilder("Test")
                .UseBootComponents(builder => {
                    builder.RegisterComponentType<TestModuleLoader>().ForService<IModuleLoader>();
                    builder.RegisterComponentInstance(featureLoadersLog);
                })
                .UseMicroserviceXml(XElement.Parse(@"
                    <microservice>
                        <framework-modules>
                            <module assembly='FxM1'><feature name='FX2'/><feature name='FX3'/></module>
                        </framework-modules>
                        <application-modules>
                            <module assembly='AppM1'><feature name='A2'/></module>
                            <module assembly='AppM2'><feature name='A3'/></module>
                        </application-modules>
                        <customization-modules>
                            <module assembly='CustM1'><feature name='C2'/><feature name='C3'/></module>
                        </customization-modules>
                    </microservice>
                "))
                .BuildHost();

            host.CurrentStateChanged += (sender, args) => {
                if (host.CurrentState == MicroserviceState.Configured)
                {
                    featureLoadersLog.Clear();
                }
            };

            //-- act

            host.Compile(CancellationToken.None);

            //-- assert

            host.CurrentState.Should().Be(MicroserviceState.CompiledStopped);

            featureLoadersLog.Messages.Should().Equal(
                #region Expected log messages
                // step pre-6: BeforeCompileComponents is invoked on feature loader phase extensions
                // this only applies to feature loaders that have phase extension
                "FrameworkFeatureTwoPhaseExtension:BeforeCompileComponents",
                "ApplicationFeatureTwoPhaseExtension:BeforeCompileComponents",
                "CustomizationFeatureTwoPhaseExtension:BeforeCompileComponents",
                // step 6: CompileComponents is invoked on feature loaders
                "FrameworkFeatureOne:CompileComponents",
                "FrameworkFeatureTwo:CompileComponents",
                "FrameworkFeatureThree:CompileComponents",
                "ApplicationFeatureOne:CompileComponents",
                "ApplicationFeatureTwo:CompileComponents",
                "ApplicationFeatureThree:CompileComponents",
                "CustomizationFeatureOne:CompileComponents",
                "CustomizationFeatureTwo:CompileComponents",
                "CustomizationFeatureThree:CompileComponents",
                // step pre-7: BeforeContributeCompiledComponents is invoked on feature loader phase extensions
                // this only applies to feature loaders that have phase extension
                "FrameworkFeatureTwoPhaseExtension:BeforeContributeCompiledComponents",
                "ApplicationFeatureTwoPhaseExtension:BeforeContributeCompiledComponents",
                "CustomizationFeatureTwoPhaseExtension:BeforeContributeCompiledComponents",
                // step 7: ContributeCompiledComponents is invoked on feature loaders
                "FrameworkFeatureOne:ContributeCompiledComponents",
                "FrameworkFeatureTwo:ContributeCompiledComponents",
                "FrameworkFeatureThree:ContributeCompiledComponents",
                "ApplicationFeatureOne:ContributeCompiledComponents",
                "ApplicationFeatureTwo:ContributeCompiledComponents",
                "ApplicationFeatureThree:ContributeCompiledComponents",
                "CustomizationFeatureOne:ContributeCompiledComponents",
                "CustomizationFeatureTwo:ContributeCompiledComponents",
                "CustomizationFeatureThree:ContributeCompiledComponents",
                // step post-7: AfterContributeCompiledComponents is invoked on feature loader phase extensions
                // this only applies to feature loaders that have phase extension
                "FrameworkFeatureTwoPhaseExtension:AfterContributeCompiledComponents",
                "ApplicationFeatureTwoPhaseExtension:AfterContributeCompiledComponents",
                "CustomizationFeatureTwoPhaseExtension:AfterContributeCompiledComponents"
                #endregion
            );
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void CanRegisterLifecycleComponents()
        {
            //-- arrange

            var testLog = new TestFeatureLoaderLog();
            var host = (MicroserviceHost)new MicroserviceHostBuilder("Test")
                .UseBootComponents(builder => {
                    builder.RegisterComponentType<TestModuleLoader>().ForService<IModuleLoader>();
                    builder.RegisterComponentInstance(testLog);

                    FeatureWillContributeComponentInstance<FrameworkFeatureOne, TestFeatureLoaderLog>(builder, testLog);
                    FeatureWillContributeLifecycleComponent<FrameworkFeatureOne, FrameworkComponentOne>(builder);
                    FeatureWillContributeLifecycleComponent<FrameworkFeatureTwo, FrameworkComponentTwo>(builder);
                    FeatureWillContributeLifecycleComponent<ApplicationFeatureOne, ApplicationComponentOne>(builder);
                    FeatureWillContributeLifecycleComponent<ApplicationFeatureTwo, ApplicationComponentTwo>(builder);
                    FeatureWillContributeLifecycleComponent<CustomizationFeatureOne, CustomizationComponentOne>(builder);
                    FeatureWillContributeLifecycleComponent<CustomizationFeatureTwo, CustomizationComponentTwo>(builder);
                })
                .UseMicroserviceXml(XElement.Parse(@"
                    <microservice>
                        <framework-modules>
                            <module assembly='FxM1'><feature name='FX2'/></module>
                        </framework-modules>
                        <application-modules>
                            <module assembly='AppM1'><feature name='A2'/></module>
                        </application-modules>
                        <customization-modules>
                            <module assembly='CustM1'><feature name='C2'/></module>
                        </customization-modules>
                    </microservice>
                "))
                .BuildHost();

            //-- act

            host.Compile(CancellationToken.None);

            //-- assert

            host.CurrentState.Should().Be(MicroserviceState.CompiledStopped);

            var lifecycleComponents = host.GetModuleComponents().ResolveAll<ILifecycleComponent>();
            var lifecycleComponentTypes = lifecycleComponents.Select(c => c.GetType()).ToArray();

            lifecycleComponentTypes.Should().BeEquivalentTo(new Type[] {
                typeof(FrameworkComponentOne),
                typeof(FrameworkComponentTwo),
                typeof(ApplicationComponentOne),
                typeof(ApplicationComponentTwo),
                typeof(CustomizationComponentOne),
                typeof(CustomizationComponentTwo)
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static void FeatureWillContributeLifecycleComponent<TFeature, TComponent>(IComponentContainerBuilder builder)
            where TFeature : TestFeatureLoader
            where TComponent : TestLifecycleComponent
        {
            builder.RegisterComponentInstance(new FeatureLoaderInitializer<TFeature>(feature => {
                feature.OnContributeComponents += (existingComponents, newComponents) => {
                    newComponents.ContributeLifecycleComponent<TComponent>();
                };
            }));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static void FeatureWillContributeComponentInstance<TFeature, TComponent>(IComponentContainerBuilder builder, TComponent instance)
            where TFeature : TestFeatureLoader
            where TComponent : class
        {
            builder.RegisterComponentInstance(new FeatureLoaderInitializer<TFeature>(feature => {
                feature.OnContributeComponents += (existingComponents, newComponents) => {
                    newComponents.RegisterComponentInstance<TComponent>(instance);
                };
            }));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class TestModuleLoader : DefaultModuleLoader
        {
            public TestModuleLoader(IBootConfiguration bootConfig, IComponentContainer bootComponents) 
                : base(bootConfig, bootComponents)
            {
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override IEnumerable<Type> GetModulePublicTypes(IModuleConfiguration moduleConfig)
            {
                if (moduleConfig.IsKernelModule)
                {
                    return new[] { typeof(FrameworkFeatureOne) };
                }

                switch (moduleConfig.ModuleName)
                {
                    case "FxM1": // module M1
                        return new[] { typeof(FrameworkFeatureTwo), typeof(FrameworkFeatureThree) };
                    case "AppM1": // module M1
                        return new[] { typeof(ApplicationFeatureOne), typeof(ApplicationFeatureTwo) };
                    case "AppM2": // module M1
                        return new[] { typeof(ApplicationFeatureThree) };
                    case "CustM1": // module M1
                        return new[] { typeof(CustomizationFeatureOne), typeof(CustomizationFeatureTwo), typeof(CustomizationFeatureThree) };
                }

                throw new Exception($"Unexpected mock module name: {moduleConfig.ModuleName}");
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class TestFeatureLoaderLog
        {
            private readonly List<string> _messages = new List<string>();
            public void Add(object source, string message)
            {
                _messages.Add($"{source.GetType().Name}:{message}");
            }
            public string[] Take()
            {
                var messageArray = _messages.ToArray();
                _messages.Clear();
                return messageArray;
            }
            public void Clear()
            {
                _messages.Clear();
            }
            public IReadOnlyList<string> Messages => _messages;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        public abstract class TestFeatureLoader : IFeatureLoader
        {
            private TestFeatureLoaderLog _log;

            //-------------------------------------------------------------------------------------------------------------------------------------------------
            
            protected TestFeatureLoader(IComponentContainer bootComponents)
            {
                _log = bootComponents.Resolve<TestFeatureLoaderLog>();
                _log.Add(this, ".ctor");
            }
            public virtual void ContributeConfigSections(IComponentContainerBuilder newComponents)
            {
                _log.Add(this, nameof(ContributeConfigSections));
                OnContributeConfigSections?.Invoke(newComponents);
            }
            public virtual void ContributeConfiguration(IComponentContainer existingComponents)
            {
                _log.Add(this, nameof(ContributeConfiguration));
                OnContributeConfiguration?.Invoke(existingComponents);
            }
            public virtual void ContributeComponents(IComponentContainer existingComponents, IComponentContainerBuilder newComponents)
            {
                _log.Add(this, nameof(ContributeComponents));
                OnContributeComponents?.Invoke(existingComponents, newComponents);
            }
            public virtual void ContributeAdapterComponents(IComponentContainer existingComponents, IComponentContainerBuilder newComponents)
            {
                _log.Add(this, nameof(ContributeAdapterComponents));
                OnContributeAdapterComponents?.Invoke(existingComponents, newComponents);
            }
            public virtual void CompileComponents(IComponentContainer existingComponents)
            {
                _log.Add(this, nameof(CompileComponents));
                OnCompileComponents?.Invoke(existingComponents);
            }
            public virtual void ContributeCompiledComponents(IComponentContainer existingComponents, IComponentContainerBuilder newComponents)
            {
                _log.Add(this, nameof(ContributeCompiledComponents));
                OnContributeCompiledComponents?.Invoke(existingComponents, newComponents);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public event Action<IComponentContainerBuilder> OnContributeConfigSections;
            public event Action<IComponentContainer> OnContributeConfiguration;
            public event Action<IComponentContainer, IComponentContainerBuilder> OnContributeComponents;
            public event Action<IComponentContainer, IComponentContainerBuilder> OnContributeAdapterComponents;
            public event Action<IComponentContainer> OnCompileComponents;
            public event Action<IComponentContainer, IComponentContainerBuilder> OnContributeCompiledComponents;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        public abstract class TestFeatureLoaderWithPhaseExtension : TestFeatureLoader, IFeatureLoaderWithPhaseExtension
        {
            private readonly TestFeatureLoaderPhaseExtension _phaseExtension;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected TestFeatureLoaderWithPhaseExtension(IComponentContainer bootComponents)
                : base(bootComponents)
            {
                _phaseExtension = CreatePhaseExtension(bootComponents);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public IFeatureLoaderPhaseExtension PhaseExtension => _phaseExtension;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected abstract TestFeatureLoaderPhaseExtension CreatePhaseExtension(IComponentContainer bootComponents);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        public abstract class TestFeatureLoaderPhaseExtension : IFeatureLoaderPhaseExtension
        {
            private readonly TestFeatureLoader _featureLoader;
            private readonly TestFeatureLoaderLog _log;

            //-------------------------------------------------------------------------------------------------------------------------------------------------
            
            public TestFeatureLoaderPhaseExtension(TestFeatureLoader featureLoader, TestFeatureLoaderLog log)
            {
                _featureLoader = featureLoader;
                _log = log;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void AfterContributeCompiledComponents(IComponentContainer components)
            {
                _log.Add(this, nameof(AfterContributeCompiledComponents));
                OnAfterContributeCompiledComponents?.Invoke(components);
            }
            public void BeforeCompileComponents(IComponentContainer components)
            {
                _log.Add(this, nameof(BeforeCompileComponents));
                OnBeforeCompileComponents?.Invoke(components);
            }
            public void BeforeContributeAdapterComponents(IComponentContainer components)
            {
                _log.Add(this, nameof(BeforeContributeAdapterComponents));
                OnBeforeContributeAdapterComponents?.Invoke(components);
            }
            public void BeforeContributeCompiledComponents(IComponentContainer components)
            {
                _log.Add(this, nameof(BeforeContributeCompiledComponents));
                OnBeforeContributeCompiledComponents?.Invoke(components);
            }
            public void BeforeContributeComponents(IComponentContainer components)
            {
                _log.Add(this, nameof(BeforeContributeComponents));
                OnBeforeContributeComponents?.Invoke(components);
            }
            public void BeforeContributeConfigSections(IComponentContainer components)
            {
                _log.Add(this, nameof(BeforeContributeConfigSections));
                OnBeforeContributeConfigSections?.Invoke(components);
            }
            public void BeforeContributeConfiguration(IComponentContainer components)
            {
                _log.Add(this, nameof(BeforeContributeConfiguration));
                OnBeforeContributeConfiguration?.Invoke(components);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public event Action<IComponentContainer> OnAfterContributeCompiledComponents;
            public event Action<IComponentContainer> OnBeforeCompileComponents;
            public event Action<IComponentContainer> OnBeforeContributeAdapterComponents;
            public event Action<IComponentContainer> OnBeforeContributeCompiledComponents;
            public event Action<IComponentContainer> OnBeforeContributeComponents;
            public event Action<IComponentContainer> OnBeforeContributeConfigSections;
            public event Action<IComponentContainer> OnBeforeContributeConfiguration;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public delegate void FeatureLoaderInitializer<T>(T loader) where T : TestFeatureLoader;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DefaultFeatureLoader]
        public class FrameworkFeatureOne : TestFeatureLoader
        {
            public FrameworkFeatureOne(IComponentContainer bootComponents) 
                : base(bootComponents)
            {
                foreach (var initializer in bootComponents.ResolveAll<FeatureLoaderInitializer<FrameworkFeatureOne>>())
                {
                    initializer(this);
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [FeatureLoader(Name = "FX2")]
        public class FrameworkFeatureTwo : TestFeatureLoaderWithPhaseExtension
        {
            public FrameworkFeatureTwo(IComponentContainer bootComponents) 
                : base(bootComponents)
            {
                if (bootComponents.TryResolve(out FeatureLoaderInitializer<FrameworkFeatureTwo> initializer))
                {
                    initializer(this);
                }
            }
            protected override TestFeatureLoaderPhaseExtension CreatePhaseExtension(IComponentContainer bootComponents)
            {
                return new FrameworkFeatureTwoPhaseExtension(this, bootComponents.Resolve<TestFeatureLoaderLog>());
            }
            private class FrameworkFeatureTwoPhaseExtension : TestFeatureLoaderPhaseExtension
            {
                public FrameworkFeatureTwoPhaseExtension(TestFeatureLoader featureLoader, TestFeatureLoaderLog log) 
                    : base(featureLoader, log)
                {
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [FeatureLoader(Name = "FX3")]
        public class FrameworkFeatureThree : TestFeatureLoader
        {
            public FrameworkFeatureThree(IComponentContainer bootComponents) 
                : base(bootComponents)
            {
                if (bootComponents.TryResolve(out FeatureLoaderInitializer<FrameworkFeatureThree> initializer))
                {
                    initializer(this);
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DefaultFeatureLoader]
        public class ApplicationFeatureOne : TestFeatureLoader
        {
            public ApplicationFeatureOne(IComponentContainer bootComponents) 
                : base(bootComponents)
            {
                if (bootComponents.TryResolve(out FeatureLoaderInitializer<ApplicationFeatureOne> initializer))
                {
                    initializer(this);
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [FeatureLoader(Name = "A2")]
        public class ApplicationFeatureTwo : TestFeatureLoaderWithPhaseExtension
        {
            public ApplicationFeatureTwo(IComponentContainer bootComponents) 
                : base(bootComponents)
            {
                if (bootComponents.TryResolve(out FeatureLoaderInitializer<ApplicationFeatureTwo> initializer))
                {
                    initializer(this);
                }
            }
            protected override TestFeatureLoaderPhaseExtension CreatePhaseExtension(IComponentContainer bootComponents)
            {
                return new ApplicationFeatureTwoPhaseExtension(this, bootComponents.Resolve<TestFeatureLoaderLog>());
            }
            private class ApplicationFeatureTwoPhaseExtension : TestFeatureLoaderPhaseExtension
            {
                public ApplicationFeatureTwoPhaseExtension(TestFeatureLoader featureLoader, TestFeatureLoaderLog log) 
                    : base(featureLoader, log)
                {
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [FeatureLoader(Name = "A3")]
        public class ApplicationFeatureThree : TestFeatureLoader
        {
            public ApplicationFeatureThree(IComponentContainer bootComponents) 
                : base(bootComponents)
            {
                if (bootComponents.TryResolve(out FeatureLoaderInitializer<ApplicationFeatureThree> initializer))
                {
                    initializer(this);
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DefaultFeatureLoader]
        public class CustomizationFeatureOne : TestFeatureLoader
        {
            public CustomizationFeatureOne(IComponentContainer bootComponents) 
                : base(bootComponents)
            {
                if (bootComponents.TryResolve(out FeatureLoaderInitializer<CustomizationFeatureOne> initializer))
                {
                    initializer(this);
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [FeatureLoader(Name = "C2")]
        public class CustomizationFeatureTwo : TestFeatureLoaderWithPhaseExtension
        {
            public CustomizationFeatureTwo(IComponentContainer bootComponents) 
                : base(bootComponents)
            {
                if (bootComponents.TryResolve(out FeatureLoaderInitializer<CustomizationFeatureTwo> initializer))
                {
                    initializer(this);
                }
            }
            protected override TestFeatureLoaderPhaseExtension CreatePhaseExtension(IComponentContainer bootComponents)
            {
                return new CustomizationFeatureTwoPhaseExtension(this, bootComponents.Resolve<TestFeatureLoaderLog>());
            }
            private class CustomizationFeatureTwoPhaseExtension : TestFeatureLoaderPhaseExtension
            {
                public CustomizationFeatureTwoPhaseExtension(TestFeatureLoader featureLoader, TestFeatureLoaderLog log) 
                    : base(featureLoader, log)
                {
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [FeatureLoader(Name = "C3")]
        public class CustomizationFeatureThree : TestFeatureLoader
        {
            public CustomizationFeatureThree(IComponentContainer bootComponents) 
                : base(bootComponents)
            {
                if (bootComponents.TryResolve(out FeatureLoaderInitializer<CustomizationFeatureThree> initializer))
                {
                    initializer(this);
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public abstract class TestLifecycleComponent : LifecycleComponentBase
        {
            private readonly TestFeatureLoaderLog _log;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected TestLifecycleComponent(TestFeatureLoaderLog log)
            {
                _log = log;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override void MicroserviceLoading()
            {
                _log.Add(this, nameof(MicroserviceLoading));
            }
            public override void Load()
            {
                _log.Add(this, nameof(Load));
            }
            public override void MicroserviceLoaded()
            {
                _log.Add(this, nameof(MicroserviceLoaded));
            }
            public override void MicroserviceActivating()
            {
                _log.Add(this, nameof(MicroserviceActivating));
            }
            public override void Activate()
            {
                _log.Add(this, nameof(Activate));
            }
            public override void MicroserviceActivated()
            {
                _log.Add(this, nameof(MicroserviceActivated));
            }
            public override void MicroserviceMaybeDeactivating()
            {
                _log.Add(this, nameof(MicroserviceMaybeDeactivating));
            }
            public override void MayDeactivate()
            {
                _log.Add(this, nameof(MayDeactivate));
            }
            public override void MicroserviceMaybeDeactivated()
            {
                _log.Add(this, nameof(MicroserviceMaybeDeactivated));
            }
            public override void MicroserviceMaybeUnloading()
            {
                _log.Add(this, nameof(MicroserviceMaybeUnloading));
            }
            public override void MayUnload()
            {
                _log.Add(this, nameof(MayUnload));
            }
            public override void MicroserviceMaybeUnloaded()
            {
                _log.Add(this, nameof(MicroserviceMaybeUnloaded));
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class FrameworkComponentOne : TestLifecycleComponent
        {
            public FrameworkComponentOne(TestFeatureLoaderLog log) : base(log)
            {
            }
        }
        public class FrameworkComponentTwo : TestLifecycleComponent
        {
            public FrameworkComponentTwo(TestFeatureLoaderLog log) : base(log)
            {
            }
        }
        public class ApplicationComponentOne : TestLifecycleComponent
        {
            public ApplicationComponentOne(TestFeatureLoaderLog log) : base(log)
            {
            }
        }
        public class ApplicationComponentTwo : TestLifecycleComponent
        {
            public ApplicationComponentTwo(TestFeatureLoaderLog log) : base(log)
            {
            }
        }
        public class CustomizationComponentOne : TestLifecycleComponent
        {
            public CustomizationComponentOne(TestFeatureLoaderLog log) : base(log)
            {
            }
        }
        public class CustomizationComponentTwo : TestLifecycleComponent
        {
            public CustomizationComponentTwo(TestFeatureLoaderLog log) : base(log)
            {
            }
        }
    }
}