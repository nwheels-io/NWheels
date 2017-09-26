using System;
using System.Collections.Generic;
using System.Text;
using NWheels.Kernel.Api.Execution;
using NWheels.Kernel.Api.Extensions;
using NWheels.Kernel.Api.Logging;
using NWheels.Microservices.Runtime.Cli;

namespace NWheels.Microservices.Runtime
{
    public class ConsoleMicroserviceHostLogger : IMicroserviceHostLogger
    {
        private readonly LogLevel _logLevel;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ConsoleMicroserviceHostLogger(IBootConfiguration bootConfiguration)
        {
            _logLevel = bootConfiguration.LogLevel;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Activated()
        {
            ColorConsole.Log(LogLevel.Info, nameof(Activated));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity Activating()
        {
            ColorConsole.Log(LogLevel.Verbose, nameof(Activating));
            return _s_stubActivity;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void BatchJobCanceled()
        {
            ColorConsole.Log(LogLevel.Warning, nameof(BatchJobCanceled));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void BatchJobCompleted()
        {
            ColorConsole.Log(LogLevel.Info, nameof(BatchJobCompleted));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void BatchJobFailed(Exception error)
        {
            ColorConsole.Log(LogLevel.Error, $"{nameof(BatchJobFailed)}: {error}");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Compiled()
        {
            ColorConsole.Log(LogLevel.Info, nameof(Compiled));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity Compiling()
        {
            ColorConsole.Log(LogLevel.Verbose, nameof(Compiling));
            return _s_stubActivity;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Configured()
        {
            ColorConsole.Log(LogLevel.Info, nameof(Configured));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity Configuring()
        {
            ColorConsole.Log(LogLevel.Verbose, nameof(Configuring));
            return _s_stubActivity;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Deactivated()
        {
            ColorConsole.Log(LogLevel.Info, nameof(Deactivated));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity Deactivating()
        {
            ColorConsole.Log(LogLevel.Verbose, nameof(Deactivating));
            return _s_stubActivity;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void EnteredState(MicroserviceState state)
        {
            ColorConsole.Log(LogLevel.Verbose, $"{nameof(EnteredState)}: {state}");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity ExecutingFeatureLoaderPhaseExtension(Type loaderType, string phase)
        {
            ColorConsole.Log(LogLevel.Verbose, $"{nameof(ExecutingFeatureLoaderPhaseExtension)}: {loaderType.FriendlyName()}, phase: {phase}");
            return _s_stubActivity;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void FailedToActivate(Exception error)
        {
            ColorConsole.Log(LogLevel.Critical, $"{nameof(FailedToActivate)}: {error}");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void FailedToCompile(Exception error)
        {
            ColorConsole.Log(LogLevel.Critical, $"{nameof(FailedToCompile)}: {error}");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void FailedToConfigure(Exception error)
        {
            ColorConsole.Log(LogLevel.Critical, $"{nameof(FailedToConfigure)}: {error}");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void FailedToDeactivate(Exception error)
        {
            ColorConsole.Log(LogLevel.Critical, $"{nameof(FailedToDeactivate)}: {error}");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void FailedToLoad(Exception error)
        {
            ColorConsole.Log(LogLevel.Critical, $"{nameof(FailedToLoad)}: {error}");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void FailedToLoadLifecycleComponents(Exception error)
        {
            ColorConsole.Log(LogLevel.Critical, $"{nameof(FailedToLoadLifecycleComponents)}: {error}");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void FailedToUnload(Exception error)
        {
            ColorConsole.Log(LogLevel.Critical, $"{nameof(FailedToUnload)}: {error}");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Faulted()
        {
            ColorConsole.Log(LogLevel.Critical, nameof(Faulted));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity FeatureCompilingComponents(Type loader)
        {
            ColorConsole.Log(LogLevel.Verbose, $"{nameof(FeatureCompilingComponents)}: {loader.FriendlyName()}");
            return _s_stubActivity;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity FeatureContributingAdapterComponents(Type loader)
        {
            ColorConsole.Log(LogLevel.Verbose, $"{nameof(FeatureContributingAdapterComponents)}: {loader.FriendlyName()}");
            return _s_stubActivity;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity FeatureContributingCompiledComponents(Type loader)
        {
            ColorConsole.Log(LogLevel.Verbose, $"{nameof(FeatureContributingCompiledComponents)}: {loader.FriendlyName()}");
            return _s_stubActivity;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity FeatureContributingComponents(Type loader)
        {
            ColorConsole.Log(LogLevel.Verbose, $"{nameof(FeatureContributingComponents)}: {loader.FriendlyName()}");
            return _s_stubActivity;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity FeatureContributingConfigSections(Type loader)
        {
            ColorConsole.Log(LogLevel.Verbose, $"{nameof(FeatureContributingConfigSections)}: {loader.FriendlyName()}");
            return _s_stubActivity;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity FeatureContributingConfiguration(Type loader)
        {
            ColorConsole.Log(LogLevel.Verbose, $"{nameof(FeatureContributingConfiguration)}: {loader.FriendlyName()}");
            return _s_stubActivity;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void FeatureLoaderFailed(Type loaderType, string phase, Exception error)
        {
            ColorConsole.Log(LogLevel.Error, $"{nameof(FeatureLoaderFailed)}: phase={phase}, {error}");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void FeatureLoaderPhaseExtensionFailed(Type loaderType, string phase, Exception error)
        {
            ColorConsole.Log(LogLevel.Error, $"{nameof(FeatureLoaderPhaseExtensionFailed)}: phase={phase}, {error}");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity FeaturesCompilingComponents()
        {
            ColorConsole.Log(LogLevel.Verbose, nameof(FeaturesCompilingComponents));
            return _s_stubActivity;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity FeaturesContributingAdapterComponents()
        {
            ColorConsole.Log(LogLevel.Verbose, nameof(FeaturesContributingAdapterComponents));
            return _s_stubActivity;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity FeaturesContributingCompiledComponents()
        {
            ColorConsole.Log(LogLevel.Verbose, nameof(FeaturesContributingCompiledComponents));
            return _s_stubActivity;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity FeaturesContributingComponents()
        {
            ColorConsole.Log(LogLevel.Verbose, nameof(FeaturesContributingComponents));
            return _s_stubActivity;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity FeaturesContributingConfigSections()
        {
            ColorConsole.Log(LogLevel.Verbose, nameof(FeaturesContributingConfigSections));
            return _s_stubActivity;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity FeaturesContributingConfiguration()
        {
            ColorConsole.Log(LogLevel.Verbose, nameof(FeaturesContributingConfiguration));
            return _s_stubActivity;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void LifecycleComponentFailed(Type componentType, string lifecycleMethod, Exception error)
        {
            ColorConsole.Log(LogLevel.Error, $"{nameof(LifecycleComponentFailed)}: {componentType.FriendlyName()}, method={lifecycleMethod}, {error}");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity LifecycleComponentsActivate()
        {
            ColorConsole.Log(LogLevel.Verbose, nameof(LifecycleComponentsActivate));
            return _s_stubActivity;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity LifecycleComponentsLoad()
        {
            ColorConsole.Log(LogLevel.Verbose, nameof(LifecycleComponentsLoad));
            return _s_stubActivity;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity LifecycleComponentsMayDeactivate()
        {
            ColorConsole.Log(LogLevel.Verbose, nameof(LifecycleComponentsMayDeactivate));
            return _s_stubActivity;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity LifecycleComponentsMayUnload()
        {
            ColorConsole.Log(LogLevel.Verbose, nameof(LifecycleComponentsMayUnload));
            return _s_stubActivity;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity LifecycleComponentsMicroserviceActivated()
        {
            ColorConsole.Log(LogLevel.Verbose, nameof(LifecycleComponentsMicroserviceActivated));
            return _s_stubActivity;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity LifecycleComponentsMicroserviceActivating()
        {
            ColorConsole.Log(LogLevel.Verbose, nameof(LifecycleComponentsMicroserviceActivating));
            return _s_stubActivity;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity LifecycleComponentsMicroserviceLoaded()
        {
            ColorConsole.Log(LogLevel.Verbose, nameof(LifecycleComponentsMicroserviceLoaded));
            return _s_stubActivity;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity LifecycleComponentsMicroserviceLoading()
        {
            ColorConsole.Log(LogLevel.Verbose, nameof(LifecycleComponentsMicroserviceLoading));
            return _s_stubActivity;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity LifecycleComponentsMicroserviceMaybeDeactivated()
        {
            ColorConsole.Log(LogLevel.Verbose, nameof(LifecycleComponentsMicroserviceMaybeDeactivated));
            return _s_stubActivity;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity LifecycleComponentsMicroserviceMaybeDeactivating()
        {
            ColorConsole.Log(LogLevel.Verbose, nameof(LifecycleComponentsMicroserviceMaybeDeactivating));
            return _s_stubActivity;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity LifecycleComponentsMicroserviceMaybeUnloaded()
        {
            ColorConsole.Log(LogLevel.Verbose, nameof(LifecycleComponentsMicroserviceMaybeUnloaded));
            return _s_stubActivity;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity LifecycleComponentsMicroserviceMaybeUnloading()
        {
            ColorConsole.Log(LogLevel.Verbose, nameof(LifecycleComponentsMicroserviceMaybeUnloading));
            return _s_stubActivity;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Loaded()
        {
            ColorConsole.Log(LogLevel.Verbose, nameof(Loaded));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void LoadedLifecycleComponent(Type type)
        {
            ColorConsole.Log(LogLevel.Verbose, $"{nameof(LoadedLifecycleComponent)}: {type.FriendlyName()}");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity Loading()
        {
            ColorConsole.Log(LogLevel.Verbose, nameof(Loading));
            return _s_stubActivity;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity LoadingLifecycleComponents()
        {
            ColorConsole.Log(LogLevel.Verbose, nameof(LoadingLifecycleComponents));
            return _s_stubActivity;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void NoLifecycleComponentsLoaded()
        {
            ColorConsole.Log(LogLevel.Warning, nameof(NoLifecycleComponentsLoaded));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void RunningAsDaemon()
        {
            ColorConsole.Log(LogLevel.Info, nameof(RunningAsDaemon));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void RunningInBatchJobMode()
        {
            ColorConsole.Log(LogLevel.Info, nameof(RunningInBatchJobMode));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void StartingInState(MicroserviceState state)
        {
            ColorConsole.Log(LogLevel.Verbose, $"{nameof(StartingInState)}: {state}");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void StoppingDaemon()
        {
            ColorConsole.Log(LogLevel.Info, nameof(StoppingDaemon));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Unloaded()
        {
            ColorConsole.Log(LogLevel.Info, nameof(Unloaded));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity Unloading()
        {
            ColorConsole.Log(LogLevel.Verbose, nameof(Unloading));
            return _s_stubActivity;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void UsingFeatureLoader(Type type)
        {
            ColorConsole.Log(LogLevel.Verbose, $"{nameof(UsingFeatureLoader)}: {type.FriendlyName()}");
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
