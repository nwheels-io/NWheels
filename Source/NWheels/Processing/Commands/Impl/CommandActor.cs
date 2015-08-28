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

        private void ExecuteCommand<TConcreteCommand>(TConcreteCommand command, Action<TConcreteCommand> concreteExecutor)
            where TConcreteCommand : AbstractCommandMessage
        {
            using ( var activity = _logger.ExecutingCommand(command, command.Session.Endpoint, command.Session, command.Session.UserPrincipal) )
            {
                try
                {
                    using ( _sessionManager.JoinSession(command.Session.Id) )
                    {
                        concreteExecutor(command);
                    }
                    
                    EnqueueSuccessfulCommandResult(command);
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

        private void ExecuteEntityMethodCommand(EntityMethodCommandMessage command)
        {
            var entityContract = command.EntityId.ContractType;

            using ( var context = _framework.As<ICoreFramework>().NewUnitOfWorkForEntity(entityContract) )
            {
                _logger.LookingUpEntity(id: command.EntityId);

                var entityInstance = context.GetEntityRepository(entityContract).TryGetById(command.EntityId);

                if ( entityInstance != null )
                {
                    command.Call.ExecuteOn(entityInstance);
                    EnqueueSuccessfulCommandResult(command);
                }
                else
                {
                    throw _logger.EntityNotFound(id: command.EntityId);
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void ExecuteTransactionScriptCommand(TransactionScriptCommandMessage command)
        {
            var scriptInstance = _components.Resolve(command.TransactionScriptType);
            command.Call.ExecuteOn(scriptInstance);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void ExecuteServiceMethodCommand(ServiceMethodCommandMessage command)
        {
            var serviceInstance = _components.Resolve(command.ServiceContract);
            command.Call.ExecuteOn(serviceInstance);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void EnqueueSuccessfulCommandResult(AbstractCommandMessage command)
        {
            var result = new CommandResultMessage(
                _framework,
                command.Session,
                command.MessageId,
                success: true);

            _serviceBus.EnqueueMessage(result);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void EnqueueFailedCommandResult(AbstractCommandMessage command, Exception error)
        {
            var fault = error as IFaultException;

            var result = new CommandResultMessage(
                _framework, 
                command.Session,
                command.MessageId, 
                success: false, 
                faultCode: fault != null ? fault.FaultCode : "InternalError",
                faultSubCode: fault != null ? fault.FaultSubCode : string.Empty,
                faultReason: fault != null ? fault.FaultReason : "Request failed due to an internal error.");
            
            _serviceBus.EnqueueMessage(result);
        }
    }
}
