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
        //[Fact]
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

            featureLoadersLog.Messages.Should().Equal(

            );
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class TestModuleLoader : DefaultModuleLoader
        {
            public TestModuleLoader(IBootConfiguration bootConfig) 
                : base(bootConfig)
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
                        return new[] { typeof(CustomizationFeatureOne), typeof(ApplicationFeatureTwo), typeof(ApplicationFeatureThree) };
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

            public virtual void InjectBootComponents(IComponentContainer bootComponents)
            {
                _log = bootComponents.Resolve<TestFeatureLoaderLog>();
                _log.Add(this, nameof(InjectBootComponents));
                OnInjectBootComponents?.Invoke(bootComponents);
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
            
            public Action<IComponentContainer> OnInjectBootComponents { get; set; }
            public Action<IComponentContainerBuilder> OnContributeConfigSections { get; set; }
            public Action<IComponentContainer> OnContributeConfiguration { get; set; }
            public Action<IComponentContainer, IComponentContainerBuilder> OnContributeComponents { get; set; }
            public Action<IComponentContainer, IComponentContainerBuilder> OnContributeAdapterComponents { get; set; }
            public Action<IComponentContainer> OnCompileComponents { get; set; }
            public Action<IComponentContainer, IComponentContainerBuilder> OnContributeCompiledComponents { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        public abstract class TestFeatureLoaderWithPhaseExtension : TestFeatureLoader, IFeatureLoaderWithPhaseExtension
        {
            private TestFeatureLoaderPhaseExtension _phaseExtension;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override void InjectBootComponents(IComponentContainer bootComponents)
            {
                base.InjectBootComponents(bootComponents);
                
                _phaseExtension = CreatePhaseExtension(bootComponents);
                OnInitPhaseExtension?.Invoke(_phaseExtension);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public Action<TestFeatureLoaderPhaseExtension> OnInitPhaseExtension { get; set; }
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

            public Action<IComponentContainer> OnAfterContributeCompiledComponents { get; set; }
            public Action<IComponentContainer> OnBeforeCompileComponents { get; set; }
            public Action<IComponentContainer> OnBeforeContributeAdapterComponents { get; set; }
            public Action<IComponentContainer> OnBeforeContributeCompiledComponents { get; set; }
            public Action<IComponentContainer> OnBeforeContributeComponents { get; set; }
            public Action<IComponentContainer> OnBeforeContributeConfigSections { get; set; }
            public Action<IComponentContainer> OnBeforeContributeConfiguration { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        [DefaultFeatureLoader]
        public class FrameworkFeatureOne : TestFeatureLoader
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [FeatureLoader(Name = "FX2")]
        public class FrameworkFeatureTwo : TestFeatureLoaderWithPhaseExtension
        {
            protected override TestFeatureLoaderPhaseExtension CreatePhaseExtension(IComponentContainer bootComponents)
            {
                return new FrameworkFeatureOnePhaseExtension(this, bootComponents.Resolve<TestFeatureLoaderLog>());
            }
            private class FrameworkFeatureOnePhaseExtension : TestFeatureLoaderPhaseExtension
            {
                public FrameworkFeatureOnePhaseExtension(TestFeatureLoader featureLoader, TestFeatureLoaderLog log) 
                    : base(featureLoader, log)
                {
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [FeatureLoader(Name = "FX3")]
        public class FrameworkFeatureThree : TestFeatureLoader
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DefaultFeatureLoader]
        public class ApplicationFeatureOne : TestFeatureLoader
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [FeatureLoader(Name = "A2")]
        public class ApplicationFeatureTwo : TestFeatureLoaderWithPhaseExtension
        {
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
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DefaultFeatureLoader]
        public class CustomizationFeatureOne : TestFeatureLoader
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [FeatureLoader(Name = "C2")]
        public class CustomizationFeatureTwo : TestFeatureLoaderWithPhaseExtension
        {
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
        }
    }
}