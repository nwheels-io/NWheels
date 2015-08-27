using System;
using System.Collections.Generic;
using NWheels.Processing.Commands.Impl;
using NWheels.Processing.Messages;

namespace NWheels.Processing.Commands
{
    public class TransactionScriptCommandMessage : ServiceMethodCommandMessage
    {
        public TransactionScriptCommandMessage(Type transactionScriptType, IMethodCallObject call)
            : base(call)
        {
            this.TransactionScriptType = transactionScriptType;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Type TransactionScriptType { get; private set; }
    }
}
