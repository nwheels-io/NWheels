using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Threading;
using System.Xml;
using Autofac;
using Autofac.Core;
using Autofac.Core.Activators.Reflection;
using Hapil;
using NWheels.Extensions;
using NWheels.Hosting;
using NWheels.Logging;

namespace NWheels.Endpoints.Core.Wcf
{
    public class WcfEndpointComponent : LifecycleEventListenerBase
    {
        private readonly SoapApiEndpointRegistration _endpointRegistration;
        private readonly IFrameworkEndpointsConfig _config;
        private readonly ILogger _logger;
        private readonly IComponentContext _components;
        private readonly Type _contractType;
        private InjectionEnabledServiceHost _serviceHost;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public WcfEndpointComponent(
            SoapApiEndpointRegistration endpointRegistration, 
            Auto<IFrameworkEndpointsConfig> config, 
            Auto<ILogger> logger, 
            IComponentContext components)
        {
            _endpointRegistration = endpointRegistration;
            _config = config.Instance;
            _logger = logger.Instance;
            _components = components;
            _contractType = _endpointRegistration.Contract;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void NodeActivated()
        {
            _endpointRegistration.ApplyConfiguration(_config);
            _serviceHost = new InjectionEnabledServiceHost(_components, _contractType);

            ConfigureApiEndpoint();

            if ( _endpointRegistration.ShouldPublishMetadata )
            {
                ConfigureMetadataEndpoint();
            }

            ConfigureExceptionDetails();

            _serviceHost.Open();
            _logger.ServiceHostOpen(listenUrl: _endpointRegistration.Address.ToString(), metadataUrl: _endpointRegistration.MetadataAddress.ToString());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void NodeDeactivating()
        {
            _serviceHost.Close(TimeSpan.FromSeconds(10));
            _logger.ServiceHostClosed(listenUrl: _endpointRegistration.Address.ToString(), metadataUrl: _endpointRegistration.MetadataAddress.ToString());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void ConfigureExceptionDetails()
        {
            var debugBehavior = _serviceHost.Description.Behaviors.Find<ServiceDebugBehavior>();

            if ( debugBehavior == null )
            {
                debugBehavior = new ServiceDebugBehavior();
                _serviceHost.Description.Behaviors.Add(debugBehavior);
            }

            debugBehavior.IncludeExceptionDetailInFaults = _endpointRegistration.ShouldExposeExceptionDetails;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void ConfigureApiEndpoint()
        {
            var apiEndpoint = new ServiceEndpoint(
                ContractDescription.GetContract(_endpointRegistration.Contract),
                new BasicHttpBinding() {
                    ReaderQuotas = XmlDictionaryReaderQuotas.Max,
                    MaxReceivedMessageSize = 10 * 1024 * 1024
                },
                new EndpointAddress(_endpointRegistration.Address));

            _serviceHost.AddServiceEndpoint(apiEndpoint);
            apiEndpoint.EndpointBehaviors.Add(new MessageLogBehavior(new LoggingMessageInspector(_logger)));

            _serviceHost.Description.Behaviors.Add(new ErrorHandlerBehavior(new ErrorHandler(_logger)));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void ConfigureMetadataEndpoint()
        {
            var metadataBehavior = new ServiceMetadataBehavior();
            
            metadataBehavior.HttpGetEnabled = true;
            metadataBehavior.HttpGetUrl = _endpointRegistration.MetadataAddress;
            
            _serviceHost.Description.Behaviors.Add(metadataBehavior);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class InjectionEnabledServiceHost : ServiceHostBase
        {
            private readonly IComponentContext _components;
            private readonly Type _serviceContractType;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public InjectionEnabledServiceHost(IComponentContext components, Type serviceContractType)
                : base()
            {
                _components = components;
                _serviceContractType = serviceContractType;

                InitializeDescription(new UriSchemeKeyedCollection());
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected override ServiceDescription CreateDescription(out IDictionary<string, ContractDescription> implementedContracts)
            {
                Type serviceType;

                if ( !_components.TryGetImplementationType(_serviceContractType, out serviceType) )
                {
                    throw new ConfigurationErrorsException("Could not find registered implementation of contract: " + _serviceContractType.ToString());
                }

                var contractDescription = ContractDescription.GetContract(_serviceContractType);

                implementedContracts = new Dictionary<string, ContractDescription> {
                    { contractDescription.ConfigurationName, contractDescription }
                };

                var serviceDescription = new ServiceDescription {
                    ServiceType = serviceType,
                    ConfigurationName = serviceType.FullName,
                    Name = _serviceContractType.Name.TrimPrefix("I"),
                    Namespace = _serviceContractType.Namespace,
                };

                serviceDescription.Behaviors.Add(new InjectionEnabledInstancingBehavior(_components, _serviceContractType));
                
                return serviceDescription;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class InjectionEnabledInstanceProvider : IInstanceProvider
        {
            private Type _serviceContractType;
            private IComponentContext _components;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public InjectionEnabledInstanceProvider(Type serviceContractType, IComponentContext components)
            {
                _serviceContractType = serviceContractType;
                _components = components;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public object GetInstance(InstanceContext instanceContext)
            {
                return this.GetInstance(instanceContext, message: null);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public object GetInstance(InstanceContext instanceContext, Message message)
            {
                return _components.Resolve(_serviceContractType);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void ReleaseInstance(InstanceContext instanceContext, object instance)
            {
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class PerCallInstanceContextProvider : IInstanceContextProvider
        {
            public InstanceContext GetExistingInstanceContext(Message message, IContextChannel channel)
            {
                return null;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void InitializeInstanceContext(InstanceContext instanceContext, Message message, IContextChannel channel)
            {
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public bool IsIdle(InstanceContext instanceContext)
            {
                return true;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void NotifyIdle(InstanceContextIdleCallback callback, InstanceContext instanceContext)
            {
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class SingletonInstanceContextProvider : IInstanceContextProvider
        {
            private readonly ServiceHostBase _serviceHost;
            private readonly object _instanceContextSyncRoot = new object();
            private InstanceContext _instanceContext;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public SingletonInstanceContextProvider(ServiceHostBase serviceHost)
            {
                _serviceHost = serviceHost;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public InstanceContext GetExistingInstanceContext(Message message, IContextChannel channel)
            {
                if ( _instanceContext == null )
                {
                    if ( !Monitor.TryEnter(_instanceContextSyncRoot, timeout: TimeSpan.FromSeconds(10)) )
                    {
                        throw new TimeoutException("Could not acquire lock on instance context withing allotted timeout.");
                    }

                    if ( _instanceContext != null )
                    {
                        Monitor.Exit(_instanceContextSyncRoot);
                    }
                }

                return _instanceContext;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void InitializeInstanceContext(InstanceContext instanceContext, Message message, IContextChannel channel)
            {
                _instanceContext = instanceContext;
                Monitor.Exit(_instanceContextSyncRoot);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public bool IsIdle(InstanceContext instanceContext)
            {
                return false;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void NotifyIdle(InstanceContextIdleCallback callback, InstanceContext instanceContext)
            {
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class InjectionEnabledInstancingBehavior : IServiceBehavior
        {
            private readonly IComponentContext _components;
            private readonly Type _serviceContractType;
            private readonly bool _isSingleton;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public InjectionEnabledInstancingBehavior(IComponentContext components, Type serviceContractType)
            {
                _components = components;
                _serviceContractType = serviceContractType;
                _isSingleton = components.IsServiceRegisteredAsSingleton(_serviceContractType);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase,
                Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters)
            {
            }
 
            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
            {
                var instanceProvider = new InjectionEnabledInstanceProvider(_serviceContractType, _components);
                IInstanceContextProvider instanceContextProvider;

                if ( _isSingleton )
                {
                    instanceContextProvider = new SingletonInstanceContextProvider(serviceHostBase);
                }
                else
                {
                    instanceContextProvider = new PerCallInstanceContextProvider();
                }

                foreach( var dispatcher in serviceHostBase.ChannelDispatchers.OfType<ChannelDispatcher>() )
                {
                    foreach ( var endpointDispatcher in dispatcher.Endpoints )
                    {
                        DispatchRuntime dispatchRuntime = endpointDispatcher.DispatchRuntime;
                        dispatchRuntime.InstanceProvider = instanceProvider;
                        dispatchRuntime.InstanceContextProvider = instanceContextProvider;
                    }
                }
            }
 
            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
            {
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface ILogger : IApplicationEventLogger
        {
            [LogInfo]
            void ServiceHostOpen(string listenUrl, string metadataUrl);

            [LogInfo]
            void ServiceHostClosed(string listenUrl, string metadataUrl);

            [LogThread(ThreadTaskType.ApiRequest)]
            ILogActivity HandlingRequest(string action);

            [LogDebug]
            void IncomingRequest(
                [Detail(ContentTypes = LogContentTypes.CommunicationMessage, MaxStringLength = 4096, IncludeInSingleLineText = false)] 
                string requestXml);

            [LogDebug]
            void OutgoingResponse(
                [Detail(ContentTypes = LogContentTypes.CommunicationMessage, MaxStringLength = 4096, IncludeInSingleLineText = false)] 
                string responseXml);

            [LogError]
            void FaultResponse(
                [Detail(ContentTypes = LogContentTypes.CommunicationMessage, MaxStringLength = 4096, IncludeInSingleLineText = false)] 
                string responseXml);

            [LogError]
            void OperationFailed(Exception error);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class LoggingMessageInspector : IDispatchMessageInspector
        {
            private readonly ILogger _logger;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public LoggingMessageInspector(ILogger logger)
            {
                _logger = logger;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            object IDispatchMessageInspector.AfterReceiveRequest(ref System.ServiceModel.Channels.Message request, IClientChannel channel, InstanceContext instanceContext)
            {
                var activity = _logger.HandlingRequest(request.Headers.Action);
                OperationContext.Current.Extensions.Add(new ThreadLogOperationContextExtension(activity));

                MessageBuffer buffer = request.CreateBufferedCopy(Int32.MaxValue);
                request = buffer.CreateMessage();
                var requestXml = buffer.CreateMessage().ToString();
                var requestCopy = buffer.CreateMessage();

                System.Xml.XmlDictionaryReader xrdr = requestCopy.GetReaderAtBodyContents();
                string bodyData = xrdr.ReadOuterXml();

                // Replace the body placeholder with the actual SOAP body.
                requestXml = requestXml.Replace("... stream ...", bodyData);
                _logger.IncomingRequest(requestXml);

                return null;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            void IDispatchMessageInspector.BeforeSendReply(ref System.ServiceModel.Channels.Message reply, object correlationState)
            {
                var logExtension = OperationContext.Current.Extensions.Find<ThreadLogOperationContextExtension>();

                if ( logExtension != null )
                {
                    MessageBuffer buffer = reply.CreateBufferedCopy(Int32.MaxValue);
                    reply = buffer.CreateMessage();
                    var responseXml = buffer.CreateMessage().ToString();

                    if ( reply.IsFault )
                    {
                        _logger.FaultResponse(responseXml);
                    }
                    else
                    {
                        _logger.OutgoingResponse(responseXml);
                    }

                    logExtension.Activity.Dispose();
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class ThreadLogOperationContextExtension : IExtension<OperationContext>
        {
            public ThreadLogOperationContextExtension(ILogActivity activity)
            {
                Activity = activity;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------
            
            public void Attach(OperationContext owner)
            {
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void Detach(OperationContext owner)
            {
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ILogActivity Activity { get; private set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class MessageLogBehavior : IEndpointBehavior
        {
            private readonly LoggingMessageInspector _messageInspector;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public MessageLogBehavior(LoggingMessageInspector messageInspector)
            {
                _messageInspector = messageInspector;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void AddBindingParameters(ServiceEndpoint endpoint, System.ServiceModel.Channels.BindingParameterCollection bindingParameters)
            {
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------
            
            public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
            {
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------
            
            public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
            {
                endpointDispatcher.DispatchRuntime.MessageInspectors.Add(_messageInspector);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------
            
            public void Validate(ServiceEndpoint endpoint)
            {
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class ErrorHandler : IErrorHandler
        {
            private readonly ILogger _logger;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ErrorHandler(ILogger logger)
            {
                _logger = logger;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public bool HandleError(Exception error)
            {
                _logger.OperationFailed(error);
                return true;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void ProvideFault(Exception error, MessageVersion version, ref Message fault)
            {
                FaultException faultException = new FaultException("Server error encountered. All details have been logged.");
                MessageFault messageFault = faultException.CreateMessageFault();

                fault = Message.CreateMessage(version, messageFault, faultException.Action);
            }
        }
        
        //-------------------------------------------------------------------------------------------------------------------------------------------------

        private class ErrorHandlerBehavior : IServiceBehavior
        {
            private readonly ErrorHandler _handler;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ErrorHandlerBehavior(ErrorHandler handler)
            {
                _handler = handler;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void AddBindingParameters(
                ServiceDescription serviceDescription,
                ServiceHostBase serviceHostBase,
                Collection<ServiceEndpoint> endpoints,
                BindingParameterCollection bindingParameters)
            {
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
            {
                foreach ( ChannelDispatcherBase channelDispatcherBase in serviceHostBase.ChannelDispatchers )
                {
                    ChannelDispatcher channelDispatcher = channelDispatcherBase as ChannelDispatcher;

                    if ( channelDispatcher != null )
                    {
                        channelDispatcher.ErrorHandlers.Add(_handler);
                    }
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
            {
            }
        }
    }
}
