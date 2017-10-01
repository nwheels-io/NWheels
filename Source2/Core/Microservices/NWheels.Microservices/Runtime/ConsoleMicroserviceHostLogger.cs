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
            return _s_stubActivity;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void BatchJobCanceled()
        {
            if (_logLevel <= LogLevel.Warning)
            {
                ColorConsole.Log(LogLevel.Warning, nameof(BatchJobCanceled));
            }
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

        public void BatchJobFailed(Exception error)
        {
            if (_logLevel <= LogLevel.Error)
            {
                ColorConsole.Log(LogLevel.Error, $"{nameof(BatchJobFailed)}: {error}");
            }
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
            return _s_stubActivity;
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
            return _s_stubActivity;
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
            return _s_stubActivity;
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
            return _s_stubActivity;
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

        public void FailedToLoadLifecycleComponents(Exception error)
        {
            if (_logLevel <= LogLevel.Critical)
            {
                ColorConsole.Log(LogLevel.Critical, $"{nameof(FailedToLoadLifecycleComponents)}: {error}");
            }
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
            return _s_stubActivity;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity FeatureContributingAdapterComponents(Type loader)
        {
            if (_logLevel <= LogLevel.Verbose)
            {
                ColorConsole.Log(LogLevel.Verbose, $"{nameof(FeatureContributingAdapterComponents)}: {loader.FriendlyName()}");
            }
            return _s_stubActivity;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity FeatureContributingCompiledComponents(Type loader)
        {
            if (_logLevel <= LogLevel.Verbose)
            {
                ColorConsole.Log(LogLevel.Verbose, $"{nameof(FeatureContributingCompiledComponents)}: {loader.FriendlyName()}");
            }
            return _s_stubActivity;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity FeatureContributingComponents(Type loader)
        {
            if (_logLevel <= LogLevel.Verbose)
            {
                ColorConsole.Log(LogLevel.Verbose, $"{nameof(FeatureContributingComponents)}: {loader.FriendlyName()}");
            }
            return _s_stubActivity;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity FeatureContributingConfigSections(Type loader)
        {
            if (_logLevel <= LogLevel.Verbose)
            {
                ColorConsole.Log(LogLevel.Verbose, $"{nameof(FeatureContributingConfigSections)}: {loader.FriendlyName()}");
            }
            return _s_stubActivity;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity FeatureContributingConfiguration(Type loader)
        {
            if (_logLevel <= LogLevel.Verbose)
            {
                ColorConsole.Log(LogLevel.Verbose, $"{nameof(FeatureContributingConfiguration)}: {loader.FriendlyName()}");
            }
            return _s_stubActivity;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MicroserviceHostException FeatureLoaderFailed(Type loaderType, string phase, Exception error)
        {
            if (_logLevel <= LogLevel.Error)
            {
                ColorConsole.Log(LogLevel.Error, $"{nameof(FeatureLoaderFailed)}: phase={phase}, {error}");
            }
            return MicroserviceHostException.FeatureLoaderFailed(loaderType, phase, error);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void FeatureLoaderPhaseExtensionFailed(Type loaderType, string phase, Exception error)
        {
            if (_logLevel <= LogLevel.Error)
            {
                ColorConsole.Log(LogLevel.Error, $"{nameof(FeatureLoaderPhaseExtensionFailed)}: phase={phase}, {error}");
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity FeaturesCompilingComponents()
        {
            if (_logLevel <= LogLevel.Verbose)
            {
                ColorConsole.Log(LogLevel.Verbose, nameof(FeaturesCompilingComponents));
            }
            return _s_stubActivity;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity FeaturesContributingAdapterComponents()
        {
            if (_logLevel <= LogLevel.Verbose)
            {
                ColorConsole.Log(LogLevel.Verbose, nameof(FeaturesContributingAdapterComponents));
            }
            return _s_stubActivity;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity FeaturesContributingCompiledComponents()
        {
            if (_logLevel <= LogLevel.Verbose)
            {
                ColorConsole.Log(LogLevel.Verbose, nameof(FeaturesContributingCompiledComponents));
            }
            return _s_stubActivity;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity FeaturesContributingComponents()
        {
            if (_logLevel <= LogLevel.Verbose)
            {
                ColorConsole.Log(LogLevel.Verbose, nameof(FeaturesContributingComponents));
            }
            return _s_stubActivity;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity FeaturesContributingConfigSections()
        {
            if (_logLevel <= LogLevel.Verbose)
            {
                ColorConsole.Log(LogLevel.Verbose, nameof(FeaturesContributingConfigSections));
            }
            return _s_stubActivity;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity FeaturesContributingConfiguration()
        {
            if (_logLevel <= LogLevel.Verbose)
            {
                ColorConsole.Log(LogLevel.Verbose, nameof(FeaturesContributingConfiguration));
            }
            return _s_stubActivity;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void LifecycleComponentFailed(Type componentType, string lifecycleMethod, Exception error)
        {
            if (_logLevel <= LogLevel.Error)
            {
                ColorConsole.Log(LogLevel.Error, $"{nameof(LifecycleComponentFailed)}: {componentType.FriendlyName()}, method={lifecycleMethod}, {error}");
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity LifecycleComponentsActivate()
        {
            if (_logLevel <= LogLevel.Verbose)
            {
                ColorConsole.Log(LogLevel.Verbose, nameof(LifecycleComponentsActivate));
            }
            return _s_stubActivity;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity LifecycleComponentsLoad()
        {
            if (_logLevel <= LogLevel.Verbose)
            {
                ColorConsole.Log(LogLevel.Verbose, nameof(LifecycleComponentsLoad));
            }
            return _s_stubActivity;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity LifecycleComponentsMayDeactivate()
        {
            if (_logLevel <= LogLevel.Verbose)
            {
                ColorConsole.Log(LogLevel.Verbose, nameof(LifecycleComponentsMayDeactivate));
            }
            return _s_stubActivity;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity LifecycleComponentsMayUnload()
        {
            if (_logLevel <= LogLevel.Verbose)
            {
                ColorConsole.Log(LogLevel.Verbose, nameof(LifecycleComponentsMayUnload));
            }
            return _s_stubActivity;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity LifecycleComponentsMicroserviceActivated()
        {
            if (_logLevel <= LogLevel.Verbose)
            {
                ColorConsole.Log(LogLevel.Verbose, nameof(LifecycleComponentsMicroserviceActivated));
            }
            return _s_stubActivity;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity LifecycleComponentsMicroserviceActivating()
        {
            if (_logLevel <= LogLevel.Verbose)
            {
                ColorConsole.Log(LogLevel.Verbose, nameof(LifecycleComponentsMicroserviceActivating));
            }
            return _s_stubActivity;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity LifecycleComponentsMicroserviceLoaded()
        {
            if (_logLevel <= LogLevel.Verbose)
            {
                ColorConsole.Log(LogLevel.Verbose, nameof(LifecycleComponentsMicroserviceLoaded));
            }
            return _s_stubActivity;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity LifecycleComponentsMicroserviceLoading()
        {
            if (_logLevel <= LogLevel.Verbose)
            {
                ColorConsole.Log(LogLevel.Verbose, nameof(LifecycleComponentsMicroserviceLoading));
            }
            return _s_stubActivity;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity LifecycleComponentsMicroserviceMaybeDeactivated()
        {
            if (_logLevel <= LogLevel.Verbose)
            {
                ColorConsole.Log(LogLevel.Verbose, nameof(LifecycleComponentsMicroserviceMaybeDeactivated));
            }
            return _s_stubActivity;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity LifecycleComponentsMicroserviceMaybeDeactivating()
        {
            if (_logLevel <= LogLevel.Verbose)
            {
                ColorConsole.Log(LogLevel.Verbose, nameof(LifecycleComponentsMicroserviceMaybeDeactivating));
            }
            return _s_stubActivity;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity LifecycleComponentsMicroserviceMaybeUnloaded()
        {
            if (_logLevel <= LogLevel.Verbose)
            {
                ColorConsole.Log(LogLevel.Verbose, nameof(LifecycleComponentsMicroserviceMaybeUnloaded));
            }
            return _s_stubActivity;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity LifecycleComponentsMicroserviceMaybeUnloading()
        {
            if (_logLevel <= LogLevel.Verbose)
            {
                ColorConsole.Log(LogLevel.Verbose, nameof(LifecycleComponentsMicroserviceMaybeUnloading));
            }
            return _s_stubActivity;
        }

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
            return _s_stubActivity;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity LoadingLifecycleComponents()
        {
            if (_logLevel <= LogLevel.Verbose)
            {
                ColorConsole.Log(LogLevel.Verbose, nameof(LoadingLifecycleComponents));
            }
            return _s_stubActivity;
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
            return _s_stubActivity;
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

        private static readonly StubActivity _s_stubActivity = new StubActivity();

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class StubActivity : IExecutionPathActivity
        {
            public void Dispose()
            {
            }
            public void Fail(Exception error)
            {
            }
            public void Fail(string reason)
            {
            }
            public string Text => null;
        }
    }
}
