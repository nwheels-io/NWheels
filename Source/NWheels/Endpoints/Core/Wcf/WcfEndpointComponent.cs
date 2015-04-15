using System;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Xml;
using Autofac;
using Autofac.Core;
using Autofac.Core.Activators.Reflection;
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
        private readonly Type _serviceType;
        private ServiceHost _serviceHost;

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

            if ( !_components.TryGetImplementationType(_contractType, out _serviceType) )
            {
                throw new ConfigurationErrorsException("Could not find registered implementation of contract: " + _contractType.ToString());
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void NodeActivated()
        {
            _endpointRegistration.ApplyConfiguration(_config);
            _serviceHost = new ServiceHost(_serviceType);

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

            _serviceHost.Description.Behaviors.Add(new AutofacInstanceProviderBehavior(_components, _serviceType));
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

        public class AutofacInstanceProvider : IInstanceProvider
        {
            private Type _serviceType;
            private IComponentContext _components;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public AutofacInstanceProvider(Type serviceType, IComponentContext components)
            {
                _serviceType = serviceType;
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
                return _components.Resolve(_serviceType);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void ReleaseInstance(InstanceContext instanceContext, object instance)
            {
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class AutofacInstanceProviderBehavior : IServiceBehavior
        {
            private readonly IComponentContext _components;
            private readonly Type _serviceType;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public AutofacInstanceProviderBehavior(IComponentContext components, Type serviceType)
            {
                _components = components;
                _serviceType = serviceType;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase,
                Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters)
            {
            }
 
            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
            {
                var instanceProvider = new AutofacInstanceProvider(_serviceType, _components);
 
                foreach( var dispatcher in serviceHostBase.ChannelDispatchers.OfType<ChannelDispatcher>() )
                {
                    foreach ( var endpointDispatcher in dispatcher.Endpoints )
                    {
                        DispatchRuntime dispatchRuntime = endpointDispatcher.DispatchRuntime;
                        dispatchRuntime.InstanceProvider = instanceProvider;
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

            [LogThread(ThreadTaskType.IncomingRequest)]
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
                OperationContext.Current.Extensions.Add(new LogOperationExtension(activity));

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
                var logExtension = OperationContext.Current.Extensions.Find<LogOperationExtension>();

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

        private class LogOperationExtension : IExtension<OperationContext>
        {
            public LogOperationExtension(ILogActivity activity)
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
