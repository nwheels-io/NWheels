using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using Autofac;
using Autofac.Core;
using Autofac.Extras.Multitenant;
using Hapil;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NWheels.Authorization;
using NWheels.Authorization.Impl;
using NWheels.Concurrency.Impl;
using NWheels.Configuration;
using NWheels.Configuration.Core;
using NWheels.Conventions;
using NWheels.Conventions.Core;
using NWheels.Core;
using NWheels.DataObjects;
using NWheels.DataObjects.Core;
using NWheels.DataObjects.Core.Conventions;
using NWheels.Endpoints;
using NWheels.Entities;
using NWheels.Entities.Core;
using NWheels.Entities.Impl;
using NWheels.Exceptions;
using NWheels.Extensions;
using NWheels.Globalization;
using NWheels.Logging;
using NWheels.Logging.Core;
using NWheels.Logging.Impl;
using NWheels.Processing;
using NWheels.Processing.Rules.Core;
using NWheels.Processing.Workflows;
using NWheels.Processing.Workflows.Core;
using NWheels.Utilities;
using Formatting = Newtonsoft.Json.Formatting;
using NWheels.Entities.Factories;
using NWheels.Authorization.Core;
using NWheels.Authorization.Factories;
using NWheels.Concurrency;
using NWheels.Configuration.Impls;
using NWheels.Entities.Migrations;
using NWheels.Hosting.Factories;
using NWheels.Logging.Factories;
using NWheels.Processing.Commands.Factories;
using NWheels.TypeModel;
using NWheels.UI.Impl;

