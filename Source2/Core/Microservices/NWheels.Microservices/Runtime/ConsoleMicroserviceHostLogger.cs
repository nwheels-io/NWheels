using System;
using System.Collections.Generic;
using System.Text;
using NWheels.Kernel.Api.Execution;
using NWheels.Kernel.Api.Extensions;
using NWheels.Kernel.Api.Logging;
using NWheels.Microservices.Api;
using NWheels.Microservices.Api.Exceptions;
using NWheels.Microservices.Runtime.Cli;

namespace NWheels.Microservices.Runtime
{
    public class ConsoleMicroserviceHostLogger : IMicroserviceHostLogger
    {
        private readonly IBootConfiguration _bootConfig;
        private LogLevel _logLevel;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ConsoleMicroserviceHostLogger(IBootConfiguration bootConfig)
        {
            _bootConfig = bootConfig;
            _logLevel = bootConfig.LogLevel;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Activated()
        {
            if (_logLevel <= LogLevel.Info)
            {
                ColorConsole.Log(LogLevel.Info, nameof(Activated));
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity Activating()
        {
            if (_logLevel <= LogLevel.Verbose)
            {
                ColorConsole.Log(LogLevel.Verbose, nameof(Activating));
            }
            return new StubActivity(nameof(Activating));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public OperationCanceledException BatchJobCanceled()
        {
            if (_logLevel <= LogLevel.Warning)
            {
                ColorConsole.Log(LogLevel.Warning, nameof(BatchJobCanceled));
            }
            return new OperationCanceledException("Batch job has been cancelled.");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public OperationCanceledException BatchJobCanceled(OperationCanceledException exception)
        {
            if (_logLevel <= LogLevel.Warning)
            {
                ColorConsole.Log(LogLevel.Warning, $"{nameof(BatchJobCanceled)}: {exception.Message}");
            }
            return exception;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void BatchJobCompleted()
        {
            if (_logLevel <= LogLevel.Info)
            {
                ColorConsole.Log(LogLevel.Info, nameof(BatchJobCompleted));
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MicroserviceHostException BatchJobFailed(Exception error)
        {
            if (_logLevel <= LogLevel.Error)
            {
                ColorConsole.Log(LogLevel.Error, $"{nameof(BatchJobFailed)}: {error}");
            }
            return MicroserviceHostException.BatchJobFailed(error);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Compiled()
        {
            if (_logLevel <= LogLevel.Info)
            {
                ColorConsole.Log(LogLevel.Info, nameof(Compiled));
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity Compiling()
        {
            if (_logLevel <= LogLevel.Verbose)
            {
                ColorConsole.Log(LogLevel.Verbose, nameof(Compiling));
            }
            return new StubActivity(nameof(Compiling));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Configured()
        {
            if (_logLevel <= LogLevel.Info)
            {
                ColorConsole.Log(LogLevel.Info, nameof(Configured));
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity Configuring()
        {
            _logLevel = _bootConfig.LogLevel;

            if (_logLevel <= LogLevel.Verbose)
            {
                ColorConsole.Log(LogLevel.Verbose, nameof(Configuring));
            }
            return new StubActivity(nameof(Configuring));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Deactivated()
        {
            if (_logLevel <= LogLevel.Info)
            {
                ColorConsole.Log(LogLevel.Info, nameof(Deactivated));
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity Deactivating()
        {
            if (_logLevel <= LogLevel.Verbose)
            {
                ColorConsole.Log(LogLevel.Verbose, nameof(Deactivating));
            }
            return new StubActivity(nameof(Deactivating));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void EnteredState(MicroserviceState state)
        {
            if (_logLevel <= LogLevel.Debug)
            {
                ColorConsole.Log(LogLevel.Debug, $"{nameof(EnteredState)}: {state}");
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity ExecutingFeatureLoaderPhaseExtension(Type loaderType, string phase)
        {
            if (_logLevel <= LogLevel.Verbose)
            {
                ColorConsole.Log(LogLevel.Verbose, $"{nameof(ExecutingFeatureLoaderPhaseExtension)}: {loaderType.FriendlyName()}, phase: {phase}");
            }
            return new StubActivity(nameof(ExecutingFeatureLoaderPhaseExtension));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void FailedToActivate(Exception error)
        {
            if (_logLevel <= LogLevel.Critical)
            {
                ColorConsole.Log(LogLevel.Critical, $"{nameof(FailedToActivate)}: {error}");
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void FailedToCompile(Exception error)
        {
            if (_logLevel <= LogLevel.Critical)
            {
                ColorConsole.Log(LogLevel.Critical, $"{nameof(FailedToCompile)}: {error}");
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void FailedToConfigure(Exception error)
        {
            if (_logLevel <= LogLevel.Critical)
            {
                ColorConsole.Log(LogLevel.Critical, $"{nameof(FailedToConfigure)}: {error}");
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void FailedToDeactivate(Exception error)
        {
            if (_logLevel <= LogLevel.Critical)
            {
                ColorConsole.Log(LogLevel.Critical, $"{nameof(FailedToDeactivate)}: {error}");
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void FailedToLoad(Exception error)
        {
            if (_logLevel <= LogLevel.Critical)
            {
                ColorConsole.Log(LogLevel.Critical, $"{nameof(FailedToLoad)}: {error}");
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MicroserviceHostException FailedToLoadLifecycleComponents(Exception error)
        {
            if (_logLevel <= LogLevel.Critical)
            {
                ColorConsole.Log(LogLevel.Critical, $"{nameof(FailedToLoadLifecycleComponents)}: {error}");
            }
            return MicroserviceHostException.LifecycleComponentConstructorFailed(error);
        } 

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void FailedToUnload(Exception error)
        {
            if (_logLevel <= LogLevel.Critical)
            {
                ColorConsole.Log(LogLevel.Critical, $"{nameof(FailedToUnload)}: {error}");
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Faulted()
        {
            if (_logLevel <= LogLevel.Critical)
            {
                ColorConsole.Log(LogLevel.Critical, nameof(Faulted));
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity FeatureCompilingComponents(Type loader)
        {
            if (_logLevel <= LogLevel.Verbose)
            {
                ColorConsole.Log(LogLevel.Verbose, $"{nameof(FeatureCompilingComponents)}: {loader.FriendlyName()}");
            }
            return new StubActivity(nameof(FeatureCompilingComponents));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity FeatureContributingAdapterComponents(Type loader)
        {
            if (_logLevel <= LogLevel.Verbose)
            {
                ColorConsole.Log(LogLevel.Verbose, $"{nameof(FeatureContributingAdapterComponents)}: {loader.FriendlyName()}");
            }
            return new StubActivity(nameof(FeatureContributingAdapterComponents));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity FeatureContributingCompiledComponents(Type loader)
        {
            if (_logLevel <= LogLevel.Verbose)
            {
                ColorConsole.Log(LogLevel.Verbose, $"{nameof(FeatureContributingCompiledComponents)}: {loader.FriendlyName()}");
            }
            return new StubActivity(nameof(FeatureContributingCompiledComponents));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity FeatureContributingComponents(Type loader)
        {
            if (_logLevel <= LogLevel.Verbose)
            {
                ColorConsole.Log(LogLevel.Verbose, $"{nameof(FeatureContributingComponents)}: {loader.FriendlyName()}");
            }
            return new StubActivity(nameof(FeatureContributingComponents));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity FeatureContributingConfigSections(Type loader)
        {
            if (_logLevel <= LogLevel.Verbose)
            {
                ColorConsole.Log(LogLevel.Verbose, $"{nameof(FeatureContributingConfigSections)}: {loader.FriendlyName()}");
            }
            return new StubActivity(nameof(FeatureContributingConfigSections));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity FeatureContributingConfiguration(Type loader)
        {
            if (_logLevel <= LogLevel.Verbose)
            {
                ColorConsole.Log(LogLevel.Verbose, $"{nameof(FeatureContributingConfiguration)}: {loader.FriendlyName()}");
            }
            return new StubActivity(nameof(FeatureContributingConfiguration));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MicroserviceHostException FeatureLoaderFailed(Type loaderType, string phase, Exception error)
        {
            if (_logLevel <= LogLevel.Error)
            {
                ColorConsole.Log(LogLevel.Error, $"{nameof(FeatureLoaderFailed)}: type={loaderType.FriendlyName()}, phase={phase}, {error}");
            }
            return MicroserviceHostException.FeatureLoaderFailed(loaderType, phase, error);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MicroserviceHostException FeatureLoaderPhaseExtensionFailed(Type loaderType, string phase, Exception error)
        {
            if (_logLevel <= LogLevel.Error)
            {
                ColorConsole.Log(LogLevel.Error, $"{nameof(FeatureLoaderPhaseExtensionFailed)}: phase={phase}, {error}");
            }
            return MicroserviceHostException.FeatureLoaderFailed(loaderType, phase, error);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity FeaturesCompilingComponents()
        {
            if (_logLevel <= LogLevel.Verbose)
            {
                ColorConsole.Log(LogLevel.Verbose, nameof(FeaturesCompilingComponents));
            }
            return new StubActivity(nameof(FeaturesCompilingComponents));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity FeaturesContributingAdapterComponents()
        {
            if (_logLevel <= LogLevel.Verbose)
            {
                ColorConsole.Log(LogLevel.Verbose, nameof(FeaturesContributingAdapterComponents));
            }
            return new StubActivity(nameof(FeaturesContributingAdapterComponents));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity FeaturesContributingCompiledComponents()
        {
            if (_logLevel <= LogLevel.Verbose)
            {
                ColorConsole.Log(LogLevel.Verbose, nameof(FeaturesContributingCompiledComponents));
            }
            return new StubActivity(nameof(FeaturesContributingCompiledComponents));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity FeaturesContributingComponents()
        {
            if (_logLevel <= LogLevel.Verbose)
            {
                ColorConsole.Log(LogLevel.Verbose, nameof(FeaturesContributingComponents));
            }
            return new StubActivity(nameof(FeaturesContributingComponents));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity FeaturesContributingConfigSections()
        {
            if (_logLevel <= LogLevel.Verbose)
            {
                ColorConsole.Log(LogLevel.Verbose, nameof(FeaturesContributingConfigSections));
            }
            return new StubActivity(nameof(FeaturesContributingConfigSections));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity FeaturesContributingConfiguration()
        {
            if (_logLevel <= LogLevel.Verbose)
            {
                ColorConsole.Log(LogLevel.Verbose, nameof(FeaturesContributingConfiguration));
            }
            return new StubActivity(nameof(FeaturesContributingConfiguration));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MicroserviceHostException LifecycleComponentFailed(Type componentType, string lifecycleMethod, Exception error)
        {
            if (_logLevel <= LogLevel.Error)
            {
                ColorConsole.Log(LogLevel.Error, $"{nameof(LifecycleComponentFailed)}: {componentType.FriendlyName()}, method={lifecycleMethod}, {error}");
            }
            return MicroserviceHostException.LifecycleComponentFailed(componentType, lifecycleMethod, error);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

#if false

        public IExecutionPathActivity LifecycleComponentsActivate()
        {
            if (_logLevel <= LogLevel.Verbose)
            {
                ColorConsole.Log(LogLevel.Verbose, nameof(LifecycleComponentsActivate));
            }
            return new StubActivity(nameof(LifecycleComponentsActivate));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity LifecycleComponentsLoad()
        {
            if (_logLevel <= LogLevel.Verbose)
            {
                ColorConsole.Log(LogLevel.Verbose, nameof(LifecycleComponentsLoad));
            }
            return new StubActivity(nameof(LifecycleComponentsLoad));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity LifecycleComponentsMayDeactivate()
        {
            if (_logLevel <= LogLevel.Verbose)
            {
                ColorConsole.Log(LogLevel.Verbose, nameof(LifecycleComponentsMayDeactivate));
            }
            return new StubActivity(nameof(LifecycleComponentsMayDeactivate));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity LifecycleComponentsMayUnload()
        {
            if (_logLevel <= LogLevel.Verbose)
            {
                ColorConsole.Log(LogLevel.Verbose, nameof(LifecycleComponentsMayUnload));
            }
            return new StubActivity(nameof(LifecycleComponentsMayUnload));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity LifecycleComponentsMicroserviceActivated()
        {
            if (_logLevel <= LogLevel.Verbose)
            {
                ColorConsole.Log(LogLevel.Verbose, nameof(LifecycleComponentsMicroserviceActivated));
            }
            return new StubActivity(nameof(LifecycleComponentsMicroserviceActivated));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity LifecycleComponentsMicroserviceActivating()
        {
            if (_logLevel <= LogLevel.Verbose)
            {
                ColorConsole.Log(LogLevel.Verbose, nameof(LifecycleComponentsMicroserviceActivating));
            }
            return new StubActivity(nameof(LifecycleComponentsMicroserviceActivating));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity LifecycleComponentsMicroserviceLoaded()
        {
            if (_logLevel <= LogLevel.Verbose)
            {
                ColorConsole.Log(LogLevel.Verbose, nameof(LifecycleComponentsMicroserviceLoaded));
            }
            return new StubActivity(nameof(LifecycleComponentsMicroserviceLoaded));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity LifecycleComponentsMicroserviceLoading()
        {
            if (_logLevel <= LogLevel.Verbose)
            {
                ColorConsole.Log(LogLevel.Verbose, nameof(LifecycleComponentsMicroserviceLoading));
            }
            return new StubActivity(nameof(LifecycleComponentsMicroserviceLoading));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity LifecycleComponentsMicroserviceMaybeDeactivated()
        {
            if (_logLevel <= LogLevel.Verbose)
            {
                ColorConsole.Log(LogLevel.Verbose, nameof(LifecycleComponentsMicroserviceMaybeDeactivated));
            }
            return new StubActivity(nameof(LifecycleComponentsMicroserviceMaybeDeactivated));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity LifecycleComponentsMicroserviceMaybeDeactivating()
        {
            if (_logLevel <= LogLevel.Verbose)
            {
                ColorConsole.Log(LogLevel.Verbose, nameof(LifecycleComponentsMicroserviceMaybeDeactivating));
            }
            return new StubActivity(nameof(LifecycleComponentsMicroserviceMaybeDeactivating));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity LifecycleComponentsMicroserviceMaybeUnloaded()
        {
            if (_logLevel <= LogLevel.Verbose)
            {
                ColorConsole.Log(LogLevel.Verbose, nameof(LifecycleComponentsMicroserviceMaybeUnloaded));
            }
            return new StubActivity(nameof(LifecycleComponentsMicroserviceMaybeUnloaded));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity LifecycleComponentsMicroserviceMaybeUnloading()
        {
            if (_logLevel <= LogLevel.Verbose)
            {
                ColorConsole.Log(LogLevel.Verbose, nameof(LifecycleComponentsMicroserviceMaybeUnloading));
            }
            return new StubActivity(nameof(LifecycleComponentsMicroserviceMaybeUnloading));
        }

#endif
        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Loaded()
        {
            if (_logLevel <= LogLevel.Info)
            {
                ColorConsole.Log(LogLevel.Info, nameof(Loaded));
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void LoadedLifecycleComponent(Type type)
        {
            if (_logLevel <= LogLevel.Verbose)
            {
                ColorConsole.Log(LogLevel.Verbose, $"{nameof(LoadedLifecycleComponent)}: {type.FriendlyName()}");
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity Loading()
        {
            if (_logLevel <= LogLevel.Verbose)
            {
                ColorConsole.Log(LogLevel.Verbose, nameof(Loading));
            }
            return new StubActivity(nameof(Loading));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity LoadingLifecycleComponents()
        {
            if (_logLevel <= LogLevel.Verbose)
            {
                ColorConsole.Log(LogLevel.Verbose, nameof(LoadingLifecycleComponents));
            }
            return new StubActivity(nameof(LoadingLifecycleComponents));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void NoLifecycleComponentsLoaded()
        {
            if (_logLevel <= LogLevel.Warning)
            {
                ColorConsole.Log(LogLevel.Warning, nameof(NoLifecycleComponentsLoaded));
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void RunningInDaemonMode()
        {
            if (_logLevel <= LogLevel.Info)
            {
                ColorConsole.Log(LogLevel.Info, nameof(RunningInDaemonMode));
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void RunningInBatchJobMode()
        {
            if (_logLevel <= LogLevel.Info)
            {
                ColorConsole.Log(LogLevel.Info, nameof(RunningInBatchJobMode));
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void StartingInState(MicroserviceState state)
        {
            if (_logLevel <= LogLevel.Verbose)
            {
                ColorConsole.Log(LogLevel.Verbose, $"{nameof(StartingInState)}: {state}");
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void StoppingDaemon()
        {
            if (_logLevel <= LogLevel.Info)
            {
                ColorConsole.Log(LogLevel.Info, nameof(StoppingDaemon));
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Unloaded()
        {
            if (_logLevel <= LogLevel.Info)
            {
                ColorConsole.Log(LogLevel.Info, nameof(Unloaded));
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity Unloading()
        {
            if (_logLevel <= LogLevel.Verbose)
            {
                ColorConsole.Log(LogLevel.Verbose, nameof(Unloading));
            }
            return new StubActivity(nameof(Unloading));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void UsingFeatureLoader(Type type)
        {
            if (_logLevel <= LogLevel.Verbose)
            {
                ColorConsole.Log(LogLevel.Verbose, $"{nameof(UsingFeatureLoader)}: {type.FriendlyName()}");
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity ConfiguringAdapterInjectionPorts()
        {
            if (_logLevel <= LogLevel.Verbose)
            {
                ColorConsole.Log(LogLevel.Verbose, nameof(ConfiguringAdapterInjectionPorts));
            }
            return new StubActivity(nameof(ConfiguringAdapterInjectionPorts));
        }


        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity ConfiguringAdapterPort(Type portType)
        {
            if (_logLevel <= LogLevel.Verbose)
            {
                ColorConsole.Log(LogLevel.Verbose, $"{nameof(ConfiguringAdapterPort)}: {portType.FriendlyName()}");
            }
            return new StubActivity($"{nameof(ConfiguringAdapterPort)}: {portType.FriendlyName()}");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class StubActivity : IExecutionPathActivity
        {
            public StubActivity(string text)
            {
                this.Text = text;
            }
            public void Dispose()
            {
            }
            public void Fail(Exception error)
            {
            }
            public void Fail(string reason)
            {
            }
            public string Text { get; }
        }
    }
}
