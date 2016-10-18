using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;

namespace NWheels.Stacks.Endpoints.Wcf.ServiceHostComponents
{
    public class ErrorHandler : IErrorHandler
    {
        private readonly IWcfServiceLogger _logger;

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public ErrorHandler(IWcfServiceLogger logger)
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
}