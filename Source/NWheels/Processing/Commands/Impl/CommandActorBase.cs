using System;
using Autofac;
using NWheels.Authorization;
using NWheels.Exceptions;
using NWheels.Processing.Messages;

namespace NWheels.Processing.Commands.Impl
{
    public abstract class CommandActorBase
    {
        private readonly IComponentContext _components;
        private readonly IFramework _framework;
        private readonly ICommandActorLogger _logger;
        private readonly IServiceBus _serviceBus;
        private readonly ISessionManager _sessionManager;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected CommandActorBase(
            IComponentContext components, 
            IFramework framework, 
            ICommandActorLogger logger, 
            IServiceBus serviceBus, 
            ISessionManager sessionManager)
        {
            _components = components;
            _framework = framework;
            _logger = logger;
            _serviceBus = serviceBus;
            _sessionManager = sessionManager;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected void ExecuteCommand<TConcreteCommand>(TConcreteCommand command, Func<TConcreteCommand, object> concreteExecutor)
            where TConcreteCommand : AbstractCommandMessage
        {
            using ( var activity = _logger.ExecutingCommand(command, command.Session.Endpoint, command.Session, command.Session.UserPrincipal) )
            {
                try
                {
                    object resultValue;
                    string newSessionId = null;

                    using ( _sessionManager.JoinSession(command.Session.Id) )
                    {
                        resultValue = concreteExecutor(command);

                        if ( _sessionManager.CurrentSession.Id != command.Session.Id )
                        {
                            newSessionId = _sessionManager.CurrentSession.Id;
                        }
                    }

                    EnqueueSuccessfulCommandResult(command, resultValue, newSessionId);
                }
                catch ( Exception e )
                {
                    activity.Fail(e);
                    _logger.CommandFailed(command, command.Session.Endpoint, command.Session, command.Session.UserPrincipal, error: e);

                    EnqueueFailedCommandResult(command, e);

                    throw;
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected IComponentContext Components
        {
            get { return _components; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        protected ICommandActorLogger Logger
        {
            get { return _logger; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        protected IServiceBus ServiceBus
        {
            get { return _serviceBus; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        protected ISessionManager SessionManager
        {
            get { return _sessionManager; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected IFramework Framework
        {
            get { return _framework; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void EnqueueSuccessfulCommandResult(AbstractCommandMessage command, object resultValue, string newSessionId)
        {
            var returnValueResultMessage = (resultValue as CommandResultMessage);
            CommandResultMessage resultMessage;

            if (returnValueResultMessage != null)
            {
                resultMessage = returnValueResultMessage.Mutate(_framework, command.Session, command.MessageId);
            }
            else
            { 
                resultMessage = new CommandResultMessage(
                    _framework,
                    command.Session,
                    command.MessageId,
                    resultValue,
                    success: true,
                    newSessionId: newSessionId);
            }

            ReturnResultMessage(command, resultMessage);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void EnqueueFailedCommandResult(AbstractCommandMessage command, Exception error)
        {
            var fault = error as IFaultException;

            var resultMessage = new CommandResultMessage(
                _framework, 
                command.Session,
                command.MessageId,
                result: null,
                success: false,
                faultType: fault != null ? fault.FaultType: "InternalError",
                faultCode: fault != null ? fault.FaultCode : "Exception",
                faultSubCode: fault != null ? fault.FaultSubCode : string.Empty,
                faultReason: fault != null ? fault.FaultReason : "Request failed due to an internal error.");

            ReturnResultMessage(command, resultMessage);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void ReturnResultMessage(AbstractCommandMessage command, CommandResultMessage resultMessage)
        {
            if ( command.IsSynchronous )
            {
                command.Result = resultMessage;
            }
            else
            {
                _serviceBus.EnqueueMessage(resultMessage);
            }
        }
    }
}