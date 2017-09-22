using NWheels.Kernel.Api.Execution;
using System;
using NWheels.Kernel.Api.Logging;

namespace NWheels.Microservices.Runtime
{
    [LoggerComponent]
    public interface IMicroserviceHostLogger
    {
        [LogVerbose]
        void StartingInState(MicroserviceState state);

        [LogVerbose]
        void EnteredState(MicroserviceState state);

        [LogCritical]
        void Faulted();

        [LogVerbose]
        IExecutionPathActivity Configuring();

        [LogInfo]
        void Configured();

        [LogCritical]
        void FailedToConfigure(Exception error);

        [LogVerbose]
        IExecutionPathActivity Compiling();

        [LogInfo]
        void Compiled();

        [LogCritical]
        void FailedToCompile(Exception error);

        [LogVerbose]
        IExecutionPathActivity Loading();

        [LogInfo]
        void Loaded();

        [LogCritical]
        void FailedToLoad(Exception error);

        [LogVerbose]
        IExecutionPathActivity Activating();

        [LogInfo]
        void Activated();

        [LogCritical]
        void FailedToActivate(Exception error);

        [LogVerbose]
        IExecutionPathActivity Deactivating();

        [LogInfo]
        void Deactivated();

        [LogCritical]
        void FailedToDeactivate(Exception error);

        [LogVerbose]
        IExecutionPathActivity Unloading();

        [LogInfo]
        void Unloaded();

        [LogCritical]
        void FailedToUnload(Exception error);

        [LogVerbose]
        void UsingFeatureLoader(Type type);

        [LogVerbose]
        IExecutionPathActivity FeaturesContributingConfigSections();

        [LogVerbose]
        IExecutionPathActivity FeatureContributingConfigSections(Type loader);

        [LogVerbose]
        IExecutionPathActivity FeaturesContributingConfiguration();

        [LogVerbose]
        IExecutionPathActivity FeatureContributingConfiguration(Type loader);

        [LogVerbose]
        IExecutionPathActivity FeaturesContributingComponents();

        [LogVerbose]
        IExecutionPathActivity FeatureContributingComponents(Type loader);

        [LogVerbose]
        IExecutionPathActivity FeaturesContributingAdapterComponents();

        [LogVerbose]
        IExecutionPathActivity FeatureContributingAdapterComponents(Type loader);

        [LogVerbose]
        IExecutionPathActivity FeaturesCompilingComponents();

        [LogVerbose]
        IExecutionPathActivity FeatureCompilingComponents(Type loader);

        [LogVerbose]
        IExecutionPathActivity FeaturesContributingCompiledComponents();

        [LogVerbose]
        IExecutionPathActivity FeatureContributingCompiledComponents(Type loader);

        [LogVerbose]
        IExecutionPathActivity ExecutingFeatureLoaderPhaseExtension(Type loaderType, string phase);

        [LogError]
        void FeatureLoaderFailed(Type loaderType, string phase, Exception error);

        [LogError]
        void FeatureLoaderPhaseExtensionFailed(Type loaderType, string phase, Exception error);

        [LogVerbose]
        IExecutionPathActivity LoadingLifecycleComponents();

        [LogVerbose]
        void LoadedLifecycleComponent(Type type);

        [LogCritical]
        void FailedToLoadLifecycleComponents(Exception error);

        [LogWarning]
        void NoLifecycleComponentsLoaded();

        [LogVerbose]
        IExecutionPathActivity LifecycleComponentsMicroserviceLoading();

        [LogVerbose]
        IExecutionPathActivity LifecycleComponentsLoad();

        [LogVerbose]
        IExecutionPathActivity LifecycleComponentsMicroserviceLoaded();

        [LogVerbose]
        IExecutionPathActivity LifecycleComponentsMicroserviceActivating();

        [LogVerbose]
        IExecutionPathActivity LifecycleComponentsActivate();

        [LogVerbose]
        IExecutionPathActivity LifecycleComponentsMicroserviceActivated();

        [LogVerbose]
        IExecutionPathActivity LifecycleComponentsMicroserviceMaybeDeactivating();

        [LogVerbose]
        IExecutionPathActivity LifecycleComponentsMayDeactivate();

        [LogVerbose]
        IExecutionPathActivity LifecycleComponentsMicroserviceMaybeDeactivated();

        [LogVerbose]
        IExecutionPathActivity LifecycleComponentsMicroserviceMaybeUnloading();

        [LogVerbose]
        IExecutionPathActivity LifecycleComponentsMayUnload();

        [LogVerbose]
        IExecutionPathActivity LifecycleComponentsMicroserviceMaybeUnloaded();

        [LogError]
        void LifecycleComponentFailed(Type componentType, string lifecycleMethod, Exception error);

        [LogInfo]
        void RunningAsDaemon();

        [LogInfo]
        void StoppingDaemon();

        [LogInfo]
        void RunningInBatchJobMode();

        [LogInfo]
        void BatchJobCompleted();

        [LogWarning]
        void BatchJobCanceled();

        [LogError]
        void BatchJobFailed(Exception error);
    }
}
