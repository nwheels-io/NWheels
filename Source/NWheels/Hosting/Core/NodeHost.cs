using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Autofac;
using Autofac.Extras.Multitenant;
using Hapil;
using NWheels.Configuration;
using NWheels.Configuration.Core;
using NWheels.Conventions;
using NWheels.Conventions.Core;
using NWheels.Core;
using NWheels.Core.Processing;
using NWheels.DataObjects;
using NWheels.DataObjects.Core;
using NWheels.DataObjects.Core.Conventions;
using NWheels.Entities;
using NWheels.Entities.Core;
using NWheels.Exceptions;
using NWheels.Extensions;
using NWheels.Logging;
using NWheels.Logging.Core;
using NWheels.Processing;
using NWheels.Processing.Core;
using NWheels.Utilities;

namespace NWheels.Hosting.Core
{
    public class NodeHost : INodeHost
    {
        private readonly BootConfiguration _bootConfig;
        private readonly DynamicModule _dynamicModule;
        private readonly IContainer _baseContainer;
        private readonly INodeHostLogger _logger;
        private readonly IInitializableHostComponent[] _hostComponents;
        private readonly StateMachine<NodeState, NodeTrigger> _stateMachine;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public NodeHost(BootConfiguration bootConfig, Action<ContainerBuilder> registerHostComponents = null)
        {
            _bootConfig = bootConfig;
            
            _dynamicModule = new DynamicModule(
                simpleName: "NWheels.RunTimeTypes", 
                allowSave: true, 
                saveDirectory: PathUtility.LocalBinPath());

            _baseContainer = BuildBaseContainer(registerHostComponents);

            _hostComponents = InitializeHostComponents();

            _stateMachine = new StateMachine<NodeState, NodeTrigger>(
                new StateMachineCodeBehind(this), 
                _baseContainer.Resolve<Auto<StateMachine<NodeState, NodeTrigger>.ILogger>>());

            _logger = _baseContainer.ResolveAuto<INodeHostLogger>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Load()
        {
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
            using ( _logger.NodeStartingUp() )
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
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void DeactivateAndUnload()
        {
            using ( _logger.NodeShuttingDown() )
            {
                Deactivate();
                Unload();
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

        private IContainer BuildBaseContainer(Action<ContainerBuilder> registerHostComponents)
        {
            var builder = new ContainerBuilder();

            builder.RegisterInstance<NodeHost>(this);
            builder.RegisterInstance<DynamicModule>(_dynamicModule);
            builder.RegisterInstance(_bootConfig).As<INodeConfiguration>();
            builder.RegisterGeneric(typeof(Auto<>)).SingleInstance();
            builder.RegisterType<UniversalThreadLogAnchor>().As<IThreadLogAnchor>().SingleInstance();
            builder.RegisterType<ThreadRegistry>().As<IThreadRegistry, IInitializableHostComponent>().SingleInstance();
            builder.RegisterType<ThreadLogAppender>().As<IThreadLogAppender>().SingleInstance();
            builder.RegisterType<RealFramework>().As<IFramework>().WithParameter(new TypedParameter(typeof(BootConfiguration), _bootConfig)).SingleInstance();
            builder.RegisterType<LoggerObjectFactory>().As<IAutoObjectFactory>().SingleInstance();
            builder.RegisterType<ConfigurationObjectFactory>().As<IAutoObjectFactory>().SingleInstance();
            builder.RegisterType<XmlConfigurationLoader>().SingleInstance().InstancePerLifetimeScope();
            builder.RegisterAdapter<IConfigSectionRegistration, IConfigurationSection>((ctx, reg) => reg.ResolveFromContainer(ctx)).SingleInstance();
            builder.RegisterAdapter<RelationalMappingConventionDefault, IRelationalMappingConvention>(RelationalMappingConventionBase.FromDefault).SingleInstance();
            builder.RegisterConfigSection<IFrameworkLoggingConfiguration>();

            builder.RegisterType<ContractMetadataConvention>().As<IMetadataConvention>().SingleInstance();
            builder.RegisterType<AttributeMetadataConvention>().As<IMetadataConvention>().SingleInstance();
            builder.RegisterType<RelationMetadataConvention>().As<IMetadataConvention>().SingleInstance();
            builder.RegisterInstance(new PascalCaseRelationalMappingConvention(usePluralTableNames: true)).As<IRelationalMappingConvention>();
            builder.RegisterType<MetadataConventionSet>().SingleInstance();
            builder.RegisterType<TypeMetadataCache>().As<ITypeMetadataCache>().SingleInstance();

            if ( registerHostComponents != null )
            {
                registerHostComponents(builder);
            }

            return builder.Build();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private IInitializableHostComponent[] InitializeHostComponents()
        {
            IEnumerable<IInitializableHostComponent> resolved;

            if ( _baseContainer.TryResolve<IEnumerable<IInitializableHostComponent>>(out resolved) )
            {
                var initializableComponents = resolved.ToArray();

                foreach ( var component in initializableComponents )
                {
                    component.Initializing();
                }

                return initializableComponents;
            }

            return new IInitializableHostComponent[0];
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private NodeLifetime CreateNodeLifetime()
        {
            return new NodeLifetime(_bootConfig, _baseContainer, _hostComponents, _logger);
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
                e.ReceiveFeedack(success ? NodeTrigger.LoadSuccess : NodeTrigger.LoadFailure);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void ActivatingEntered(object sender, StateMachineFeedbackEventArgs<NodeState, NodeTrigger> e)
            {
                var success = _lifetime.ExecuteActivatePhase();
                e.ReceiveFeedack(success ? NodeTrigger.ActivateSuccess : NodeTrigger.ActivateFailure);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void DeactivatingEntered(object sender, StateMachineFeedbackEventArgs<NodeState, NodeTrigger> e)
            {
                _lifetime.ExecuteDeactivatePhase();
                e.ReceiveFeedack(NodeTrigger.DeactivateDone);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void UnloadingEntered(object sender, StateMachineFeedbackEventArgs<NodeState, NodeTrigger> e)
            {
                _lifetime.ExecuteUnloadPhase();
                e.ReceiveFeedack(NodeTrigger.UnloadDone);
                _lifetime.Dispose();
                _lifetime = null;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class NodeLifetime : IDisposable, ITenantIdentificationStrategy
        {
            private readonly BootConfiguration _nodeConfig;
            private readonly INodeHostLogger _logger;
            private readonly IInitializableHostComponent[] _hostComponents;
            private readonly ILifetimeScope _lifetimeContainer;
            private readonly List<ILifecycleEventListener> _lifecycleComponents;
            private readonly RevertableSequence _loadSequence;
            private readonly RevertableSequence _activateSequence;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public NodeLifetime(BootConfiguration nodeConfig, IContainer baseContainer, IInitializableHostComponent[] hostComponents, INodeHostLogger logger)
            {
                _nodeConfig = nodeConfig;
                _logger = logger;
                _hostComponents = hostComponents;
                _lifetimeContainer = baseContainer.BeginLifetimeScope();// new MultitenantContainer(this, baseContainer);
                _loadSequence = new RevertableSequence(new LoadSequenceCodeBehind(this));
                _activateSequence = new RevertableSequence(new ActivateSequenceCodeBehind(this));
                _lifecycleComponents = new List<ILifecycleEventListener>();

                LoadModules();
                //FindLifecycleComponents();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void Dispose()
            {
                _lifetimeContainer.Dispose();
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

            public IInitializableHostComponent[] HostComponents
            {
                get { return _hostComponents; }
            }

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
                    foreach ( var module in _nodeConfig.FrameworkModules.Concat(_nodeConfig.ApplicationModules) )
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
                var moduleLoaderType = assembly.GetType(module.LoaderClass, throwOnError: true);

                var loaderTypeUpdater = new ContainerBuilder();
                loaderTypeUpdater.RegisterType(moduleLoaderType);

                foreach ( var feature in module.Features )
                {
                    _logger.RegisteringFeature(feature.Name);

                    var featureLoaderType = assembly.GetType(feature.LoaderClass, throwOnError: true);
                    loaderTypeUpdater.RegisterType(featureLoaderType);
                }

                loaderTypeUpdater.Update(_lifetimeContainer.ComponentRegistry);

                var loaderInstance = (Autofac.Module)_lifetimeContainer.Resolve(moduleLoaderType);

                var moduleUpdater = new ContainerBuilder();
                moduleUpdater.RegisterModule(loaderInstance);
                moduleUpdater.Update(_lifetimeContainer.ComponentRegistry);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private Assembly LoadModuleAssembly(BootConfiguration.ModuleConfig module)
            {
                var simpleAssemblyName = Path.GetFileNameWithoutExtension(module.Assembly);
                var loadedAssembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(asm => asm.GetName().Name.EqualsIgnoreCase(simpleAssemblyName));

                if ( loadedAssembly != null )
                {
                    _logger.AssemblyAlreadyLoaded(simpleAssemblyName);
                    return loadedAssembly;
                }

                try
                {
                    var assemblyName = new AssemblyName() {
                        Name = simpleAssemblyName
                    };
                    return Assembly.Load(assemblyName);
                }
                catch ( Exception e )
                {
                    _logger.AssemblyLoadByNameFailed(simpleAssemblyName, e);
                }

                return LoadModuleAssemblyByFilePath(module);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private Assembly LoadModuleAssemblyByFilePath(BootConfiguration.ModuleConfig module)
            {
                var coreBinPath = PathUtility.LocalBinPath(module.Assembly);
                var appBinPath = Path.Combine(_nodeConfig.LoadedFromDirectory, module.Assembly);

                if ( File.Exists(coreBinPath) )
                {
                    return Assembly.LoadFrom(coreBinPath);
                }
                else if ( File.Exists(appBinPath) )
                {
                    return Assembly.LoadFrom(appBinPath);
                }
                else
                {
                    _logger.ProbedModuleAssemblyLocation(coreBinPath);
                    _logger.ProbedModuleAssemblyLocation(appBinPath);

                    throw new NodeHostConfigException(string.Format("Module assembly '{0}' could not be found at any of the probed locations.", module.Assembly));
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class LoadSequenceCodeBehind : IRevertableSequenceCodeBehind
        {
            private readonly NodeLifetime _ownerLifetime;
            private readonly INodeHostLogger _logger;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public LoadSequenceCodeBehind(NodeLifetime ownerLifetime)
            {
                _ownerLifetime = ownerLifetime;
                _logger = _ownerLifetime.Logger;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void BuildSequence(IRevertableSequenceBuilder sequence)
            {
                sequence.Once().OnPerform(LoadConfiguration);
                sequence.Once().OnPerform(CallHostComponentsConfigured);
                sequence.Once().OnPerform(FindLifecycleComponents);
                sequence.ForEach(GetLifecycleComponents).OnPerform(CallComponentNodeConfigured);
                sequence.ForEach(GetLifecycleComponents).OnPerform(CallComponentNodeLoading).OnRevert(CallComponentNodeUnloaded);
                sequence.ForEach(GetLifecycleComponents).OnPerform(CallComponentLoad).OnRevert(CallComponentUnload);
                sequence.ForEach(GetLifecycleComponents).OnPerform(CallComponentNodeLoaded).OnRevert(CallComponentNodeUnloading);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void LoadConfiguration()
            {
                var loader = _ownerLifetime.LifetimeContainer.Resolve<XmlConfigurationLoader>();
                loader.LoadConfiguration(_ownerLifetime.NodeConfig.ConfigFiles);

                WriteEffectiveConfigurationXml(loader);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private static void WriteEffectiveConfigurationXml(XmlConfigurationLoader loader)
            {
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

                File.WriteAllText(PathUtility.LocalBinPath("effective-config-dump.xml"), effectiveConfigurationXmlText.ToString());
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void CallHostComponentsConfigured()
            {
                foreach ( var component in _ownerLifetime.HostComponents )
                {
                    using ( _logger.HostComponentConfigured(component.GetType().FullName) )
                    {
                        component.Configured();
                    }
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void FindLifecycleComponents()
            {
                using ( _logger.LookingForLifecycleComponents() )
                {
                    try
                    {
                        IEnumerable<ILifecycleEventListener> foundComponents;

                        if ( _ownerLifetime.LifetimeContainer.TryResolve<IEnumerable<ILifecycleEventListener>>(out foundComponents) )
                        {
                            _ownerLifetime.LifecycleComponents.AddRange(foundComponents);

                            foreach ( var component in _ownerLifetime.LifecycleComponents )
                            {
                                _logger.FoundLifecycleComponent(component.GetType().ToString());
                            }
                        }
                        else
                        {
                            _logger.NoLifecycleComponentsFound();
                        }

                        if ( _ownerLifetime.LifecycleComponents.Count == 0 )
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
                return _ownerLifetime.LifecycleComponents;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void CallComponentNodeConfigured(ILifecycleEventListener component, int index, bool isLast)
            {
                using ( _logger.ComponentNodeConfigured(component.GetType().FullName) )
                {
                    component.NodeConfigured();
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void CallComponentNodeLoading(ILifecycleEventListener component, int index, bool isLast)
            {
                using ( _logger.ComponentNodeLoading(component.GetType().FullName) )
                {
                    component.NodeLoading();
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void CallComponentLoad(ILifecycleEventListener component, int index, bool isLast)
            {
                using ( _logger.ComponentLoading(component.GetType().FullName) )
                {
                    component.Load();
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void CallComponentNodeLoaded(ILifecycleEventListener component, int index, bool isLast)
            {
                using ( _logger.ComponentNodeLoaded(component.GetType().FullName) )
                {
                    component.NodeLoaded();
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void CallComponentNodeUnloading(ILifecycleEventListener component, int index, bool isLast)
            {
                using ( _logger.ComponentNodeUnloading(component.GetType().FullName) )
                {
                    component.NodeUnloading();
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void CallComponentUnload(ILifecycleEventListener component, int index, bool isLast)
            {
                using ( _logger.ComponentUnloading(component.GetType().FullName) )
                {
                    component.Unload();
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void CallComponentNodeUnloaded(ILifecycleEventListener component, int index, bool isLast)
            {
                using ( _logger.ComponentNodeUnloaded(component.GetType().FullName) )
                {
                    component.NodeUnloaded();
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class ActivateSequenceCodeBehind : IRevertableSequenceCodeBehind
        {
            private readonly NodeLifetime _ownerLifetime;
            private readonly INodeHostLogger _logger;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ActivateSequenceCodeBehind(NodeLifetime ownerLifetime)
            {
                _ownerLifetime = ownerLifetime;
                _logger = _ownerLifetime.Logger;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void BuildSequence(IRevertableSequenceBuilder sequence)
            {
                sequence.ForEach(GetLifecycleComponents).OnPerform(CallComponentNodeActivating).OnRevert(CallComponentNodeDeactivated);
                sequence.ForEach(GetLifecycleComponents).OnPerform(CallComponentActivate).OnRevert(CallComponentDeactivate);
                sequence.ForEach(GetLifecycleComponents).OnPerform(CallComponentNodeActivated).OnRevert(CallComponentNodeDeactivating);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private IList<ILifecycleEventListener> GetLifecycleComponents()
            {
                return _ownerLifetime.LifecycleComponents;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void CallComponentNodeActivating(ILifecycleEventListener component, int index, bool isLast)
            {
                using ( _logger.ComponentNodeActivating(component.GetType().FullName) )
                {
                    component.NodeActivating();
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void CallComponentActivate(ILifecycleEventListener component, int index, bool isLast)
            {
                using ( _logger.ComponentActivating(component.GetType().FullName) )
                {
                    component.Activate();
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void CallComponentNodeActivated(ILifecycleEventListener component, int index, bool isLast)
            {
                using ( _logger.ComponentNodeActivated(component.GetType().FullName) )
                {
                    component.NodeActivated();
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void CallComponentNodeDeactivating(ILifecycleEventListener component, int index, bool isLast)
            {
                using ( _logger.ComponentNodeDeactivating(component.GetType().FullName) )
                {
                    component.NodeDeactivating();
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void CallComponentDeactivate(ILifecycleEventListener component, int index, bool isLast)
            {
                using ( _logger.ComponentDeactivating(component.GetType().FullName) )
                {
                    component.Deactivate();
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void CallComponentNodeDeactivated(ILifecycleEventListener component, int index, bool isLast)
            {
                using ( _logger.ComponentNodeDeactivated(component.GetType().FullName) )
                {
                    component.NodeDeactivated();
                }
            }
        }
    }
}
