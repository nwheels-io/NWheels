using System;
using System.Collections.Generic;
using System.Text;
using NWheels.Kernel.Api.Execution;
using NWheels.Kernel.Api.Logging;

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
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity Activating()
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void BatchJobCanceled()
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void BatchJobCompleted()
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void BatchJobFailed(Exception error)
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Compiled()
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity Compiling()
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Configured()
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity Configuring()
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Deactivated()
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity Deactivating()
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void EnteredState(MicroserviceState state)
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity ExecutingFeatureLoaderPhaseExtension(Type loaderType, string phase)
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void FailedToActivate(Exception error)
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void FailedToCompile(Exception error)
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void FailedToConfigure(Exception error)
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void FailedToDeactivate(Exception error)
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void FailedToLoad(Exception error)
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void FailedToLoadLifecycleComponents(Exception error)
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void FailedToUnload(Exception error)
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Faulted()
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity FeatureCompilingComponents(Type loader)
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity FeatureContributingAdapterComponents(Type loader)
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity FeatureContributingCompiledComponents(Type loader)
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity FeatureContributingComponents(Type loader)
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity FeatureContributingConfigSections(Type loader)
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity FeatureContributingConfiguration(Type loader)
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void FeatureLoaderFailed(Type loaderType, string phase, Exception error)
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void FeatureLoaderPhaseExtensionFailed(Type loaderType, string phase, Exception error)
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity FeaturesCompilingComponents()
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity FeaturesContributingAdapterComponents()
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity FeaturesContributingCompiledComponents()
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity FeaturesContributingComponents()
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity FeaturesContributingConfigSections()
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity FeaturesContributingConfiguration()
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void LifecycleComponentFailed(Type componentType, string lifecycleMethod, Exception error)
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity LifecycleComponentsActivate()
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity LifecycleComponentsLoad()
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity LifecycleComponentsMayDeactivate()
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity LifecycleComponentsMayUnload()
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity LifecycleComponentsMicroserviceActivated()
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity LifecycleComponentsMicroserviceActivating()
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity LifecycleComponentsMicroserviceLoaded()
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity LifecycleComponentsMicroserviceLoading()
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity LifecycleComponentsMicroserviceMaybeDeactivated()
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity LifecycleComponentsMicroserviceMaybeDeactivating()
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity LifecycleComponentsMicroserviceMaybeUnloaded()
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity LifecycleComponentsMicroserviceMaybeUnloading()
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Loaded()
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void LoadedLifecycleComponent(Type type)
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity Loading()
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity LoadingLifecycleComponents()
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void NoLifecycleComponentsLoaded()
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void RunningAsDaemon()
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void RunningInBatchJobMode()
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void StartingInState(MicroserviceState state)
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void StoppingDaemon()
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Unloaded()
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity Unloading()
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void UsingFeatureLoader(Type type)
        {
            throw new NotImplementedException();
        }

    }
}
