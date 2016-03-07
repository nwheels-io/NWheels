using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;

namespace NWheels.Stacks.Endpoints.Wcf.ServiceHostComponents
{
    public class LoggingMessageInspector : IDispatchMessageInspector
    {
        private readonly IWcfServiceLogger _logger;

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public LoggingMessageInspector(IWcfServiceLogger logger)
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
}