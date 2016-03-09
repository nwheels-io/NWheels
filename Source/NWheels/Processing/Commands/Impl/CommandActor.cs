using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using NWheels.Authorization;
using NWheels.Core;
using NWheels.DataObjects.Core;
using NWheels.Entities;
using NWheels.Extensions;
using NWheels.Processing.Messages;

namespace NWheels.Processing.Commands.Impl
{
    public class CommandActor : 
        CommandActorBase, 
        IMessageHandler<EntityChangeSetCommandMessage>,
        IMessageHandler<EntityMethodCommandMessage>,
        IMessageHandler<TransactionScriptCommandMessage>,
        IMessageHandler<ServiceMethodCommandMessage>
    {
        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public CommandActor(
            IComponentContext components, 
            IFramework framework, 
            ICommandActorLogger logger, 
            IServiceBus serviceBus, 
            ISessionManager sessionManager)
            : base(components, framework, logger, serviceBus, sessionManager)
        {
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

        private object ExecuteEntityMethodCommand(EntityMethodCommandMessage command)
        {
            var entityContract = command.EntityId.ContractType;

            using ( var context = Framework.As<ICoreFramework>().NewUnitOfWork(command.DomainContextContract) )
            {
                Logger.LookingUpEntity(id: command.EntityId, contextType: command.DomainContextContract.Name);

                var entityRepo = context.GetEntityRepository(entityContract);
                var entityInstance = entityRepo.TryGetById(command.EntityId);

                if ( entityInstance != null )
                {
                    command.Call.ExecuteOn(entityInstance);

                    //TODO: fix IsModified to take inherited properties into account - Hapil? inherited properties are not decorated? DomainObjectFactory
                    //if (((IObject)entityInstance).IsModified)
                    //{
                        entityRepo.Save(entityInstance);
                    //}

                    context.CommitChanges();
                    return command.Call.Result;
                }
                else
                {
                    throw Logger.EntityNotFound(id: command.EntityId);
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private object ExecuteTransactionScriptCommand(TransactionScriptCommandMessage command)
        {
            var scriptInstance = Components.Resolve(command.TransactionScriptType);
            command.Call.ExecuteOn(scriptInstance);
            return command.Call.Result;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private object ExecuteServiceMethodCommand(ServiceMethodCommandMessage command)
        {
            var serviceInstance = Components.Resolve(command.ServiceContract);
            command.Call.ExecuteOn(serviceInstance);
            return command.Call.Result;
        }
    }
}
