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
using NWheels.Microservices.Api.Exceptions;

namespace NWheels.Microservices.UnitTests.Runtime
{
    public class MicroserviceHostTests : TestBase.UnitTest
    {
        [Fact]
        public void InitialState_Source()
        {
            //-- arrange
            
            var bootLog = new TestLog();
            
            //-- act
            
            var host = new MicroserviceHostBuilder("Test")
                .UseBootComponents(builder => builder.RegisterComponentType<TestModuleLoader>().ForService<IModuleLoader>())
                .BuildHost();

            //-- assert

            host.CurrentState.Should().Be(MicroserviceState.Source);
            host.GetBootComponents().Should().NotBeNull();
            host.GetModuleComponents().Should().BeNull();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void Dispose_TransitionedToDisposedState()
        {
            //-- arrange
            
            var bootLog = new TestLog();
            var host = new MicroserviceHostBuilder("Test")
                .UseBootComponents(builder => builder.RegisterComponentType<TestModuleLoader>().ForService<IModuleLoader>())
                .BuildHost();
            
            //-- act
            
            host.Dispose();

            //-- assert

            host.CurrentState.Should().Be(MicroserviceState.Disposed);
            host.GetBootComponents().Should().NotBeNull();
            host.GetModuleComponents().Should().BeNull();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void Dispose_BootComponentsDisposed()
        {
            //-- arrange
            
            var bootLog = new TestLog();
            var host = new MicroserviceHostBuilder("Test")
                .UseBootComponents(builder => {
                    builder.RegisterComponentType<TestModuleLoader>().ForService<IModuleLoader>();
                    builder.RegisterComponentType<DisposableBootComponent>().SingleInstance();
                })
                .BuildHost();

            var disposableComponent = host.GetBootComponents().Resolve<DisposableBootComponent>();
            
            //-- act

            var wasDisposed0 = disposableComponent.WasDisposed;
            
            host.Dispose();

            var wasDisposed1 = disposableComponent.WasDisposed;

            //-- assert

            host.CurrentState.Should().Be(MicroserviceState.Disposed);
            host.GetBootComponents().Should().NotBeNull();
            host.GetModuleComponents().Should().BeNull();

            wasDisposed0.Should().BeFalse();
            wasDisposed1.Should().BeTrue();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void DisposeFromSource_MultipleTimes_NothingHappens()
        {
            //-- arrange
            
            var bootLog = new TestLog();
            var host = new MicroserviceHostBuilder("Test")
                .UseBootComponents(builder => builder.RegisterComponentType<TestModuleLoader>().ForService<IModuleLoader>())
                .BuildHost();
            
            //-- act
            
            host.Dispose();
            host.Dispose();
            host.Dispose();

            //-- assert

            host.CurrentState.Should().Be(MicroserviceState.Disposed);
            host.GetBootComponents().Should().NotBeNull();
            host.GetModuleComponents().Should().BeNull();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void Configure_Success_ConfiguredState()
        {
            //-- arrange

            var bootLog = new TestLog();
            var host = new MicroserviceHostBuilder("Test")
                .UseBootComponents(builder => {
                    builder.RegisterComponentType<TestModuleLoader>().ForService<IModuleLoader>();
                    builder.RegisterComponentInstance(bootLog);
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

            host.CurrentStateChanged += (sender, args) => bootLog.Add(host, $"CurrentStateChanged->{host.CurrentState}");

            //-- act

            host.Configure(CancellationToken.None);

            //-- assert

            host.CurrentState.Should().Be(MicroserviceState.Configured);

            bootLog.Messages.Should().Equal(
                #region Expected log messages
                // state change -> Configuring
                "MicroserviceHost:CurrentStateChanged->Configuring",
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
                "CustomizationFeatureThree:ContributeAdapterComponents",
                // state change -> Configured
                "MicroserviceHost:CurrentStateChanged->Configured"
                #endregion
            );
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void Configure_FeatureLoaderFailed_FaultedState()
        {
            //-- arrange

            var rootCauseException = new FailureTestException();
            var bootLog = new TestLog();
            var host = new MicroserviceHostBuilder("Test")
                .UseBootComponents(builder => {
                    builder.RegisterComponentType<TestModuleLoader>().ForService<IModuleLoader>();
                    builder.RegisterComponentInstance(bootLog);
                    FeatureWillDo<ApplicationFeatureOne>(builder, feature => feature.OnContributeComponents += (cc, cb) => throw rootCauseException);
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

            host.CurrentStateChanged += (sender, args) => bootLog.Add(host, $"CurrentStateChanged->{host.CurrentState}");

            //-- act

            Action act = () => {
                host.Configure(CancellationToken.None);
            };

            var hostException = act.ShouldThrow<MicroserviceHostException>().Which;
            var featureException = hostException.InnerException as MicroserviceHostException;

            //-- assert

            host.CurrentState.Should().Be(MicroserviceState.Faulted);
            hostException.Reason.Should().Be(nameof(MicroserviceHostException.MicroserviceFaulted));
            hostException.InnerException.Should().BeOfType<MicroserviceHostException>().Which.Reason.Should().Be(nameof(MicroserviceHostException.FeatureLoaderFailed));
            featureException.FailedClass.Should().Be(typeof(ApplicationFeatureOne));
            featureException.FailedPhase.Should().Be(nameof(IMicroserviceHostLogger.FeaturesContributingComponents));
            featureException.InnerException.Should().BeSameAs(rootCauseException);

            bootLog.Messages.Should().Equal(
                #region Expected log messages
                // state change -> Configuring
                "MicroserviceHost:CurrentStateChanged->Configuring",
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
                "ApplicationFeatureOne:ContributeComponents", // <-- will throw exception
                // state change -> Faulted
                "MicroserviceHost:CurrentStateChanged->Faulted"
                #endregion
            );
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void Compile_Success_CompiledStoppedState()
        {
            //-- arrange

            var bootLog = new TestLog();
            var host = new MicroserviceHostBuilder("Test")
                .UseBootComponents(builder => {
                    builder.RegisterComponentType<TestModuleLoader>().ForService<IModuleLoader>();
                    builder.RegisterComponentInstance(bootLog);
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
                if (host.CurrentState <= MicroserviceState.Configured)
                {
                    bootLog.Clear();
                }
                else
                {
                    bootLog.Add(host, $"CurrentStateChanged:{host.CurrentState}");
                }
            };

            //-- act

            host.Compile(CancellationToken.None);

            //-- assert

            host.CurrentState.Should().Be(MicroserviceState.CompiledStopped);

            bootLog.Messages.Should().Equal(
                #region Expected log messages
                // state change -> Compiling
                "MicroserviceHost:CurrentStateChanged:Compiling",
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
                "CustomizationFeatureTwoPhaseExtension:AfterContributeCompiledComponents",
                // state change -> CompiledStopped
                "MicroserviceHost:CurrentStateChanged:CompiledStopped" 
                #endregion
            );
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void Compile_FeatureLoaderFailed_FaultedState()
        {
            //-- arrange

            var rootCauseException = new FailureTestException();
            var bootLog = new TestLog();
            var host = new MicroserviceHostBuilder("Test")
                .UseBootComponents(builder => {
                    builder.RegisterComponentType<TestModuleLoader>().ForService<IModuleLoader>();
                    builder.RegisterComponentInstance(bootLog);
                    FeatureWillDo<ApplicationFeatureOne>(builder, feature => feature.OnCompileComponents += (cb) => throw rootCauseException);
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
                    bootLog.Clear();
                }
                else
                {
                    bootLog.Add(host, $"CurrentStateChanged->{host.CurrentState}");
                }
            };

            //-- act

            Action act = () => {
                host.Compile(CancellationToken.None);
            };

            var hostException = act.ShouldThrow<MicroserviceHostException>().Which;
            var featureException = hostException.InnerException as MicroserviceHostException;

            //-- assert

            host.CurrentState.Should().Be(MicroserviceState.Faulted);
            hostException.Reason.Should().Be(nameof(MicroserviceHostException.MicroserviceFaulted));
            hostException.InnerException.Should().BeOfType<MicroserviceHostException>().Which.Reason.Should().Be(nameof(MicroserviceHostException.FeatureLoaderFailed));
            featureException.FailedClass.Should().Be(typeof(ApplicationFeatureOne));
            featureException.FailedPhase.Should().Be(nameof(IMicroserviceHostLogger.FeaturesCompilingComponents));
            featureException.InnerException.Should().BeSameAs(rootCauseException);

            bootLog.Messages.Should().Equal(
                #region Expected log messages
                // state change -> Compiling
                "MicroserviceHost:CurrentStateChanged->Compiling",
                // step pre-6: BeforeCompileComponents is invoked on feature loader phase extensions
                // this only applies to feature loaders that have phase extension
                "FrameworkFeatureTwoPhaseExtension:BeforeCompileComponents",
                "ApplicationFeatureTwoPhaseExtension:BeforeCompileComponents",
                "CustomizationFeatureTwoPhaseExtension:BeforeCompileComponents",
                // step 6: CompileComponents is invoked on feature loaders
                "FrameworkFeatureOne:CompileComponents",
                "FrameworkFeatureTwo:CompileComponents",
                "FrameworkFeatureThree:CompileComponents",
                "ApplicationFeatureOne:CompileComponents", // <-- will throw exception
                // state change -> Faulted
                "MicroserviceHost:CurrentStateChanged->Faulted"
                #endregion
            );
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void CompiledStopped_Dispose_DisposedState()
        {
            //-- arrange

            var bootLog = new TestLog();
            var host = new MicroserviceHostBuilder("Test")
                .UseBootComponents(builder => {
                    builder.RegisterComponentType<TestModuleLoader>().ForService<IModuleLoader>();
                    builder.RegisterComponentInstance(bootLog);
                })
                .UseMicroserviceXml(XElement.Parse(@"
                    <microservice>
                        <framework-modules>
                            <module assembly='FxM1' />
                        </framework-modules>
                        <application-modules>
                            <module assembly='AppM1' />
                        </application-modules>
                    </microservice>
                "))
                .BuildHost();

            host.CurrentStateChanged += (sender, args) => {
                bootLog.Add(host, $"CurrentStateChanged->{host.CurrentState}");
            };
            host.Compile(CancellationToken.None);
            bootLog.Clear();

            //-- act

            host.Dispose();

            //-- assert

            host.CurrentState.Should().Be(MicroserviceState.Disposed);
            bootLog.Messages.Should().Equal("MicroserviceHost:CurrentStateChanged->Disposed");

            host.GetBootComponents().Should().NotBeNull();
            host.GetModuleComponents().Should().BeNull();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void CanRegisterLifecycleComponents()
        {
            //-- arrange

            var bootLog = new TestLog();
            var host = (MicroserviceHost)new MicroserviceHostBuilder("Test")
                .UseBootComponents(builder => {
                    builder.RegisterComponentType<TestModuleLoader>().ForService<IModuleLoader>();
                    builder.RegisterComponentInstance(bootLog);

                    FeatureWillContributeComponentInstance<FrameworkFeatureOne, TestLog>(builder, bootLog);
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

        [Fact]
        public void StartFromSource_InCluster_GoesIntoStandbyState()
        {
            //-- arrange

            var bootLog = new TestLog();
            var runLog = new TestLog();
            var host = (MicroserviceHost)new MicroserviceHostBuilder("Test")
                .Configure(bootConfig => {
                    bootConfig.ClusterName = "C1";
                })
                .UseBootComponents(builder => {
                    builder.RegisterComponentType<TestModuleLoader>().ForService<IModuleLoader>();
                    builder.RegisterComponentInstance(bootLog);
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
                .UseBootComponents(builder => {
                    FeatureWillContributeComponentInstance<FrameworkFeatureOne, TestLog>(builder, runLog);
                    FeatureWillContributeLifecycleComponent<FrameworkFeatureOne, FrameworkComponentOne>(builder);
                    FeatureWillContributeLifecycleComponent<FrameworkFeatureTwo, FrameworkComponentTwo>(builder);
                    FeatureWillContributeLifecycleComponent<ApplicationFeatureOne, ApplicationComponentOne>(builder);
                    FeatureWillContributeLifecycleComponent<ApplicationFeatureTwo, ApplicationComponentTwo>(builder);
                    FeatureWillContributeLifecycleComponent<CustomizationFeatureOne, CustomizationComponentOne>(builder);
                    FeatureWillContributeLifecycleComponent<CustomizationFeatureTwo, CustomizationComponentTwo>(builder);
                })
                .BuildHost();

            host.CurrentStateChanged += (sender, args) => runLog.Add(host, $"CurrentStateChanged->{host.CurrentState}");

            //-- act

            host.Start(CancellationToken.None);

            //-- assert

            host.CurrentState.Should().Be(MicroserviceState.Standby);

            runLog.Messages.Should().Equal(
                #region expected log messages
                "MicroserviceHost:CurrentStateChanged->Configuring",
                "MicroserviceHost:CurrentStateChanged->Configured",
                "MicroserviceHost:CurrentStateChanged->Compiling",
                "MicroserviceHost:CurrentStateChanged->CompiledStopped",
                "MicroserviceHost:CurrentStateChanged->Loading",
                // step 0: lifecycle components are instantiated
                "FrameworkComponentOne:.ctor",
                "FrameworkComponentTwo:.ctor",
                "ApplicationComponentOne:.ctor",
                "ApplicationComponentTwo:.ctor",
                "CustomizationComponentOne:.ctor",
                "CustomizationComponentTwo:.ctor",
                // step 1: MicroserviceLoading
                "FrameworkComponentOne:MicroserviceLoading",
                "FrameworkComponentTwo:MicroserviceLoading",
                "ApplicationComponentOne:MicroserviceLoading",
                "ApplicationComponentTwo:MicroserviceLoading",
                "CustomizationComponentOne:MicroserviceLoading",
                "CustomizationComponentTwo:MicroserviceLoading",
                // step 2: Load
                "FrameworkComponentOne:Load",
                "FrameworkComponentTwo:Load",
                "ApplicationComponentOne:Load",
                "ApplicationComponentTwo:Load",
                "CustomizationComponentOne:Load",
                "CustomizationComponentTwo:Load",
                // step 3: MicroserviceLoaded
                "FrameworkComponentOne:MicroserviceLoaded",
                "FrameworkComponentTwo:MicroserviceLoaded",
                "ApplicationComponentOne:MicroserviceLoaded",
                "ApplicationComponentTwo:MicroserviceLoaded",
                "CustomizationComponentOne:MicroserviceLoaded",
                "CustomizationComponentTwo:MicroserviceLoaded",
                // state changed -> Standby
                "MicroserviceHost:CurrentStateChanged->Standby"
                #endregion
            );
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void LoadPhase_ComponentFailed_SuccessfulStepsReverted_FaultedState()
        {
            //-- arrange

            var bootLog = new TestLog();
            var runLog = new TestLog();
            var rootCauseException = new FailureTestException();

            var host = (MicroserviceHost)new MicroserviceHostBuilder("Test")
                .UseBootComponents(builder => {
                    builder.RegisterComponentType<TestModuleLoader>().ForService<IModuleLoader>();
                    builder.RegisterComponentInstance(bootLog);
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
                .UseBootComponents(builder => {
                    FeatureWillContributeComponentInstance<FrameworkFeatureOne, TestLog>(builder, runLog);
                    FeatureWillContributeLifecycleComponent<FrameworkFeatureOne, FrameworkComponentOne>(builder);
                    FeatureWillContributeLifecycleComponent<FrameworkFeatureTwo, FrameworkComponentTwo>(builder);

                    FeatureWillContributeLifecycleComponent<ApplicationFeatureOne, ApplicationComponentOne>(builder);
                    ComponentWillDo<ApplicationComponentOne>(builder, component => component.OnLoad += () => throw rootCauseException);

                    FeatureWillContributeLifecycleComponent<ApplicationFeatureTwo, ApplicationComponentTwo>(builder);
                    FeatureWillContributeLifecycleComponent<CustomizationFeatureOne, CustomizationComponentOne>(builder);
                    FeatureWillContributeLifecycleComponent<CustomizationFeatureTwo, CustomizationComponentTwo>(builder);
                })
                .BuildHost();

            host.CurrentStateChanged += (sender, args) => runLog.Add(host, $"CurrentStateChanged->{host.CurrentState}");

            //-- act

            Action act = () => {
                host.Start(CancellationToken.None);
            };

            var hostException = act.ShouldThrow<MicroserviceHostException>().Which;
            var componentException = hostException.InnerException as MicroserviceHostException;

            //-- assert

            hostException.Reason.Should().Be(nameof(MicroserviceHostException.MicroserviceFaulted));
            hostException.InnerException.Should().BeOfType<MicroserviceHostException>().Which.Reason.Should().Be(nameof(MicroserviceHostException.LifecycleComponentFailed));
            componentException.FailedClass.Should().Be(typeof(ApplicationComponentOne));
            componentException.FailedPhase.Should().Be(nameof(ILifecycleComponent.Load));
            componentException.InnerException.Should().BeSameAs(rootCauseException);

            host.CurrentState.Should().Be(MicroserviceState.Faulted);
            host.GetBootComponents().Should().NotBeNull();
            host.GetModuleComponents().Should().NotBeNull();

            runLog.Messages.Should().Equal(
                #region expected log messages
                "MicroserviceHost:CurrentStateChanged->Configuring",
                "MicroserviceHost:CurrentStateChanged->Configured",
                "MicroserviceHost:CurrentStateChanged->Compiling",
                "MicroserviceHost:CurrentStateChanged->CompiledStopped",
                "MicroserviceHost:CurrentStateChanged->Loading",
                // step 0: lifecycle components are instantiated
                "FrameworkComponentOne:.ctor",
                "FrameworkComponentTwo:.ctor",
                "ApplicationComponentOne:.ctor",
                "ApplicationComponentTwo:.ctor",
                "CustomizationComponentOne:.ctor",
                "CustomizationComponentTwo:.ctor",
                // step 1: MicroserviceLoading
                "FrameworkComponentOne:MicroserviceLoading",
                "FrameworkComponentTwo:MicroserviceLoading",
                "ApplicationComponentOne:MicroserviceLoading",
                "ApplicationComponentTwo:MicroserviceLoading",
                "CustomizationComponentOne:MicroserviceLoading",
                "CustomizationComponentTwo:MicroserviceLoading",
                // step 2: Load
                "FrameworkComponentOne:Load",
                "FrameworkComponentTwo:Load",
                "ApplicationComponentOne:Load", // <-- will throw exception
                // revert step 2: MayUnload
                "FrameworkComponentTwo:MayUnload",
                "FrameworkComponentOne:MayUnload",
                // revert step 1: MicroserviceMaybeUnloaded
                "CustomizationComponentTwo:MicroserviceMaybeUnloaded",
                "CustomizationComponentOne:MicroserviceMaybeUnloaded",
                "ApplicationComponentTwo:MicroserviceMaybeUnloaded",
                "ApplicationComponentOne:MicroserviceMaybeUnloaded",
                "FrameworkComponentTwo:MicroserviceMaybeUnloaded",
                "FrameworkComponentOne:MicroserviceMaybeUnloaded",
                // state changed -> Faulted
                "MicroserviceHost:CurrentStateChanged->Faulted"
                #endregion
            );
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void StartFromSource_NotInCluster_GoesIntoActiveState()
        {
            //-- arrange

            var bootLog = new TestLog();
            var runLog = new TestLog();
            var host = (MicroserviceHost)new MicroserviceHostBuilder("Test")
                .UseBootComponents(builder => {
                    builder.RegisterComponentType<TestModuleLoader>().ForService<IModuleLoader>();
                    builder.RegisterComponentInstance(bootLog);
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
                .UseBootComponents(builder => {
                    FeatureWillContributeComponentInstance<FrameworkFeatureOne, TestLog>(builder, runLog);
                    FeatureWillContributeLifecycleComponent<FrameworkFeatureOne, FrameworkComponentOne>(builder);
                    FeatureWillContributeLifecycleComponent<FrameworkFeatureTwo, FrameworkComponentTwo>(builder);
                    FeatureWillContributeLifecycleComponent<ApplicationFeatureOne, ApplicationComponentOne>(builder);
                    FeatureWillContributeLifecycleComponent<ApplicationFeatureTwo, ApplicationComponentTwo>(builder);
                    FeatureWillContributeLifecycleComponent<CustomizationFeatureOne, CustomizationComponentOne>(builder);
                    FeatureWillContributeLifecycleComponent<CustomizationFeatureTwo, CustomizationComponentTwo>(builder);
                })
                .BuildHost();

            host.CurrentStateChanged += (sender, args) => runLog.Add(host, $"CurrentStateChanged->{host.CurrentState}");

            //-- act

            host.Start(CancellationToken.None);

            //-- assert

            host.CurrentState.Should().Be(MicroserviceState.Active);

            runLog.Messages.Should().Equal(
                #region expected log messages
                "MicroserviceHost:CurrentStateChanged->Configuring",
                "MicroserviceHost:CurrentStateChanged->Configured",
                "MicroserviceHost:CurrentStateChanged->Compiling",
                "MicroserviceHost:CurrentStateChanged->CompiledStopped",
                "MicroserviceHost:CurrentStateChanged->Loading",
                // step 0: lifecycle components are instantiated
                "FrameworkComponentOne:.ctor",
                "FrameworkComponentTwo:.ctor",
                "ApplicationComponentOne:.ctor",
                "ApplicationComponentTwo:.ctor",
                "CustomizationComponentOne:.ctor",
                "CustomizationComponentTwo:.ctor",
                // step 1: MicroserviceLoading
                "FrameworkComponentOne:MicroserviceLoading",
                "FrameworkComponentTwo:MicroserviceLoading",
                "ApplicationComponentOne:MicroserviceLoading",
                "ApplicationComponentTwo:MicroserviceLoading",
                "CustomizationComponentOne:MicroserviceLoading",
                "CustomizationComponentTwo:MicroserviceLoading",
                // step 2: Load
                "FrameworkComponentOne:Load",
                "FrameworkComponentTwo:Load",
                "ApplicationComponentOne:Load",
                "ApplicationComponentTwo:Load",
                "CustomizationComponentOne:Load",
                "CustomizationComponentTwo:Load",
                // step 3: MicroserviceLoaded
                "FrameworkComponentOne:MicroserviceLoaded",
                "FrameworkComponentTwo:MicroserviceLoaded",
                "ApplicationComponentOne:MicroserviceLoaded",
                "ApplicationComponentTwo:MicroserviceLoaded",
                "CustomizationComponentOne:MicroserviceLoaded",
                "CustomizationComponentTwo:MicroserviceLoaded",
                // state changed -> Standby -> Activating
                "MicroserviceHost:CurrentStateChanged->Standby",
                "MicroserviceHost:CurrentStateChanged->Activating",
                // step 4: MicroserviceActivating
                "FrameworkComponentOne:MicroserviceActivating",
                "FrameworkComponentTwo:MicroserviceActivating",
                "ApplicationComponentOne:MicroserviceActivating",
                "ApplicationComponentTwo:MicroserviceActivating",
                "CustomizationComponentOne:MicroserviceActivating",
                "CustomizationComponentTwo:MicroserviceActivating",
                // step 5: Activate
                "FrameworkComponentOne:Activate",
                "FrameworkComponentTwo:Activate",
                "ApplicationComponentOne:Activate",
                "ApplicationComponentTwo:Activate",
                "CustomizationComponentOne:Activate",
                "CustomizationComponentTwo:Activate",
                // step 6: MicroserviceActivated
                "FrameworkComponentOne:MicroserviceActivated",
                "FrameworkComponentTwo:MicroserviceActivated",
                "ApplicationComponentOne:MicroserviceActivated",
                "ApplicationComponentTwo:MicroserviceActivated",
                "CustomizationComponentOne:MicroserviceActivated",
                "CustomizationComponentTwo:MicroserviceActivated",
                // state changed -> Active
                "MicroserviceHost:CurrentStateChanged->Active"
                #endregion
            );
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void ActivatePhase_ComponentFailed_SuccessfulStepsReverted_FaultedState()
        {
            //-- arrange

            var bootLog = new TestLog();
            var runLog = new TestLog();
            var rootCauseException = new FailureTestException();

            var host = (MicroserviceHost)new MicroserviceHostBuilder("Test")
                .UseBootComponents(builder => {
                    builder.RegisterComponentType<TestModuleLoader>().ForService<IModuleLoader>();
                    builder.RegisterComponentInstance(bootLog);
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
                .UseBootComponents(builder => {
                    FeatureWillContributeComponentInstance<FrameworkFeatureOne, TestLog>(builder, runLog);
                    FeatureWillContributeLifecycleComponent<FrameworkFeatureOne, FrameworkComponentOne>(builder);
                    FeatureWillContributeLifecycleComponent<FrameworkFeatureTwo, FrameworkComponentTwo>(builder);

                    FeatureWillContributeLifecycleComponent<ApplicationFeatureOne, ApplicationComponentOne>(builder);
                    ComponentWillDo<ApplicationComponentOne>(builder, component => component.OnActivate += () => throw rootCauseException);

                    FeatureWillContributeLifecycleComponent<ApplicationFeatureTwo, ApplicationComponentTwo>(builder);
                    FeatureWillContributeLifecycleComponent<CustomizationFeatureOne, CustomizationComponentOne>(builder);
                    FeatureWillContributeLifecycleComponent<CustomizationFeatureTwo, CustomizationComponentTwo>(builder);
                })
                .BuildHost();

            host.CurrentStateChanged += (sender, args) => runLog.Add(host, $"CurrentStateChanged->{host.CurrentState}");

            //-- act

            Action act = () => {
                host.Start(CancellationToken.None);
            };

            var hostException = act.ShouldThrow<MicroserviceHostException>().Which;
            var componentException = hostException.InnerException as MicroserviceHostException;

            //-- assert

            hostException.Reason.Should().Be(nameof(MicroserviceHostException.MicroserviceFaulted));
            hostException.InnerException.Should().BeOfType<MicroserviceHostException>().Which.Reason.Should().Be(nameof(MicroserviceHostException.LifecycleComponentFailed));
            componentException.FailedClass.Should().Be(typeof(ApplicationComponentOne));
            componentException.FailedPhase.Should().Be(nameof(ILifecycleComponent.Activate));
            componentException.InnerException.Should().BeSameAs(rootCauseException);

            host.CurrentState.Should().Be(MicroserviceState.Faulted);
            host.GetBootComponents().Should().NotBeNull();
            host.GetModuleComponents().Should().NotBeNull();

            runLog.Messages.Should().Equal(
                #region expected log messages
                "MicroserviceHost:CurrentStateChanged->Configuring",
                "MicroserviceHost:CurrentStateChanged->Configured",
                "MicroserviceHost:CurrentStateChanged->Compiling",
                "MicroserviceHost:CurrentStateChanged->CompiledStopped",
                "MicroserviceHost:CurrentStateChanged->Loading",
                // step 0: lifecycle components are instantiated
                "FrameworkComponentOne:.ctor",
                "FrameworkComponentTwo:.ctor",
                "ApplicationComponentOne:.ctor",
                "ApplicationComponentTwo:.ctor",
                "CustomizationComponentOne:.ctor",
                "CustomizationComponentTwo:.ctor",
                // step 1: MicroserviceLoading
                "FrameworkComponentOne:MicroserviceLoading",
                "FrameworkComponentTwo:MicroserviceLoading",
                "ApplicationComponentOne:MicroserviceLoading",
                "ApplicationComponentTwo:MicroserviceLoading",
                "CustomizationComponentOne:MicroserviceLoading",
                "CustomizationComponentTwo:MicroserviceLoading",
                // step 2: Load
                "FrameworkComponentOne:Load",
                "FrameworkComponentTwo:Load",
                "ApplicationComponentOne:Load",
                "ApplicationComponentTwo:Load",
                "CustomizationComponentOne:Load",
                "CustomizationComponentTwo:Load",
                // step 3: MicroserviceLoaded
                "FrameworkComponentOne:MicroserviceLoaded",
                "FrameworkComponentTwo:MicroserviceLoaded",
                "ApplicationComponentOne:MicroserviceLoaded",
                "ApplicationComponentTwo:MicroserviceLoaded",
                "CustomizationComponentOne:MicroserviceLoaded",
                "CustomizationComponentTwo:MicroserviceLoaded",
                // state changed -> Standby -> Activating
                "MicroserviceHost:CurrentStateChanged->Standby",
                "MicroserviceHost:CurrentStateChanged->Activating",
                // step 4: MicroserviceActivating
                "FrameworkComponentOne:MicroserviceActivating",
                "FrameworkComponentTwo:MicroserviceActivating",
                "ApplicationComponentOne:MicroserviceActivating",
                "ApplicationComponentTwo:MicroserviceActivating",
                "CustomizationComponentOne:MicroserviceActivating",
                "CustomizationComponentTwo:MicroserviceActivating",
                // step 5: Activate
                "FrameworkComponentOne:Activate",
                "FrameworkComponentTwo:Activate",
                "ApplicationComponentOne:Activate", // <-- will throw exception
                // revert step 5: MayDeactivate
                "FrameworkComponentTwo:MayDeactivate",
                "FrameworkComponentOne:MayDeactivate",
                // revert step 4: MicroserviceMaybeDeactivated
                "CustomizationComponentTwo:MicroserviceMaybeDeactivated",
                "CustomizationComponentOne:MicroserviceMaybeDeactivated",
                "ApplicationComponentTwo:MicroserviceMaybeDeactivated",
                "ApplicationComponentOne:MicroserviceMaybeDeactivated",
                "FrameworkComponentTwo:MicroserviceMaybeDeactivated",
                "FrameworkComponentOne:MicroserviceMaybeDeactivated",
                // state changed -> Faulted
                "MicroserviceHost:CurrentStateChanged->Faulted"
                #endregion
            );
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void Stop_FromActive()
        {
            //-- arrange

            var bootLog = new TestLog();
            var runLog = new TestLog();
            var host = (MicroserviceHost)new MicroserviceHostBuilder("Test")
                .UseBootComponents(builder => {
                    builder.RegisterComponentType<TestModuleLoader>().ForService<IModuleLoader>();
                    builder.RegisterComponentInstance(bootLog);
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
                .UseBootComponents(builder => {
                    FeatureWillContributeComponentInstance<FrameworkFeatureOne, TestLog>(builder, runLog);
                    FeatureWillContributeLifecycleComponent<FrameworkFeatureOne, FrameworkComponentOne>(builder);
                    FeatureWillContributeLifecycleComponent<FrameworkFeatureTwo, FrameworkComponentTwo>(builder);
                    FeatureWillContributeLifecycleComponent<ApplicationFeatureOne, ApplicationComponentOne>(builder);
                    FeatureWillContributeLifecycleComponent<ApplicationFeatureTwo, ApplicationComponentTwo>(builder);
                    FeatureWillContributeLifecycleComponent<CustomizationFeatureOne, CustomizationComponentOne>(builder);
                    FeatureWillContributeLifecycleComponent<CustomizationFeatureTwo, CustomizationComponentTwo>(builder);
                })
                .BuildHost();

            host.Start(CancellationToken.None);

            runLog.Clear();
            host.CurrentStateChanged += (sender, args) => runLog.Add(host, $"CurrentStateChanged->{host.CurrentState}");

            //-- act

            var stopped = host.Stop(TimeSpan.FromSeconds(1));

            //-- assert

            host.CurrentState.Should().Be(MicroserviceState.CompiledStopped);

            runLog.Messages.Should().Equal(
                #region expected log messages
                // state changed -> Deactivating
                "MicroserviceHost:CurrentStateChanged->Deactivating",
                // step 1: MicroserviceMaybeDeactivating
                "CustomizationComponentTwo:MicroserviceMaybeDeactivating",
                "CustomizationComponentOne:MicroserviceMaybeDeactivating",
                "ApplicationComponentTwo:MicroserviceMaybeDeactivating",
                "ApplicationComponentOne:MicroserviceMaybeDeactivating",
                "FrameworkComponentTwo:MicroserviceMaybeDeactivating",
                "FrameworkComponentOne:MicroserviceMaybeDeactivating",
                // step 2: MayDeactivate
                "CustomizationComponentTwo:MayDeactivate",
                "CustomizationComponentOne:MayDeactivate",
                "ApplicationComponentTwo:MayDeactivate",
                "ApplicationComponentOne:MayDeactivate",
                "FrameworkComponentTwo:MayDeactivate",
                "FrameworkComponentOne:MayDeactivate",
                // step 3: MicroserviceMaybeDeactivated
                "CustomizationComponentTwo:MicroserviceMaybeDeactivated",
                "CustomizationComponentOne:MicroserviceMaybeDeactivated",
                "ApplicationComponentTwo:MicroserviceMaybeDeactivated",
                "ApplicationComponentOne:MicroserviceMaybeDeactivated",
                "FrameworkComponentTwo:MicroserviceMaybeDeactivated",
                "FrameworkComponentOne:MicroserviceMaybeDeactivated",
                // state changed -> Standby -> Unloading
                "MicroserviceHost:CurrentStateChanged->Standby",
                "MicroserviceHost:CurrentStateChanged->Unloading",
                // step 4: MicroserviceMaybeUnloading
                "CustomizationComponentTwo:MicroserviceMaybeUnloading",
                "CustomizationComponentOne:MicroserviceMaybeUnloading",
                "ApplicationComponentTwo:MicroserviceMaybeUnloading",
                "ApplicationComponentOne:MicroserviceMaybeUnloading",
                "FrameworkComponentTwo:MicroserviceMaybeUnloading",
                "FrameworkComponentOne:MicroserviceMaybeUnloading",
                // step 5: MayUnload
                "CustomizationComponentTwo:MayUnload",
                "CustomizationComponentOne:MayUnload",
                "ApplicationComponentTwo:MayUnload",
                "ApplicationComponentOne:MayUnload",
                "FrameworkComponentTwo:MayUnload",
                "FrameworkComponentOne:MayUnload",
                // step 3: MicroserviceMaybeUnloaded
                "CustomizationComponentTwo:MicroserviceMaybeUnloaded",
                "CustomizationComponentOne:MicroserviceMaybeUnloaded",
                "ApplicationComponentTwo:MicroserviceMaybeUnloaded",
                "ApplicationComponentOne:MicroserviceMaybeUnloaded",
                "FrameworkComponentTwo:MicroserviceMaybeUnloaded",
                "FrameworkComponentOne:MicroserviceMaybeUnloaded",
                // state changed -> CompiledStopped
                "MicroserviceHost:CurrentStateChanged->CompiledStopped"
                #endregion
            );
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void Stop_FromStandby()
        {
            //-- arrange

            var bootLog = new TestLog();
            var runLog = new TestLog();
            var host = (MicroserviceHost)new MicroserviceHostBuilder("Test")
                .Configure(bootConfig => bootConfig.ClusterName = "C1")
                .UseBootComponents(builder => {
                    builder.RegisterComponentType<TestModuleLoader>().ForService<IModuleLoader>();
                    builder.RegisterComponentInstance(bootLog);
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
                .UseBootComponents(builder => {
                    FeatureWillContributeComponentInstance<FrameworkFeatureOne, TestLog>(builder, runLog);
                    FeatureWillContributeLifecycleComponent<FrameworkFeatureOne, FrameworkComponentOne>(builder);
                    FeatureWillContributeLifecycleComponent<FrameworkFeatureTwo, FrameworkComponentTwo>(builder);
                    FeatureWillContributeLifecycleComponent<ApplicationFeatureOne, ApplicationComponentOne>(builder);
                    FeatureWillContributeLifecycleComponent<ApplicationFeatureTwo, ApplicationComponentTwo>(builder);
                    FeatureWillContributeLifecycleComponent<CustomizationFeatureOne, CustomizationComponentOne>(builder);
                    FeatureWillContributeLifecycleComponent<CustomizationFeatureTwo, CustomizationComponentTwo>(builder);
                })
                .BuildHost();

            host.Start(CancellationToken.None);
            host.CurrentState.Should().Be(MicroserviceState.Standby);

            runLog.Clear();
            host.CurrentStateChanged += (sender, args) => runLog.Add(host, $"CurrentStateChanged->{host.CurrentState}");

            //-- act

            var stopped = host.Stop(TimeSpan.FromSeconds(1));

            //-- assert

            host.CurrentState.Should().Be(MicroserviceState.CompiledStopped);

            runLog.Messages.Should().Equal(
                #region expected log messages
                "MicroserviceHost:CurrentStateChanged->Unloading",
                // step 1: MicroserviceMaybeUnloading
                "CustomizationComponentTwo:MicroserviceMaybeUnloading",
                "CustomizationComponentOne:MicroserviceMaybeUnloading",
                "ApplicationComponentTwo:MicroserviceMaybeUnloading",
                "ApplicationComponentOne:MicroserviceMaybeUnloading",
                "FrameworkComponentTwo:MicroserviceMaybeUnloading",
                "FrameworkComponentOne:MicroserviceMaybeUnloading",
                // step 2: MayUnload
                "CustomizationComponentTwo:MayUnload",
                "CustomizationComponentOne:MayUnload",
                "ApplicationComponentTwo:MayUnload",
                "ApplicationComponentOne:MayUnload",
                "FrameworkComponentTwo:MayUnload",
                "FrameworkComponentOne:MayUnload",
                // step 3: MicroserviceMaybeUnloaded
                "CustomizationComponentTwo:MicroserviceMaybeUnloaded",
                "CustomizationComponentOne:MicroserviceMaybeUnloaded",
                "ApplicationComponentTwo:MicroserviceMaybeUnloaded",
                "ApplicationComponentOne:MicroserviceMaybeUnloaded",
                "FrameworkComponentTwo:MicroserviceMaybeUnloaded",
                "FrameworkComponentOne:MicroserviceMaybeUnloaded",
                // state changed -> CompiledStopped
                "MicroserviceHost:CurrentStateChanged->CompiledStopped"
                #endregion
            );
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static void FeatureWillDo<TFeature>(IComponentContainerBuilder builder, Action<TFeature> action)
            where TFeature : TestFeatureLoader
        {
            builder.RegisterComponentInstance(new TestObjectInitializer<TFeature>(action));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static void ComponentWillDo<TComponent>(IComponentContainerBuilder builder, Action<TComponent> action)
            where TComponent : TestLifecycleComponent
        {
            builder.RegisterComponentInstance(new TestObjectInitializer<TComponent>(action));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static void FeatureWillContributeLifecycleComponent<TFeature, TComponent>(IComponentContainerBuilder builder)
            where TFeature : TestFeatureLoader
            where TComponent : TestLifecycleComponent
        {
            builder.RegisterComponentInstance(new TestObjectInitializer<TFeature>(feature => {
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
            builder.RegisterComponentInstance(new TestObjectInitializer<TFeature>(feature => {
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

        public class TestLog
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
            private TestLog _log;

            //-------------------------------------------------------------------------------------------------------------------------------------------------
            
            protected TestFeatureLoader(IComponentContainer bootComponents)
            {
                _log = bootComponents.Resolve<TestLog>();
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
            private readonly TestLog _log;

            //-------------------------------------------------------------------------------------------------------------------------------------------------
            
            public TestFeatureLoaderPhaseExtension(TestFeatureLoader featureLoader, TestLog log)
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

        public delegate void TestObjectInitializer<T>(T loader) where T : class;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class DisposableBootComponent : IDisposable
        {
            public void Dispose()
            {
                WasDisposed = true;
            }
            public bool WasDisposed { get; private set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class FailureTestException : Exception
        {
            public FailureTestException() : base("TEST-FAILURE")
            {
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DefaultFeatureLoader]
        public class FrameworkFeatureOne : TestFeatureLoader
        {
            public FrameworkFeatureOne(IComponentContainer bootComponents) 
                : base(bootComponents)
            {
                foreach (var initializer in bootComponents.ResolveAll<TestObjectInitializer<FrameworkFeatureOne>>())
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
                foreach (var initializer in bootComponents.ResolveAll<TestObjectInitializer<FrameworkFeatureTwo>>())
                {
                    initializer(this);
                }
            }
            protected override TestFeatureLoaderPhaseExtension CreatePhaseExtension(IComponentContainer bootComponents)
            {
                return new FrameworkFeatureTwoPhaseExtension(this, bootComponents.Resolve<TestLog>());
            }
            private class FrameworkFeatureTwoPhaseExtension : TestFeatureLoaderPhaseExtension
            {
                public FrameworkFeatureTwoPhaseExtension(TestFeatureLoader featureLoader, TestLog log) 
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
                foreach (var initializer in bootComponents.ResolveAll<TestObjectInitializer<FrameworkFeatureThree>>())
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
                foreach (var initializer in bootComponents.ResolveAll<TestObjectInitializer<ApplicationFeatureOne>>())
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
                foreach (var initializer in bootComponents.ResolveAll<TestObjectInitializer<ApplicationFeatureTwo>>())
                {
                    initializer(this);
                }
            }
            protected override TestFeatureLoaderPhaseExtension CreatePhaseExtension(IComponentContainer bootComponents)
            {
                return new ApplicationFeatureTwoPhaseExtension(this, bootComponents.Resolve<TestLog>());
            }
            private class ApplicationFeatureTwoPhaseExtension : TestFeatureLoaderPhaseExtension
            {
                public ApplicationFeatureTwoPhaseExtension(TestFeatureLoader featureLoader, TestLog log) 
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
                foreach (var initializer in bootComponents.ResolveAll<TestObjectInitializer<ApplicationFeatureThree>>())
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
                foreach (var initializer in bootComponents.ResolveAll<TestObjectInitializer<CustomizationFeatureOne>>())
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
                foreach (var initializer in bootComponents.ResolveAll<TestObjectInitializer<CustomizationFeatureTwo>>())
                {
                    initializer(this);
                }
            }
            protected override TestFeatureLoaderPhaseExtension CreatePhaseExtension(IComponentContainer bootComponents)
            {
                return new CustomizationFeatureTwoPhaseExtension(this, bootComponents.Resolve<TestLog>());
            }
            private class CustomizationFeatureTwoPhaseExtension : TestFeatureLoaderPhaseExtension
            {
                public CustomizationFeatureTwoPhaseExtension(TestFeatureLoader featureLoader, TestLog log) 
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
                foreach (var initializer in bootComponents.ResolveAll<TestObjectInitializer<CustomizationFeatureThree>>())
                {
                    initializer(this);
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public abstract class TestLifecycleComponent : LifecycleComponentBase
        {
            private readonly TestLog _log;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected TestLifecycleComponent(TestLog log)
            {
                _log = log;
                _log.Add(this, ".ctor");
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override void MicroserviceLoading()
            {
                _log.Add(this, nameof(MicroserviceLoading));
                OnMicroserviceLoading?.Invoke();
            }
            public override void Load()
            {
                _log.Add(this, nameof(Load));
                OnLoad?.Invoke();
            }
            public override void MicroserviceLoaded()
            {
                _log.Add(this, nameof(MicroserviceLoaded));
                OnMicroserviceLoaded?.Invoke();
            }
            public override void MicroserviceActivating()
            {
                _log.Add(this, nameof(MicroserviceActivating));
                OnMicroserviceActivating?.Invoke();
            }
            public override void Activate()
            {
                _log.Add(this, nameof(Activate));
                OnActivate?.Invoke();
            }
            public override void MicroserviceActivated()
            {
                _log.Add(this, nameof(MicroserviceActivated));
                OnMicroserviceActivated?.Invoke();
            }
            public override void MicroserviceMaybeDeactivating()
            {
                _log.Add(this, nameof(MicroserviceMaybeDeactivating));
                OnMicroserviceMaybeDeactivating?.Invoke();
            }
            public override void MayDeactivate()
            {
                _log.Add(this, nameof(MayDeactivate));
                OnMayDeactivate?.Invoke();
            }
            public override void MicroserviceMaybeDeactivated()
            {
                _log.Add(this, nameof(MicroserviceMaybeDeactivated));
                OnMicroserviceMaybeDeactivated?.Invoke();
            }
            public override void MicroserviceMaybeUnloading()
            {
                _log.Add(this, nameof(MicroserviceMaybeUnloading));
                OnMicroserviceMaybeUnloading?.Invoke();
            }
            public override void MayUnload()
            {
                _log.Add(this, nameof(MayUnload));
                OnMayUnload?.Invoke();
            }
            public override void MicroserviceMaybeUnloaded()
            {
                _log.Add(this, nameof(MicroserviceMaybeUnloaded));
                OnMicroserviceMaybeUnloaded?.Invoke();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public event Action OnMicroserviceLoading;
            public event Action OnLoad;
            public event Action OnMicroserviceLoaded;
            public event Action OnMicroserviceActivating;
            public event Action OnActivate;
            public event Action OnMicroserviceActivated;
            public event Action OnMicroserviceMaybeDeactivating;
            public event Action OnMayDeactivate;
            public event Action OnMicroserviceMaybeDeactivated;
            public event Action OnMicroserviceMaybeUnloading;
            public event Action OnMayUnload;
            public event Action OnMicroserviceMaybeUnloaded;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class FrameworkComponentOne : TestLifecycleComponent
        {
            public FrameworkComponentOne(TestLog log, MicroserviceHost host) : base(log)
            {
                foreach (var initializer in host.GetBootComponents().ResolveAll<TestObjectInitializer<FrameworkComponentOne>>())
                {
                    initializer(this);
                }
            }
        }
        public class FrameworkComponentTwo : TestLifecycleComponent
        {
            public FrameworkComponentTwo(TestLog log, MicroserviceHost host) : base(log)
            {
                foreach (var initializer in host.GetBootComponents().ResolveAll<TestObjectInitializer<FrameworkComponentTwo>>())
                {
                    initializer(this);
                }
            }
        }
        public class ApplicationComponentOne : TestLifecycleComponent
        {
            public ApplicationComponentOne(TestLog log, MicroserviceHost host) : base(log)
            {
                foreach (var initializer in host.GetBootComponents().ResolveAll<TestObjectInitializer<ApplicationComponentOne>>())
                {
                    initializer(this);
                }
            }
        }
        public class ApplicationComponentTwo : TestLifecycleComponent
        {
            public ApplicationComponentTwo(TestLog log, MicroserviceHost host) : base(log)
            {
                foreach (var initializer in host.GetBootComponents().ResolveAll<TestObjectInitializer<ApplicationComponentTwo>>())
                {
                    initializer(this);
                }
            }
        }
        public class CustomizationComponentOne : TestLifecycleComponent
        {
            public CustomizationComponentOne(TestLog log, MicroserviceHost host) : base(log)
            {
                foreach (var initializer in host.GetBootComponents().ResolveAll<TestObjectInitializer<CustomizationComponentOne>>())
                {
                    initializer(this);
                }
            }
        }
        public class CustomizationComponentTwo : TestLifecycleComponent
        {
            public CustomizationComponentTwo(TestLog log, MicroserviceHost host) : base(log)
            {
                foreach (var initializer in host.GetBootComponents().ResolveAll<TestObjectInitializer<CustomizationComponentTwo>>())
                {
                    initializer(this);
                }
            }
        }
    }
}