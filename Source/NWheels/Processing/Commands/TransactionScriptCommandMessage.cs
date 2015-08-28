using System;
using System.Collections.Generic;
using System.Security.Principal;
using NWheels.Authorization;
using NWheels.Processing.Commands.Impl;
using NWheels.Processing.Messages;

namespace NWheels.Processing.Commands
{
    public class TransactionScriptCommandMessage : ServiceMethodCommandMessage
    {
        public TransactionScriptCommandMessage(IFramework framework, ISession session, Type transactionScriptType, IMethodCallObject call)
            : base(framework, session, call)
        {
            this.TransactionScriptType = transactionScriptType;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Type TransactionScriptType { get; private set; }
    }
}
