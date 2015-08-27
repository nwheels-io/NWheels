using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using NWheels.Core;
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

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public CommandActor(IComponentContext components, IFramework framework, ICommandActorLogger logger)
        {
            _components = components;
            _framework = framework;
            _logger = logger;
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
            var entityContract = message.EntityId.ContractType;

            using ( var context = _framework.As<ICoreFramework>().NewUnitOfWorkForEntity(entityContract) )
            {
                _logger.LookingUpEntity(id: message.EntityId);
                
                var entityInstance = context.GetEntityRepository(entityContract).TryGetById(message.EntityId);

                if ( entityInstance != null )
                {
                    using ( var activity = _logger.ExecutingEntityMethod(entity: entityInstance, method: message.EntityMethod) )
                    {
                        try
                        {
                            message.Call.ExecuteOn(entityInstance);
                        }
                        catch ( Exception e )
                        {
                            activity.Fail(e);
                            _logger.EntityMethodFailed(entity: entityInstance, method: message.EntityMethod, error: e);
                            throw;
                        }
                    }
                }
                else
                {
                    throw _logger.EntityNotFound(id: message.EntityId);
                }
            }
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of IMessageHandler<TransactionScriptCommandMessage>

        public void HandleMessage(TransactionScriptCommandMessage message)
        {
            var scriptInstance = _components.Resolve(message.TransactionScriptType);

            using ( var activity = _logger.ExecutingTransactionScript(transactionScriptType: message.TransactionScriptType) )
            {
                try
                {
                    message.Call.ExecuteOn(scriptInstance);
                }
                catch ( Exception e )
                {
                    activity.Fail(e);
                    _logger.TransactionScriptFailed(transactionScriptType: message.TransactionScriptType, error: e);
                    throw;
                }
            }
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of IMessageHandler<ServiceMethodCommandMessage>

        public void HandleMessage(ServiceMethodCommandMessage message)
        {
            var serviceInstance = _components.Resolve(message.ServiceContract);
            
            using ( var activity = _logger.ExecutingServiceMethod(serviceContract: message.ServiceContract, method: message.ServiceMethod) )
            {
                try
                {
                    message.Call.ExecuteOn(serviceInstance);
                }
                catch ( Exception e )
                {
                    activity.Fail(e);
                    _logger.ServiceMethodFailed(serviceContract: message.ServiceContract, method: message.ServiceMethod, error: e);
                    throw;
                }
            }
        }

        #endregion
    }
}
