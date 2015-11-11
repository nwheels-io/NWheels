using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using NWheels.Authorization;
using NWheels.Core;
using NWheels.Exceptions;
using NWheels.Extensions;
using NWheels.Processing.Messages;

namespace NWheels.Processing.Commands.Impl
{
    public class CommandActor : 
        IMessageHandler<EntityChangeSetCommandMessage>,
        IMessageHandler<EntityMethodCommandMessage>,
        IMessageHandler<TransactionScriptCommandMessage>,
        IMessageHandler<ServiceMethodCommandMessage>
    {
        private readonly IComponentContext _components;
        private readonly IFramework _framework;
        private readonly ICommandActorLogger _logger;
        private readonly IServiceBus _serviceBus;
        private readonly ISessionManager _sessionManager;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public CommandActor(
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

        #region Implementation of IMessageHandler<EntityChangeSetCommandMessage>

        public void HandleMessage(EntityChangeSetCommandMessage message)
        {
            throw new NotImplementedException();
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of IMessageHandler<EntityMethodCommandMessage>

        public void HandleMessage(EntityMethodCommandMessage message)
        {
            ExecuteCommand(message, ExecuteEntityMethodCommand);
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of IMessageHandler<TransactionScriptCommandMessage>

        public void HandleMessage(TransactionScriptCommandMessage message)
        {
            ExecuteCommand(message, ExecuteTransactionScriptCommand);
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of IMessageHandler<ServiceMethodCommandMessage>

        public void HandleMessage(ServiceMethodCommandMessage message)
        {
            ExecuteCommand(message, ExecuteServiceMethodCommand);
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void ExecuteCommand<TConcreteCommand>(TConcreteCommand command, Func<TConcreteCommand, object> concreteExecutor)
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

        private object ExecuteEntityMethodCommand(EntityMethodCommandMessage command)
        {
            var entityContract = command.EntityId.ContractType;

            using ( var context = _framework.As<ICoreFramework>().NewUnitOfWorkForEntity(entityContract) )
            {
                _logger.LookingUpEntity(id: command.EntityId);

                var entityInstance = context.GetEntityRepository(entityContract).TryGetById(command.EntityId);

                if ( entityInstance != null )
                {
                    command.Call.ExecuteOn(entityInstance);
                    return command.Call.Result;
                }
                else
                {
                    throw _logger.EntityNotFound(id: command.EntityId);
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private object ExecuteTransactionScriptCommand(TransactionScriptCommandMessage command)
        {
            var scriptInstance = _components.Resolve(command.TransactionScriptType);
            command.Call.ExecuteOn(scriptInstance);
            return command.Call.Result;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private object ExecuteServiceMethodCommand(ServiceMethodCommandMessage command)
        {
            var serviceInstance = _components.Resolve(command.ServiceContract);
            command.Call.ExecuteOn(serviceInstance);
            return command.Call.Result;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void EnqueueSuccessfulCommandResult(AbstractCommandMessage command, object resultValue, string newSessionId)
        {
            var resultMessage = new CommandResultMessage(
                _framework,
                command.Session,
                command.MessageId,
                resultValue,
                success: true,
                newSessionId: newSessionId);

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
                faultCode: fault != null ? fault.FaultCode : "InternalError",
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
