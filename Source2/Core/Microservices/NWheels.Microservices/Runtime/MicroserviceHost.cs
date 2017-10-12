using NWheels.Kernel.Api.Execution;
using NWheels.Kernel.Api.Extensions;
using NWheels.Kernel.Api.Injection;
using NWheels.Kernel.Api.Primitives;
using NWheels.Kernel.Runtime.Injection;
using NWheels.Microservices.Api;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading;
using NWheels.Microservices.Api.Exceptions;
using NWheels.Kernel.Api.Exceptions;

namespace NWheels.Microservices.Runtime
{
    public class MicroserviceHost : IDisposable, IMicroserviceHost
    {
        public MicroserviceHost(IBootConfiguration bootConfig)
        {
            BootConfig = bootConfig ?? throw new ArgumentNullException(nameof(bootConfig));
            BootConfig.Validate();

            BootComponents = BuildBootComponentContainer();

            ModuleLoader = BootComponents.Resolve<IModuleLoader>();
            Logger = BootComponents.Resolve<IMicroserviceHostLogger>();
            var stateCodeBehind = BootComponents.Resolve<IStateMachineCodeBehind<MicroserviceState, MicroserviceTrigger>>();

            LoadSequence = new RevertableSequence(new LoadSequenceCodeBehind(this));
            ActivateSequence = new RevertableSequence(new ActivateSequenceCodeBehind(this));
            StateLock = new SafeLock("MicroserviceHost/State");

            var stateMachine = StateMachine.CreateFrom(stateCodeBehind);
            StateScheduler = StateMachineScheduler.CreateFrom(stateMachine);
            StateScheduler.CurrentStateChanged += OnCurrentStateChanged;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual void Dispose()
        {
            using (StateLock.Acquire(TimeSpan.FromSeconds(10), nameof(Dispose)))
            {
                if (Disposed)
                {
                    return;
                }

                if (!Stop(TimeSpan.FromSeconds(30)))
                {
                    //TODO: kill unresponsive threads
                }

                Disposed = true;

                if (ModuleComponents != null)
                {
                    var oldContainer = ModuleComponents;

                    ModuleComponents = null;
                    LifecycleComponents = null;

                    oldContainer.Dispose();
                }

                BootComponents.Dispose();
                CurrentStateChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual void Configure(CancellationToken cancellation)
        {
            using (StateLock.Acquire(TimeSpan.FromSeconds(10), nameof(Configure)))
            {
                ValidateNotDisposed();

                StateScheduler.QueueTrigger(MicroserviceTrigger.Configure);
                RunStateMachineUpTo(MicroserviceState.Configured, cancellation);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual void Compile(CancellationToken cancellation)
        {
            using (StateLock.Acquire(TimeSpan.FromSeconds(10), nameof(Compile)))
            {
                ValidateNotDisposed();

                StateScheduler.QueueTrigger(MicroserviceTrigger.Configure);
                StateScheduler.QueueTrigger(MicroserviceTrigger.Compile);

                RunStateMachineUpTo(MicroserviceState.CompiledStopped, cancellation);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual void Start(CancellationToken cancellation)
        {
            using (StateLock.Acquire(TimeSpan.FromSeconds(10), nameof(Start)))
            {
                ValidateNotDisposed();
                var targetState = (BootConfig.IsClusteredMode || BootConfig.IsBatchJobMode ? MicroserviceState.Standby : MicroserviceState.Active);

                if (!BootConfig.IsPrecompiledMode)
                {
                    StateScheduler.QueueTrigger(MicroserviceTrigger.Configure);
                    StateScheduler.QueueTrigger(MicroserviceTrigger.Compile);
                }

                StateScheduler.QueueTrigger(MicroserviceTrigger.Load);

                if (targetState == MicroserviceState.Active)
                {
                    StateScheduler.QueueTrigger(MicroserviceTrigger.Activate);
                }

                RunStateMachineUpTo(targetState, cancellation);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual bool Stop(TimeSpan timeout)
        {
            using (StateLock.Acquire(TimeSpan.FromSeconds(10), nameof(Stop)))
            {
                ValidateNotDisposed();

                if (StateScheduler.CurrentState <= MicroserviceState.CompiledStopped)
                {
                    return true;
                }

                switch (StateScheduler.CurrentState)
                {
                    case MicroserviceState.Standby:
                        StateScheduler.QueueTrigger(MicroserviceTrigger.Unload);
                        break;
                    case MicroserviceState.Active:
                        StateScheduler.QueueTrigger(MicroserviceTrigger.Deactivate);
                        StateScheduler.QueueTrigger(MicroserviceTrigger.Unload);
                        break;
                    default:
                        throw MicroserviceHostException.InvalidStateForStop(StateScheduler.CurrentState);
                }

                var cancellation = new CancellationTokenSource();
                cancellation.CancelAfter(timeout);

                var succeeded = RunStateMachineUpTo(MicroserviceState.CompiledStopped, cancellation.Token, throwOnCancellation: false);
                return succeeded;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual void RunDaemon(CancellationToken cancellation, TimeSpan stopTimeout, out bool stoppedWithinTimeout)
        {
            if (BootConfig.IsBatchJobMode)
            {
                throw MicroserviceHostException.NotConfiguredToRunInDaemonMode();
            }

            using (StateLock.Acquire(TimeSpan.FromSeconds(10), nameof(RunDaemon)))
            {
                ValidateNotDisposed();
                Start(cancellation);
                Logger.RunningInDaemonMode();

                cancellation.WaitHandle.WaitOne();

                Logger.StoppingDaemon();
                stoppedWithinTimeout = Stop(stopTimeout);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual bool RunBatchJob(Action batchJob, CancellationToken cancellation, TimeSpan stopTimeout, out bool stoppedWithinTimeout)
        {
            bool invokeBatchJob()
            {
                Logger.RunningInBatchJobMode();

                try
                {
                    batchJob();

                    if (!cancellation.IsCancellationRequested)
                    {
                        Logger.BatchJobCompleted();
                        return true;
                    }

                    Logger.BatchJobCanceled();
                }
                catch (OperationCanceledException)
                {
                    Logger.BatchJobCanceled();
                }
                catch (Exception error)
                {
                    Logger.BatchJobFailed(error);
                }

                return false;
            }

            if (!BootConfig.IsBatchJobMode)
            {
                throw MicroserviceHostException.NotConfiguredToRunInBatchJobMode();
            }

            bool success;

            using (StateLock.Acquire(TimeSpan.FromSeconds(10), nameof(RunBatchJob)))
            {
                ValidateNotDisposed();
                Start(cancellation);

                success = invokeBatchJob();

                stoppedWithinTimeout = Stop(stopTimeout);
            }

            return success;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IComponentContainer GetBootComponents()
        {
            return this.BootComponents;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IComponentContainer GetModuleComponents()
        {
            return this.ModuleComponents;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IBootConfiguration BootConfig { get; }
        public IMicroserviceHostLogger Logger { get; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MicroserviceState CurrentState => (this.Disposed ? MicroserviceState.Disposed : StateScheduler.CurrentState);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public event EventHandler CurrentStateChanged;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected IComponentContainer BootComponents { get; }
        protected IModuleLoader ModuleLoader { get; }
        protected StateMachineScheduler<MicroserviceState, MicroserviceTrigger> StateScheduler { get; }
        protected SafeLock StateLock { get; } 
        protected IInternalComponentContainer ModuleComponents { get; private set; }
        protected List<IFeatureLoader> FeatureLoaders { get; private set; }
        protected List<ILifecycleComponent> LifecycleComponents { get; private set; }
        protected RevertableSequence LoadSequence { get; }
        protected RevertableSequence ActivateSequence { get; }
        protected bool Disposed { get; set; }
        protected Exception TransitionLastError { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected void ValidateNotDisposed()
        {
            if (Disposed)
            {
                throw new ObjectDisposedException(nameof(MicroserviceHost));
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected bool RunStateMachineUpTo(MicroserviceState targetState, CancellationToken cancellation, bool throwOnCancellation = true)
        {
            Exception error = null;

            var finalState = StateScheduler.RunOnCurrentThread(
                exitWhen: state => {
                    return (state == targetState || state == MicroserviceState.Faulted);
                },
                onError: (state, e) => {
                    error = e;
                    TransitionLastError = e;
                    return false; // exit on error
                },
                cancellation: cancellation
            );

            if (error != null || TransitionLastError != null || finalState == MicroserviceState.Faulted)
            {
                throw MicroserviceHostException.MicroserviceFaulted(error ?? TransitionLastError);
            }

            if (finalState != targetState && (!cancellation.IsCancellationRequested || !throwOnCancellation))
            {
                throw MicroserviceHostException.MicroserviceDidNotReachRequiredState(required: targetState, actual: finalState);
            }

            return (finalState == targetState);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected MicroserviceTrigger ExecuteStateTransitionPhase(
            Action phaseAction, 
            Func<IExecutionPathActivity> logActivity, 
            Action logSuccess, 
            Action<Exception> logError)
        {
            TransitionLastError = null;

            using (logActivity())
            {
                try
                {
                    phaseAction();
                    logSuccess();
                    return MicroserviceTrigger.OK;
                }
                catch (Exception e)
                {
                    logError(e);
                    TransitionLastError = e;
                    return MicroserviceTrigger.Failed;
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected void ExecuteFeatureLoaderPhase(
            IEnumerable<IFeatureLoader> featureLoaders,
            Func<IExecutionPathActivity> logPhase,
            Func<Type, IExecutionPathActivity> logFeature,
            Action<IFeatureLoader> action)
        {
            logPhase().RunActivityOrThrow(phaseActivity => {
                foreach (var feature in featureLoaders)
                {
                    var featureActivity = logFeature(feature.GetType());

                    if (!featureActivity.RunActivityOrCatch(() => action(feature), out Exception error))
                    {
                        throw Logger.FeatureLoaderFailed(feature.GetType(), phase: phaseActivity.Text, error: error);
                    }
                }
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected void ExecuteFeatureLoaderPhase(
            IEnumerable<IFeatureLoader> featureLoaders,
            Func<IExecutionPathActivity> logPhase,
            Func<Type, IExecutionPathActivity> logFeature,
            Action<IFeatureLoader, IComponentContainerBuilder> action)
        {
            logPhase().RunActivityOrThrow(phaseActivity => {

                var newComponents = new ComponentContainerBuilder(this.ModuleComponents);

                foreach (var feature in featureLoaders)
                {
                    var featureActivity = logFeature(feature.GetType());

                    if (!featureActivity.RunActivityOrCatch(() => action(feature, newComponents), out Exception error))
                    {
                        throw Logger.FeatureLoaderFailed(feature.GetType(), phase: phaseActivity.Text, error: error);
                    }
                }

                this.ModuleComponents.Merge(newComponents);
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected void ExecuteFeatureLoaderPhaseExtensions(
            IEnumerable<IFeatureLoader> featureLoaders,
            Func<IFeatureLoaderPhaseExtension, Action<IComponentContainer>> actionSelector)
        {
            foreach (var loader in featureLoaders.OfType<IFeatureLoaderWithPhaseExtension>())
            {
                var extension = loader.PhaseExtension;
                if (extension == null)
                {
                    continue;
                }

                var action = actionSelector(extension);

                using (var activity = Logger.ExecutingFeatureLoaderPhaseExtension(loader.GetType(), phase: action.Method.Name))
                {
                    try
                    {
                        action(this.ModuleComponents);
                    }
                    catch (Exception e)
                    {
                        activity.Fail(e);
                        Logger.FeatureLoaderPhaseExtensionFailed(loader.GetType(), phase: action.Method.Name, error: e);
                    }
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual RevertableSequence CreateLoadSequence()
        {
            return new RevertableSequence(new LoadSequenceCodeBehind(this));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual RevertableSequence CreateActivateSequence()
        {
            return new RevertableSequence(new ActivateSequenceCodeBehind(this));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual void LoadLifecycleComponents()
        {
            using (var activity = Logger.LoadingLifecycleComponents())
            {
                try
                {
                    this.LifecycleComponents = ModuleComponents.ResolveAll<ILifecycleComponent>().ToList();

                    foreach (var compolonent in this.LifecycleComponents)
                    {
                        Logger.LoadedLifecycleComponent(compolonent.GetType());
                    }

                    if (this.LifecycleComponents.Count == 0 && !BootConfig.IsBatchJobMode)
                    {
                        Logger.NoLifecycleComponentsLoaded();
                    }
                }
                catch (Exception e)
                {
                    activity.Fail(e);
                    Logger.FailedToLoadLifecycleComponents(e);
                    throw;
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual MicroserviceTrigger OnConfiguring()
        {
            void doConfigure()
            {
                var rootBuilder = new ComponentContainerBuilder(rootContainer: null);

                this.ModuleComponents = InitializeModuleComponentContainer();
                this.FeatureLoaders = ModuleLoader.GetBootFeatureLoaders().ToList();

                doFeatureLoaderPhases();
            }

            void doFeatureLoaderPhases()
            {
                ExecuteFeatureLoaderPhaseExtensions(FeatureLoaders, extension => extension.BeforeContributeConfigSections);

                ExecuteFeatureLoaderPhase(
                    FeatureLoaders, Logger.FeaturesContributingConfigSections, Logger.FeatureContributingConfigSections, 
                    (feature, newComponents) => feature.ContributeConfigSections(newComponents));

                ExecuteFeatureLoaderPhaseExtensions(FeatureLoaders, extension => extension.BeforeContributeConfiguration);

                ExecuteFeatureLoaderPhase(
                    FeatureLoaders, Logger.FeaturesContributingConfiguration, Logger.FeatureContributingConfiguration,
                    (feature) => feature.ContributeConfiguration(this.ModuleComponents));

                ExecuteFeatureLoaderPhaseExtensions(FeatureLoaders, extension => extension.BeforeContributeComponents);

                ExecuteFeatureLoaderPhase(
                    FeatureLoaders, Logger.FeaturesContributingComponents, Logger.FeatureContributingComponents,
                    (feature, newComponents) => feature.ContributeComponents(this.ModuleComponents, newComponents));

                ExecuteFeatureLoaderPhaseExtensions(FeatureLoaders, extension => extension.BeforeContributeAdapterComponents);

                ExecuteFeatureLoaderPhase(
                    FeatureLoaders, Logger.FeaturesContributingAdapterComponents, Logger.FeatureContributingAdapterComponents,
                    (feature, newComponents) => feature.ContributeAdapterComponents(this.ModuleComponents, newComponents));
            }

            return ExecuteStateTransitionPhase(doConfigure, Logger.Configuring, Logger.Configured, Logger.FailedToConfigure);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual MicroserviceTrigger OnCompiling()
        {
            void doCompile()
            {
                ExecuteFeatureLoaderPhaseExtensions(FeatureLoaders, extension => extension.BeforeCompileComponents);

                ExecuteFeatureLoaderPhase(
                    FeatureLoaders, Logger.FeaturesCompilingComponents, Logger.FeatureCompilingComponents,
                    (feature, newComponents) => feature.CompileComponents(this.ModuleComponents));

                ExecuteFeatureLoaderPhaseExtensions(FeatureLoaders, extension => extension.BeforeContributeCompiledComponents);

                ExecuteFeatureLoaderPhase(
                    FeatureLoaders, Logger.FeaturesContributingCompiledComponents, Logger.FeatureContributingCompiledComponents,
                    (feature, newComponents) => feature.ContributeCompiledComponents(this.ModuleComponents, newComponents));

                ExecuteFeatureLoaderPhaseExtensions(FeatureLoaders, extension => extension.AfterContributeCompiledComponents);
            }

            return ExecuteStateTransitionPhase(doCompile, Logger.Compiling, Logger.Compiled, Logger.FailedToCompile);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual void OnUnloaded()
        {
            ModuleComponents = null;
            FeatureLoaders = null;
            LifecycleComponents = null;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual MicroserviceTrigger OnLoading()
        {
            return ExecuteStateTransitionPhase(LoadSequence.Perform, Logger.Loading, Logger.Loaded, Logger.FailedToLoad);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual MicroserviceTrigger OnActivating()
        {
            return ExecuteStateTransitionPhase(ActivateSequence.Perform, Logger.Activating, Logger.Activated, Logger.FailedToActivate);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual MicroserviceTrigger OnDeactivating()
        {
            return ExecuteStateTransitionPhase(ActivateSequence.Revert, Logger.Deactivating, Logger.Deactivated, Logger.FailedToDeactivate);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual MicroserviceTrigger OnUnloading()
        {
            return ExecuteStateTransitionPhase(LoadSequence.Revert, Logger.Unloading, Logger.Unloaded, Logger.FailedToUnload);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual void OnCurrentStateChanged(object sender, EventArgs args)
        {
            Logger.EnteredState(StateScheduler.CurrentState);
            CurrentStateChanged?.Invoke(this, args);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private MicroserviceStateMachineOptions CreateStateCodeBehindOptions()
        {
            return new MicroserviceStateMachineOptions {
                Host = this,
                BootConfig = this.BootConfig,
                OnConfiguring = this.OnConfiguring,
                OnCompiling = this.OnCompiling,
                OnLoading = this.OnLoading,
                OnActivating = this.OnActivating,
                OnDeactivating = this.OnDeactivating,
                OnUnloading = this.OnUnloading,
                OnUnloaded = this.OnUnloaded
            };
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private IComponentContainer BuildBootComponentContainer()
        {
            var builder = new ComponentContainerBuilder();

            builder.RegisterComponentType<ConsoleMicroserviceHostLogger>()
                .SingleInstance()
                .ForService<IMicroserviceHostLogger>()
                .AsFallback();

            builder.RegisterComponentType<DefaultModuleLoader>()
                .SingleInstance()
                .ForService<IModuleLoader>()
                .AsFallback();

            builder.RegisterComponentType<DefaultMicroserviceStateCodeBehind>()
                .InstancePerDependency()
                .ForService<IStateMachineCodeBehind<MicroserviceState, MicroserviceTrigger>>()
                .AsFallback();

            builder.RegisterComponentInstance(this);
            builder.RegisterComponentInstance(BootConfig).ForService<IBootConfiguration>();
            builder.RegisterComponentInstance(CreateStateCodeBehindOptions());
            BootConfig.BootComponents.Contribute(builder);

            return builder.CreateComponentContainer();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private IInternalComponentContainer InitializeModuleComponentContainer()
        {
            var builder = new ComponentContainerBuilder();

            builder.RegisterComponentInstance(this);
            builder.RegisterComponentInstance(BootConfig).ForService<IBootConfiguration>();

            return builder.CreateComponentContainer();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected abstract class LifecycleComponentSequenceBase
        {
            protected LifecycleComponentSequenceBase(MicroserviceHost ownerHost)
            {
                OwnerHost = ownerHost;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected void InvokeLifecycleMethod(
                ILifecycleComponent component,
                Func<ILifecycleComponent, Action> lifecycleMethodSelector)
            {
                var lifecycleMethod = lifecycleMethodSelector(component);

                try
                {
                    lifecycleMethod();
                }
                catch (Exception error)
                {
                    throw OwnerHost.Logger.LifecycleComponentFailed(component.GetType(), lifecycleMethod.Method.Name, error);
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected void JoinSystemSession()
            {
                // TODO: grant root permissions to current execution path
                //_systemSession = _ownerLifetime.LifetimeContainer.Resolve<ISessionManager>().JoinGlobalSystem();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected void LeaveSystemSession()
            {
                // TODO: revoke root permissions from current execution path

                //_systemSession.Dispose();
                //_systemSession = null;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected MicroserviceHost OwnerHost { get; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class LoadSequenceCodeBehind : LifecycleComponentSequenceBase, IRevertableSequenceCodeBehind
        {
            public LoadSequenceCodeBehind(MicroserviceHost ownerHost)
                : base(ownerHost)
            {
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void BuildSequence(IRevertableSequenceBuilder sequence)
            {
                sequence.Once().OnPerform(OwnerHost.LoadLifecycleComponents);

                sequence.ForEach(() => OwnerHost.LifecycleComponents)
                    .OnPerform((component, index, isLast) => InvokeLifecycleMethod(component, x => x.MicroserviceLoading))
                    .OnRevert((component, index, isLast) => InvokeLifecycleMethod(component, x => x.MicroserviceMaybeUnloaded));

                sequence.ForEach(() => OwnerHost.LifecycleComponents)
                    .OnPerform((component, index, isLast) => InvokeLifecycleMethod(component, x => x.Load))
                    .OnRevert((component, index, isLast) => InvokeLifecycleMethod(component, x => x.MayUnload));

                sequence.ForEach(() => OwnerHost.LifecycleComponents)
                    .OnPerform((component, index, isLast) => InvokeLifecycleMethod(component, x => x.MicroserviceLoaded))
                    .OnRevert((component, index, isLast) => InvokeLifecycleMethod(component, x => x.MicroserviceMaybeUnloading));

                //TODO: initialize under root permissions
                /*sequence.Once().OnPerform(JoinSystemSession).OnRevert(LeaveSystemSession);
                sequence.Once().OnPerform(LeaveSystemSession).OnRevert(JoinSystemSession);*/
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class ActivateSequenceCodeBehind : LifecycleComponentSequenceBase, IRevertableSequenceCodeBehind
        {
            public ActivateSequenceCodeBehind(MicroserviceHost ownerHost)
                : base(ownerHost)
            {
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void BuildSequence(IRevertableSequenceBuilder sequence)
            {
                sequence.ForEach(() => OwnerHost.LifecycleComponents)
                    .OnPerform((component, index, isLast) => InvokeLifecycleMethod(component, x => x.MicroserviceActivating))
                    .OnRevert((component, index, isLast) => InvokeLifecycleMethod(component, x => x.MicroserviceMaybeDeactivated));

                sequence.ForEach(() => OwnerHost.LifecycleComponents)
                    .OnPerform((component, index, isLast) => InvokeLifecycleMethod(component, x => x.Activate))
                    .OnRevert((component, index, isLast) => InvokeLifecycleMethod(component, x => x.MayDeactivate));

                sequence.ForEach(() => OwnerHost.LifecycleComponents)
                    .OnPerform((component, index, isLast) => InvokeLifecycleMethod(component, x => x.MicroserviceActivated))
                    .OnRevert((component, index, isLast) => InvokeLifecycleMethod(component, x => x.MicroserviceMaybeDeactivating));

                //TODO: initialize under root permissions
                /*sequence.Once().OnPerform(JoinSystemSession).OnRevert(LeaveSystemSession);
                sequence.Once().OnPerform(LeaveSystemSession).OnRevert(JoinSystemSession);*/
            }
        }
    }
}
