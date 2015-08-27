using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NWheels.Entities;
using NWheels.Exceptions;
using NWheels.Logging;

namespace NWheels.Processing.Commands.Impl
{
    public interface ICommandActorLogger : IApplicationEventLogger
    {
        [LogDebug]
        void LookingUpEntity(IEntityId id);

        [LogError]
        EntityNotFoundException EntityNotFound(IEntityId id);

        [LogActivity]
        ILogActivity ExecutingEntityMethod(object entity, MethodInfo method);

        [LogActivity]
        ILogActivity ExecutingTransactionScript(Type transactionScriptType);

        [LogActivity]
        ILogActivity ExecutingServiceMethod(Type serviceContract, MethodInfo method);

        [LogError]
        void EntityMethodFailed(object entity, MethodInfo method, Exception error);

        [LogError]
        void TransactionScriptFailed(Type transactionScriptType, Exception error);

        [LogError]
        void ServiceMethodFailed(Type serviceContract, MethodInfo method, Exception error);
    }
}
