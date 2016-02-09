using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Principal;
using NWheels.Authorization;
using NWheels.Processing.Commands.Impl;
using NWheels.Processing.Messages;

namespace NWheels.Processing.Commands
{
    public class TransactionScriptCommandMessage : ServiceMethodCommandMessage
    {
        public TransactionScriptCommandMessage(IFramework framework, ISession session, Type transactionScriptType, IMethodCallObject call, bool synchronous)
            : base(framework, session, call, synchronous)
        {
            this.TransactionScriptType = transactionScriptType;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override bool CheckAuthorization(out bool authenticationRequired)
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Type TransactionScriptType { get; private set; }

        #region Overrides of AbstractCommandMessage

        public override string AuditName
        {
            get
            {
                var attribute = TransactionScriptType.GetCustomAttribute<TransactionScriptAttribute>();

                if (attribute != null)
                {
                    return attribute.AuditName;
                }
                else
                {
                    return base.AuditName;
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override string CommandString
        {
            get
            {
                return string.Format("InvokeTransactionScript({0}.{1})", TransactionScriptType.FullName, ServiceMethod.Name);
            }
        }

        #endregion
    }
}