namespace NWheels.Hosting.Core
{
    public class NodeHost : INodeHost
    {
        public const string DynamicAssemblyName = "NWheels.RunTimeTypes";

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private readonly BootConfiguration _bootConfig;
        private readonly Action<ContainerBuilder> _registerHostComponents;
        private int _initializationCount = 0;
        private DynamicModule _dynamicModule;
        private IContainer _baseContainer;
        private INodeHostLogger _logger;
        //private readonly IInitializableHostComponent[] _hostComponents;
        private TransientStateMachine<NodeState, NodeTrigger> _stateMachine;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public NodeHost(BootConfiguration bootConfig, Action<ContainerBuilder> registerHostComponents = null)
        {
            _bootConfig = bootConfig;
            _registerHostComponents = registerHostComponents;

            CleanDynamicAssemblyFromDisk();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Load()
        {
            InitializeBeforeLoad();

            using ( _logger.NodeLoading() )
            {
                try
                {
                    _stateMachine.ReceiveTrigger(NodeTrigger.Load);
                }
                catch ( Exception e )
                {
                    _logger.NodeHasFailedToLoad(e);
                    throw;
                }

                if ( _stateMachine.CurrentState != NodeState.Standby )
                {
                    throw _logger.NodeHasFailedToLoad();
                }

                _logger.NodeSuccessfullyLoaded();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Activate()
        {
            using ( _logger.NodeActivating() )
            {
                try
                {
                    _stateMachine.ReceiveTrigger(NodeTrigger.Activate);
                }
                catch ( Exception e )
                {
                    _logger.NodeHasFailedToActivate(e);
                    throw;
                }

                if ( _stateMachine.CurrentState != NodeState.Active )
                {
                    throw _logger.NodeHasFailedToActivate();
                }

                _logger.NodeSuccessfullyActivated();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void LoadAndActivate()
        {
            InitializeBeforeLoad();

            using ( _logger.NodeStartingUp(
                _bootConfig.ApplicationName, 
                _bootConfig.EnvironmentType,
                _bootConfig.EnvironmentName,
                _bootConfig.NodeName,
                _bootConfig.InstanceId) )
            {
                Load();
                Activate();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Deactivate()
        {
            using ( _logger.NodeDeactivating() )
            {
                _stateMachine.ReceiveTrigger(NodeTrigger.Deactivate);

                if ( _stateMachine.CurrentState != NodeState.Standby )
                {
                    throw _logger.NodeHasFailedToDeactivate();
                }

                _logger.NodeDeactivated();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Unload()
        {
            using ( _logger.NodeUnloading() )
            {
                _stateMachine.ReceiveTrigger(NodeTrigger.Unload);

                if ( _stateMachine.CurrentState != NodeState.Down )
                {
                    throw _logger.NodeHasFailedToUnload();
                }

                _logger.NodeUnloaded();
            }

            FinalizeAfterUnload();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void DeactivateAndUnload()
        {
            using ( _logger.NodeShuttingDown() )
            {
                if (_stateMachine.CurrentState == NodeState.Active)
                {
                    Deactivate();
                }

                if (_stateMachine.CurrentState == NodeState.Standby)
                {
                    Unload();
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public INodeConfiguration Node
        {
            get
            {
                return _bootConfig;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public NodeState State
        {
            get
            {
                return _stateMachine.CurrentState;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        public IContainer Components
        {
            get
            {
                return _baseContainer;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal BootConfiguration BootConfig
        {
            get
            {
                return _bootConfig;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void InitializeBeforeLoad()
        {
            if ( Interlocked.Increment(ref _initializationCount) > 1 )
            {
                return;
            }

            _dynamicModule = new DynamicModule(
                simpleName: DynamicAssemblyName,
                allowSave: true,
                saveDirectory: PathUtility.DynamicArtifactPath());

            _baseContainer = BuildBaseContainer(_registerHostComponents);

            //_hostComponents = InitializeHostComponents();

            _stateMachine = new TransientStateMachine<NodeState, NodeTrigger>(
                new StateMachineCodeBehind(this),
                _baseContainer.Resolve<TransientStateMachine<NodeState, NodeTrigger>.ILogger>());

            _logger = _baseContainer.Resolve<INodeHostLogger>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void FinalizeAfterUnload()
        {
            if ( Interlocked.Decrement(ref _initializationCount) > 0 )
            {
                return;
            }

            _baseContainer.Dispose();
            _baseContainer = null;
            _dynamicModule = null;
            _stateMachine = null;
            _logger = null;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private IContainer BuildBaseContainer(Action<ContainerBuilder> registerHostComponents)
        {
            var builder = new ContainerBuilder();

            builder.RegisterInstance<NodeHost>(this);
            builder.RegisterInstance<DynamicModule>(_dynamicModule);
            builder.RegisterInstance(_bootConfig).As<INodeConfiguration>();
            builder.RegisterType<UniversalThreadLogAnchor>().As<IThreadLogAnchor>().SingleInstance();
            builder.RegisterType<ThreadRegistry>().As<ThreadRegistry, IThreadRegistry>().SingleInstance();
            builder.RegisterPipeline<IThreadLogAppender>().SingleInstance();
            builder.RegisterType<ThreadLogAppender>().As<IThreadLogAppender>().SingleInstance().LastInPipeline();
            builder.RegisterPipeline<IThreadPostMortem>().SingleInstance();
            //builder.RegisterType<StupidXmlThreadLogPersistor>().As<IThreadLogPersistor, ILifecycleEventListener>().SingleInstance();
            builder.RegisterPipeline<ILifecycleEventListener>();

            builder.RegisterType<BootTimeFramework>().As<IFramework>().WithParameter(new TypedParameter(typeof(BootConfiguration), _bootConfig)).SingleInstance();
            builder.RegisterType<DefaultAssemblySearchPathProvider>().As<IAssemblySearchPathProvider>();

            builder.RegisterType<LoggerObjectFactory>().As<LoggerObjectFactory, IAutoObjectFactory>().SingleInstance();
            builder.RegisterType<PipelineObjectFactory>().SingleInstance();
            builder.RegisterType<ConfigurationObjectFactory>().As<ConfigurationObjectFactory, IConfigurationObjectFactory, IAutoObjectFactory>().SingleInstance();
            builder.RegisterPipeline<IConfigurationSource>();
            builder.Register(c => new XmlFileConfigurationSource(this, c.Resolve<IConfigurationLogger>())).As<IConfigurationSource>().LastInPipeline();
            builder.RegisterType<XmlConfigurationLoader>().SingleInstance().InstancePerLifetimeScope();
            builder.RegisterAdapter<RelationalMappingConventionDefault, IRelationalMappingConvention>(RelationalMappingConventionBase.FromDefault).SingleInstance();
            builder.RegisterType<VoidDataRepositoryFactory>().As<IDataRepositoryFactory>();
            builder.RegisterType<EntityObjectFactory>().As<EntityObjectFactory, IEntityObjectFactory>().SingleInstance();
            builder.RegisterType<DomainObjectFactory>().As<IDomainObjectFactory, DomainObjectFactory>().SingleInstance();
            builder.RegisterType<PresentationObjectFactory>().As<IPresentationObjectFactory>().SingleInstance();
            builder.RegisterType<MethodCallObjectFactory>().As<IMethodCallObjectFactory>().SingleInstance();
            builder.RegisterPipeline<IDomainContextPopulator>();

            builder.RegisterPipeline<IComponentAspectProvider>().SingleInstance();
            builder.RegisterType<CallLoggingAspectConvention.AspectProvider>().As<IComponentAspectProvider>().AnchoredFirstInPipeline();
            builder.RegisterType<CallAuthorizationAspectConvention.AspectProvider>().As<IComponentAspectProvider>().AnchoredFirstInPipeline();
            builder.RegisterType<ComponentAspectFactory>().SingleInstance();

            builder.NWheelsFeatures().Hosting().RegisterLifecycleComponent<RealTimeoutManager>().As<RealTimeoutManager>();

            builder.RegisterType<AccessControlListCache>().SingleInstance();
            builder.RegisterType<LocalTransientSessionManager>().As<ISessionManager, ICoreSessionManager>().SingleInstance();
            builder.RegisterPipeline<AnonymousEntityAccessRule>();
            builder.RegisterType<AnonymousPrincipal>().SingleInstance();
            builder.RegisterType<SystemPrincipal>().SingleInstance();
            builder.NWheelsFeatures().Logging().RegisterLogger<IAuthorizationLogger>();
            builder.NWheelsFeatures().Logging().RegisterLogger<ISessionEventLogger>();
            
            builder.NWheelsFeatures().Configuration().RegisterSection<IFrameworkLoggingConfiguration>();
            builder.NWheelsFeatures().Configuration().RegisterSection<IFrameworkEndpointsConfig>();
            builder.NWheelsFeatures().Configuration().RegisterSection<IFrameworkDatabaseConfig>();
            builder.NWheelsFeatures().Logging().RegisterLogger<IConfigurationLogger>();
            builder.NWheelsFeatures().Logging().RegisterLogger<INodeHostLogger>();
            builder.NWheelsFeatures().Logging().RegisterLogger<IShuttleServiceLogger>();
            builder.NWheelsFeatures().Logging().RegisterLogger<TransientStateMachine<NodeState, NodeTrigger>.ILogger>();
            builder.NWheelsFeatures().Logging().RegisterLogger<IDomainContextLogger>();

            builder.RegisterPipeline<IMetadataConvention>().InstancePerDependency(); // avoid caching to allow modules extend convention set
            builder.RegisterPipeline<IRelationalMappingConvention>().InstancePerDependency(); // avoid caching to allow modules extend convention set
            builder.RegisterPipeline<SchemaMigrationCollection>().InstancePerDependency(); // avoid caching to allow modules extend migration set
            builder.RegisterType<ContractMetadataConvention>().As<IMetadataConvention>().SingleInstance().FirstInPipeline();
            builder.RegisterType<AttributeMetadataConvention>().As<IMetadataConvention>().SingleInstance().LastInPipeline();
            builder.RegisterType<RelationMetadataConvention>().As<IMetadataConvention>().SingleInstance().LastInPipeline();
            builder.RegisterType<MutationMetadataConvention>().As<IMetadataConvention>().InstancePerDependency().AnchoredLastInPipeline();
            builder.RegisterPipeline<IMetadataMutation>().InstancePerDependency(); // avoid caching to allow modules extend convention set
            builder.RegisterInstance(new PascalCaseRelationalMappingConvention(usePluralTableNames: true)).As<IRelationalMappingConvention>();
            builder.RegisterType<MetadataConventionSet>().InstancePerDependency(); // avoid caching because modules can add new registrations
            builder.RegisterType<TypeMetadataCache>().As<ITypeMetadataCache, TypeMetadataCache>().SingleInstance();
            builder.RegisterType<UnitOfWorkFactory>().SingleInstance();

            builder.NWheelsFeatures().Logging().RegisterLogger<DatabaseInitializer.ILogger>();
            builder.NWheelsFeatures().Hosting().RegisterLifecycleComponent<DatabaseInitializer>().FirstInPipeline().AsSelf();
            builder.NWheelsFeatures().Processing().RegisterTransactionScript<CrudEntityImportTx>();
            builder.NWheelsFeatures().Processing().RegisterTransactionScript<CrudEntityExportTx>();

            builder.RegisterType<VoidLocalizationProvider>().As<ILocalizationProvider>().SingleInstance();

            if ( registerHostComponents != null )
            {
                registerHostComponents(builder);
            }

            return builder.Build();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        //private IInitializableHostComponent[] InitializeHostComponents()
        //{
        //    IEnumerable<IInitializableHostComponent> resolved;

        //    if ( _baseContainer.TryResolve<IEnumerable<IInitializableHostComponent>>(out resolved) )
        //    {
        //        var initializableComponents = resolved.ToArray();

        //        foreach ( var component in initializableComponents )
        //        {
        //            component.Initializing();
        //        }

        //        return initializableComponents;
        //    }

        //    return new IInitializableHostComponent[0];
        //}

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void CleanDynamicAssemblyFromDisk()
        {
            var dynamicAssemblyPath = PathUtility.HostBinPath(DynamicAssemblyName + ".dll");

            if ( File.Exists(dynamicAssemblyPath) )
            {
                File.Delete(dynamicAssemblyPath);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private NodeLifetime CreateNodeLifetime()
        {
            return new NodeLifetime(
                _bootConfig, 
                _baseContainer, 
                //_hostComponents, 
                _logger);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class StateMachineCodeBehind : IStateMachineCodeBehind<NodeState, NodeTrigger>
        {
            private readonly NodeHost _owner;
            private NodeLifetime _lifetime = null;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public StateMachineCodeBehind(NodeHost owner)
            {
                _owner = owner;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void BuildStateMachine(IStateMachineBuilder<NodeState, NodeTrigger> machine)
            {
                machine.State(NodeState.Down)
                    .SetAsInitial()
                    .OnTrigger(NodeTrigger.Load).TransitionTo(NodeState.Loading);

                machine.State(NodeState.Loading)
                    .OnTrigger(NodeTrigger.LoadSuccess).TransitionTo(NodeState.Standby)
                    .OnTrigger(NodeTrigger.LoadFailure).TransitionTo(NodeState.Down)
                    .OnEntered(LoadingEntered);

                machine.State(NodeState.Standby)
                    .OnTrigger(NodeTrigger.Activate).TransitionTo(NodeState.Activating)
                    .OnTrigger(NodeTrigger.Unload).TransitionTo(NodeState.Unloading);

                machine.State(NodeState.Activating)
                    .OnTrigger(NodeTrigger.ActivateSuccess).TransitionTo(NodeState.Active)
                    .OnTrigger(NodeTrigger.ActivateFailure).TransitionTo(NodeState.Standby)
                    .OnEntered(ActivatingEntered);

                machine.State(NodeState.Active)
                    .OnTrigger(NodeTrigger.Deactivate).TransitionTo(NodeState.Deactivating);

                machine.State(NodeState.Deactivating)
                    .OnTrigger(NodeTrigger.DeactivateDone).TransitionTo(NodeState.Standby)
                    .OnEntered(DeactivatingEntered);

                machine.State(NodeState.Unloading)
                    .OnTrigger(NodeTrigger.UnloadDone).TransitionTo(NodeState.Down)
                    .OnEntered(UnloadingEntered);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void LoadingEntered(object sender, StateMachineFeedbackEventArgs<NodeState, NodeTrigger> e)
            {
                _lifetime = _owner.CreateNodeLifetime();
                var success = _lifetime.ExecuteLoadPhase();
                e.ReceiveFeedback(success ? NodeTrigger.LoadSuccess : NodeTrigger.LoadFailure);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void ActivatingEntered(object sender, StateMachineFeedbackEventArgs<NodeState, NodeTrigger> e)
            {
                var success = _lifetime.ExecuteActivatePhase();
                e.ReceiveFeedback(success ? NodeTrigger.ActivateSuccess : NodeTrigger.ActivateFailure);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void DeactivatingEntered(object sender, StateMachineFeedbackEventArgs<NodeState, NodeTrigger> e)
            {
                _lifetime.ExecuteDeactivatePhase();
                e.ReceiveFeedback(NodeTrigger.DeactivateDone);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void UnloadingEntered(object sender, StateMachineFeedbackEventArgs<NodeState, NodeTrigger> e)
            {
                _lifetime.ExecuteUnloadPhase();
                e.ReceiveFeedback(NodeTrigger.UnloadDone);
                _lifetime.Dispose();
                _lifetime = null;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class BootTimeFramework : RealFramework
        {
            public BootTimeFramework(IComponentContext components, INodeConfiguration nodeConfig, IThreadLogAnchor threadLogAnchor)
                : base(components, nodeConfig, threadLogAnchor, timeoutManager: null)
            {
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class NodeLifetime : IDisposable, ITenantIdentificationStrategy
        {
            private readonly BootConfiguration _nodeConfig;
            private readonly INodeHostLogger _logger;
            //private readonly IInitializableHostComponent[] _hostComponents;
            private readonly ILifetimeScope _lifetimeContainer;
            private readonly List<ILifecycleEventListener> _lifecycleComponents;
            private readonly RevertableSequence _loadSequence;
            private readonly RevertableSequence _activateSequence;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public NodeLifetime(
                BootConfiguration nodeConfig, 
                IContainer baseContainer, 
                //IInitializableHostComponent[] hostComponents, 
                INodeHostLogger logger)
            {
                _nodeConfig = nodeConfig;
                _logger = logger;
                //_hostComponents = hostComponents;
                _lifetimeContainer = baseContainer;//.BeginLifetimeScope();// new MultitenantContainer(this, baseContainer);
                _loadSequence = new RevertableSequence(new LoadSequenceCodeBehind(this));
                _activateSequence = new RevertableSequence(new ActivateSequenceCodeBehind(this));
                _lifecycleComponents = new List<ILifecycleEventListener>();

                LoadModules();
                RebuildPipelines();
                //FindLifecycleComponents();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void Dispose()
            {
                //_lifetimeContainer.Dispose();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            bool ITenantIdentificationStrategy.TryIdentifyTenant(out object tenantId)
            {
                tenantId = this;
                return true;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public bool ExecuteLoadPhase()
            {
                try
                {
                    _loadSequence.Perform();
                    return true;
                }
                catch ( Exception e )
                {
                    _logger.NodeLoadError(e);
                    return false;
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public bool ExecuteActivatePhase()
            {
                try
                {
                    _activateSequence.Perform();
                    return true;
                }
                catch ( Exception e )
                {
                    _logger.NodeActivationError(e);
                    return false;
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void ExecuteDeactivatePhase()
            {
                try
                {
                    _activateSequence.Revert();
                }
                catch ( Exception e )
                {
                    _logger.NodeDeactivationError(e);
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void ExecuteUnloadPhase()
            {
                try
                {
                    _loadSequence.Revert();
                }
                catch ( Exception e )
                {
                    _logger.NodeUnloadError(e);
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public IComponentContext LifetimeContainer
            {
                get
                {
                    return _lifetimeContainer;
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public BootConfiguration NodeConfig
            {
                get
                {
                    return _nodeConfig;
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            //public IInitializableHostComponent[] HostComponents
            //{
            //    get { return _hostComponents; }
            //}

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public List<ILifecycleEventListener> LifecycleComponents
            {
                get
                {
                    return _lifecycleComponents;
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public INodeHostLogger Logger
            {
                get
                {
                    return _logger;
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void LoadModules()
            {
                using ( _logger.LoadingModules() )
                {
                    foreach ( var module in _nodeConfig.FrameworkModules.Concat(_nodeConfig.ApplicationModules).Concat(_nodeConfig.IntegrationModules) )
                    {
                        _logger.RegisteringModule(module.Name);

                        try
                        {
                            RegisterModuleLoaderTypes(module);
                        }
                        catch ( Exception e )
                        {
                            _logger.FailedToLoadModule(module.Name, e);
                            throw;
                        }
                    }

                    var frameworkUpdater = new ContainerBuilder();
                    frameworkUpdater.RegisterType<RealFramework>().As<IFramework, ICoreFramework>().WithParameter(new TypedParameter(typeof(BootConfiguration), this.NodeConfig)).SingleInstance();
                    frameworkUpdater.RegisterGeneric(typeof(Auto<>)).SingleInstance();
                    frameworkUpdater.RegisterAdapter<IConfigSectionRegistration, IConfigurationSection>(
                        (ctx, reg) => {
                            var section = reg.ResolveFromContainer(_lifetimeContainer);
                            return section;
                        }).SingleInstance();
                    frameworkUpdater.Update(_lifetimeContainer.ComponentRegistry);

                    if ( !_nodeConfig.ApplicationModules.Any() )
                    {
                        _logger.NoApplicationModulesRegistered();
                    }
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void RegisterModuleLoaderTypes(BootConfiguration.ModuleConfig module)
            {
                var assembly = LoadModuleAssembly(module);
                
                var moduleLoaderTypes = new List<Type> {
                    assembly.GetType(module.LoaderClass, throwOnError: true)
                };

                foreach ( var feature in module.Features )
                {
                    _logger.RegisteringFeature(feature.Name);
                    moduleLoaderTypes.Add(assembly.GetType(feature.LoaderClass, throwOnError: true));
                }

                var loaderTypeUpdater = new ContainerBuilder();

                foreach ( var loaderType in moduleLoaderTypes )
                {
                    loaderTypeUpdater.RegisterType(loaderType);
                }

                loaderTypeUpdater.Update(_lifetimeContainer.ComponentRegistry);
                var moduleUpdater = new ContainerBuilder();

                foreach ( var loaderType in moduleLoaderTypes )
                {
                    var loaderInstance = (Autofac.Module)_lifetimeContainer.Resolve(loaderType);
                    moduleUpdater.RegisterModule(loaderInstance);
                }

                moduleUpdater.Update(_lifetimeContainer.ComponentRegistry);
                _lifetimeContainer.Resolve<TypeMetadataCache>().InvalidateExtensibilityRegistrations();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private Assembly LoadModuleAssembly(BootConfiguration.ModuleConfig module)
            {
                Assembly assembly;

                if ( !LoadModuleAssemblyByFilePath(module, out assembly) )
                {
                    if ( !LoadModuleAssemblyBySimpleName(module, out assembly) )
                    {
                        throw new NodeHostConfigException(string.Format("Module assembly '{0}' could not be found at any of the probed locations.", module.Assembly));
                    }
                }

                return assembly;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private bool LoadModuleAssemblyByFilePath(BootConfiguration.ModuleConfig module, out Assembly assembly)
            {
                var probeFilePaths = _lifetimeContainer.Resolve<IAssemblySearchPathProvider>().GetAssemblySearchPaths(_nodeConfig, module);

                foreach ( var filePath in probeFilePaths )
                {
                    if ( File.Exists(filePath) )
                    {
                        assembly = Assembly.LoadFrom(filePath);
                        return true;
                    }
                    else
                    {
                        _logger.ProbedModuleAssemblyLocation(filePath);
                    }
                }

                assembly = null;
                return false;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------
            
            private bool LoadModuleAssemblyBySimpleName(BootConfiguration.ModuleConfig module, out Assembly assembly)
            {
                var simpleAssemblyName = Path.GetFileNameWithoutExtension(module.Assembly);
                var loadedAssembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(asm => asm.GetName().Name.EqualsIgnoreCase(simpleAssemblyName));

                if ( loadedAssembly != null )
                {
                    _logger.AssemblyAlreadyLoaded(simpleAssemblyName);
                    assembly = loadedAssembly;
                    return true;
                }

                try
                {
                    var assemblyName = new AssemblyName() {
                        Name = simpleAssemblyName
                    };
                    
                    assembly = Assembly.Load(assemblyName);
                    return true;
                }
                catch ( Exception e )
                {
                    _logger.AssemblyLoadByNameFailed(simpleAssemblyName, e);
                    assembly = null;
                    return false;
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void RebuildPipelines()
            {
                _lifetimeContainer.Resolve<Pipeline<IThreadLogAppender>>().Rebuild(_lifetimeContainer);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private abstract class NodeLifecycleSequenceBase
        {
            private readonly NodeLifetime _ownerLifetime;
            private IDisposable _systemSession;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected NodeLifecycleSequenceBase(NodeLifetime ownerLifetime)
            {
                _ownerLifetime = ownerLifetime;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected void JoinSystemSession()
            {
                _systemSession = _ownerLifetime.LifetimeContainer.Resolve<ISessionManager>().JoinGlobalSystem();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected void LeaveSystemSession()
            {
                _systemSession.Dispose();
                _systemSession = null;
            }
            
            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected NodeLifetime OwnerLifetime
            {
                get { return _ownerLifetime; }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class LoadSequenceCodeBehind : NodeLifecycleSequenceBase, IRevertableSequenceCodeBehind
        {
            private readonly INodeHostLogger _logger;
            private bool _suppressDynamicArtifacts;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public LoadSequenceCodeBehind(NodeLifetime ownerLifetime)
                : base(ownerLifetime)
            {
                _logger = OwnerLifetime.Logger;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void BuildSequence(IRevertableSequenceBuilder sequence)
            {
                sequence.Once().OnPerform(JoinSystemSession).OnRevert(LeaveSystemSession);
                sequence.Once().OnRevert(SaveDynamicModuleToAssembly);
                sequence.Once().OnRevert(WriteEffectiveMetadataJson);
                sequence.Once().OnPerform(LoadConfiguration);
                sequence.Once().OnPerform(FindLifecycleComponents);
                sequence.ForEach(GetLifecycleComponents).OnPerform(CallComponentNodeConfigured);
                sequence.Once().OnPerform(WriteEffectiveConfigurationXml);
                sequence.Once().OnPerform(LoadDataRepositories);
                sequence.ForEach(GetLifecycleComponents).OnPerform(CallComponentNodeLoading).OnRevert(CallComponentNodeUnloaded);
                sequence.ForEach(GetLifecycleComponents).OnPerform(CallComponentLoad).OnRevert(CallComponentUnload);
                sequence.ForEach(GetLifecycleComponents).OnPerform(CallComponentNodeLoaded).OnRevert(CallComponentNodeUnloading);
                sequence.Once().OnPerform(LeaveSystemSession).OnRevert(JoinSystemSession);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void LoadConfiguration()
            {
                var sources = OwnerLifetime.LifetimeContainer.ResolvePipeline<IConfigurationSource>();
                var loader = OwnerLifetime.LifetimeContainer.Resolve<XmlConfigurationLoader>();
                loader.LoadConfiguration(sources);

                var loggingConfiguration = OwnerLifetime.LifetimeContainer.Resolve<IFrameworkLoggingConfiguration>();
                _suppressDynamicArtifacts = loggingConfiguration.SuppressDynamicArtifacts;

                Directory.CreateDirectory(PathUtility.DynamicArtifactPath());
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void LoadDataRepositories()
            {
                var loggingConfiguration = OwnerLifetime.LifetimeContainer.Resolve<IFrameworkLoggingConfiguration>();
                _suppressDynamicArtifacts = loggingConfiguration.SuppressDynamicArtifacts;

                using ( _logger.InitializingDataRepositories() )
                {
                    var repositoryFactory = OwnerLifetime.LifetimeContainer.Resolve<IDataRepositoryFactory>();
                    var allRepositoryRegistrations = OwnerLifetime.LifetimeContainer.Resolve<IEnumerable<DataRepositoryRegistration>>().ToArray();

                    foreach ( var registration in allRepositoryRegistrations )
                    {
                        LoadDataRepository(registration, repositoryFactory);
                    }
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void LoadDataRepository(DataRepositoryRegistration registration, IDataRepositoryFactory factory)
            {
                using (var repoActivity = _logger.InitializingDataRepository(type: registration.DataRepositoryType.FullName))
                {
                    try
                    {
                        var dbConfiguration = OwnerLifetime.LifetimeContainer.Resolve<IFrameworkDatabaseConfig>();
                        var contextConfig = dbConfiguration.GetContextConnectionConfig(registration.DataRepositoryType);
                        if (contextConfig == null)
                        {
                            return;
                        }

                        IDbConnectionStringResolver connectionStringResolver;

                        if (!contextConfig.IsWildcard)
                        {
                            VerifyDatabaseConnection(contextConfig, contextConfig.ConnectionString);
                        }
                        else if (OwnerLifetime.LifetimeContainer.TryResolve<IDbConnectionStringResolver>(out connectionStringResolver))
                        {
                            var storageInitializer = OwnerLifetime.LifetimeContainer.Resolve<IStorageInitializer>();
                            var allConnectionStrings = connectionStringResolver.GetAllConnectionStrings(storageInitializer);

                            foreach (var connectionString in allConnectionStrings)
                            {
                                VerifyDatabaseConnection(contextConfig, connectionString);
                            }
                        }
                        else
                        {
                            _logger.VerifyDatabaseConnectionWildcardConfigNoResolver(contextConfig.Contract.FriendlyName());
                        }
                    }
                    catch (Exception e)
                    {
                        repoActivity.Fail(e);
                        throw;
                    }
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void VerifyDatabaseConnection(IFrameworkContextPersistenceConfig contextConfig, string connectionString)
            {
                using (var activity = _logger.VerifyingConnectionToDatabase(contextConfig.Contract.FriendlyName(), connectionString))
                {
                    try
                    {
                        var unitOfWorkFactory = OwnerLifetime.LifetimeContainer.Resolve<UnitOfWorkFactory>();
                        var repoInstance = unitOfWorkFactory.NewUnitOfWork(contextConfig.Contract, connectionString: connectionString);
                        repoInstance.Dispose();
                    }
                    catch (Exception e)
                    {
                        activity.Fail(e);
                        throw;
                    }
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void WriteEffectiveConfigurationXml()
            {
                if ( _suppressDynamicArtifacts )
                {
                    return;
                }

                var loader = OwnerLifetime.LifetimeContainer.Resolve<XmlConfigurationLoader>();
                var filePath = PathUtility.DynamicArtifactPath("effective-config-dump.xml");
                
                _logger.WritingEffectiveConfigurationToDisk(filePath);

                var effectiveConfigurationXml = new XDocument();
                loader.WriteConfigurationDocument(effectiveConfigurationXml, new ConfigurationXmlOptions() { IncludeOverrideHistory = true });

                var settings = new XmlWriterSettings {
                    Indent = true,
                    IndentChars = "\t",
                    NewLineOnAttributes = true
                };

                StringBuilder effectiveConfigurationXmlText = new StringBuilder();

                using ( XmlWriter writer = XmlWriter.Create(effectiveConfigurationXmlText, settings) )
                {
                    effectiveConfigurationXml.WriteTo(writer);
                }

                File.WriteAllText(filePath, effectiveConfigurationXmlText.ToString());
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void WriteEffectiveMetadataJson()
            {
                if ( _suppressDynamicArtifacts )
                {
                    return;
                }
                
                var metadataCache = OwnerLifetime.LifetimeContainer.Resolve<TypeMetadataCache>();

                var filePath = PathUtility.HostBinPath("effective-metadata-dump.json");
                _logger.WritingEffectiveMetadataToDisk(filePath);

                var snapshot = metadataCache.TakeSnapshot();
                var jsonSettings = new JsonSerializerSettings() {
                    PreserveReferencesHandling = PreserveReferencesHandling.All,
                    ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                    Formatting = Formatting.Indented,
                    Converters = new JsonConverter[] { new StringEnumConverter() },
                    TypeNameHandling = TypeNameHandling.Objects
                };

                var json = JsonConvert.SerializeObject(snapshot, jsonSettings);
                File.WriteAllText(filePath, json);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void SaveDynamicModuleToAssembly()
            {
                if ( _suppressDynamicArtifacts )
                {
                    return;
                }

                var dynamicModule = OwnerLifetime.LifetimeContainer.Resolve<DynamicModule>();

                var filePath = PathUtility.DynamicArtifactPath(dynamicModule.SimpleName + ".dll");
                _logger.SavingDynamicModuleToAssembly(filePath);

                dynamicModule.SaveAssembly();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            //private void CallHostComponentsConfigured()
            //{
            //    foreach ( var component in _ownerLifetime.HostComponents )
            //    {
            //        using ( _logger.HostComponentConfigured(component.GetType().FullName) )
            //        {
            //            component.Configured();
            //        }
            //    }
            //}

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void FindLifecycleComponents()
            {
                using ( _logger.LookingForLifecycleComponents() )
                {
                    try
                    {
                        Pipeline<ILifecycleEventListener> foundComponents;

                        if ( OwnerLifetime.LifetimeContainer.TryResolve<Pipeline<ILifecycleEventListener>>(out foundComponents) )
                        {
                            OwnerLifetime.LifecycleComponents.AddRange(foundComponents);

                            foreach ( var component in OwnerLifetime.LifecycleComponents )
                            {
                                _logger.FoundLifecycleComponent(component.GetType().ToString());
                            }
                        }
                        else
                        {
                            _logger.NoLifecycleComponentsFound();
                        }

                        if ( OwnerLifetime.LifecycleComponents.Count == 0 )
                        {
                            _logger.NoLifecycleComponentsFound();
                        }
                    }
                    catch ( Exception e )
                    {
                        _logger.FailedToLoadLifecycleComponents(e);
                        throw;
                    }
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private IList<ILifecycleEventListener> GetLifecycleComponents()
            {
                return OwnerLifetime.LifecycleComponents;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void CallComponentNodeConfigured(ILifecycleEventListener component, int index, bool isLast)
            {
                using ( var activity = _logger.ComponentNodeConfigured(component.GetType().FullName) )
                {
                    try
                    {
                        var additionalComponents = new List<ILifecycleEventListener>();
                        component.InjectDependencies(OwnerLifetime.LifetimeContainer);
                        component.NodeConfigured(additionalComponents);
                        OwnerLifetime.LifecycleComponents.AddRange(additionalComponents);
                    }
                    catch ( Exception e )
                    {
                        activity.Fail(e);
                        _logger.ComponentNodeEventFailed(component.GetType(), "NodeConfigured", e);
                        throw;
                    }
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void CallComponentNodeLoading(ILifecycleEventListener component, int index, bool isLast)
            {
                using ( var activity = _logger.ComponentNodeLoading(component.GetType().FullName) )
                {
                    try
                    {
                        component.NodeLoading();
                    }
                    catch ( Exception e )
                    {
                        activity.Fail(e);
                        _logger.ComponentNodeEventFailed(component.GetType(), "NodeLoading", e);
                        throw;
                    }
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void CallComponentLoad(ILifecycleEventListener component, int index, bool isLast)
            {
                using ( var activity = _logger.ComponentLoading(component.GetType().FullName) )
                {
                    try
                    {
                        component.Load();
                    }
                    catch ( Exception e )
                    {
                        activity.Fail(e);
                        _logger.ComponentNodeEventFailed(component.GetType(), "Loading", e);
                        throw;
                    }
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void CallComponentNodeLoaded(ILifecycleEventListener component, int index, bool isLast)
            {
                using ( var activity = _logger.ComponentNodeLoaded(component.GetType().FullName) )
                {
                    try
                    {
                        component.NodeLoaded();
                    }
                    catch ( Exception e )
                    {
                        activity.Fail(e);
                        _logger.ComponentNodeEventFailed(component.GetType(), "NodeLoaded", e);
                        throw;
                    }
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void CallComponentNodeUnloading(ILifecycleEventListener component, int index, bool isLast)
            {
                using ( var activity = _logger.ComponentNodeUnloading(component.GetType().FullName) )
                {
                    try
                    {
                        component.NodeUnloading();
                    }
                    catch ( Exception e )
                    {
                        activity.Fail(e);
                        _logger.ComponentNodeEventFailed(component.GetType(), "NodeUnloading", e);
                        throw;
                    }
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void CallComponentUnload(ILifecycleEventListener component, int index, bool isLast)
            {
                using ( var activity = _logger.ComponentUnloading(component.GetType().FullName) )
                {
                    try
                    {
                        component.Unload();
                    }
                    catch ( Exception e )
                    {
                        activity.Fail(e);
                        _logger.ComponentNodeEventFailed(component.GetType(), "Unloading", e);
                        throw;
                    }
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void CallComponentNodeUnloaded(ILifecycleEventListener component, int index, bool isLast)
            {
                using ( var activity = _logger.ComponentNodeUnloaded(component.GetType().FullName) )
                {
                    try
                    {
                        component.NodeUnloaded();
                    }
                    catch ( Exception e )
                    {
                        activity.Fail(e);
                        _logger.ComponentNodeEventFailed(component.GetType(), "NodeUnloaded", e);
                        throw;
                    }
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class ActivateSequenceCodeBehind : NodeLifecycleSequenceBase, IRevertableSequenceCodeBehind
        {
            private readonly INodeHostLogger _logger;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ActivateSequenceCodeBehind(NodeLifetime ownerLifetime)
                : base(ownerLifetime)
            {
                _logger = OwnerLifetime.Logger;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void BuildSequence(IRevertableSequenceBuilder sequence)
            {
                sequence.Once().OnPerform(JoinSystemSession).OnRevert(LeaveSystemSession);
                sequence.ForEach(GetLifecycleComponents).OnPerform(CallComponentNodeActivating).OnRevert(CallComponentNodeDeactivated);
                sequence.ForEach(GetLifecycleComponents).OnPerform(CallComponentActivate).OnRevert(CallComponentDeactivate);
                sequence.ForEach(GetLifecycleComponents).OnPerform(CallComponentNodeActivated).OnRevert(CallComponentNodeDeactivating);
                sequence.Once().OnPerform(LeaveSystemSession).OnRevert(JoinSystemSession);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private IList<ILifecycleEventListener> GetLifecycleComponents()
            {
                return OwnerLifetime.LifecycleComponents;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void CallComponentNodeActivating(ILifecycleEventListener component, int index, bool isLast)
            {
                using ( var activity = _logger.ComponentNodeActivating(component.GetType().FullName) )
                {
                    try
                    {
                        component.NodeActivating();
                    }
                    catch ( Exception e )
                    {
                        activity.Fail(e);
                        _logger.ComponentNodeEventFailed(component.GetType(), "NodeActivating", e);
                        throw;
                    }
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void CallComponentActivate(ILifecycleEventListener component, int index, bool isLast)
            {
                using ( var activity = _logger.ComponentActivating(component.GetType().FullName) )
                {
                    try
                    {
                        component.Activate();
                    }
                    catch ( Exception e )
                    {
                        activity.Fail(e);
                        _logger.ComponentNodeEventFailed(component.GetType(), "Activate", e);
                        throw;
                    }
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void CallComponentNodeActivated(ILifecycleEventListener component, int index, bool isLast)
            {
                using ( var activity = _logger.ComponentNodeActivated(component.GetType().FullName) )
                {
                    try
                    {
                        component.NodeActivated();
                    }
                    catch ( Exception e )
                    {
                        activity.Fail(e);
                        _logger.ComponentNodeEventFailed(component.GetType(), "NodeActivated", e);
                        throw;
                    }
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void CallComponentNodeDeactivating(ILifecycleEventListener component, int index, bool isLast)
            {
                using ( var activity = _logger.ComponentNodeDeactivating(component.GetType().FullName) )
                {
                    try
                    {
                        component.NodeDeactivating();
                    }
                    catch ( Exception e )
                    {
                        activity.Fail(e);
                        _logger.ComponentNodeEventFailed(component.GetType(), "NodeDeactivating", e);
                        throw;
                    }
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void CallComponentDeactivate(ILifecycleEventListener component, int index, bool isLast)
            {
                using ( var activity = _logger.ComponentDeactivating(component.GetType().FullName) )
                {
                    try
                    {
                        component.Deactivate();
                    }
                    catch ( Exception e )
                    {
                        activity.Fail(e);
                        _logger.ComponentNodeEventFailed(component.GetType(), "Deactivate", e);
                        throw;
                    }
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void CallComponentNodeDeactivated(ILifecycleEventListener component, int index, bool isLast)
            {
                using ( var activity = _logger.ComponentNodeDeactivated(component.GetType().FullName) )
                {
                    try
                    {
                        component.NodeDeactivated();
                    }
                    catch ( Exception e )
                    {
                        activity.Fail(e);
                        _logger.ComponentNodeEventFailed(component.GetType(), "NodeDeactivated", e);
                        throw;
                    }
                }
            }
        }
    }
}
