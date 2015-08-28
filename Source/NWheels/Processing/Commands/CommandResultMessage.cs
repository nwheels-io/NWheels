using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Authorization;
using NWheels.Extensions;
using NWheels.Processing.Messages;

namespace NWheels.Processing.Commands
{
    public class CommandResultMessage : AbstractSessionPushMessage
    {
        public CommandResultMessage(
            IFramework framework,
            ISession toSession,
            Guid commandMessageId, 
            bool success, 
            string faultCode = null, 
            string faultSubCode = null, 
            string faultReason = null)
            : base(framework, toSession)
        {
            CommandMessageId = commandMessageId;
            Success = success;
            FaultCode = faultCode;
            FaultSubCode = faultSubCode;
            FaultReason = faultReason;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override object TakeSerializableSnapshot()
        {
            return new Snapshot(this);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Guid CommandMessageId { get; private set; }
        public bool Success { get; private set; }
        public string FaultCode { get; private set; }
        public string FaultSubCode { get; private set; }
        public string FaultReason { get; private set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class Snapshot
        {
            public Snapshot(CommandResultMessage source)
            {
                this.Type = source.GetType().SimpleQualifiedName();
                this.CommandMessageId = source.CommandMessageId;
                this.Success = source.Success;
                this.FaultCode = source.FaultCode;
                this.FaultSubCode = source.FaultSubCode;
                this.FaultReason = source.FaultReason;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public string Type { get; private set; }
            public Guid CommandMessageId { get; private set; }
            public bool Success { get; private set; }
            public string FaultCode { get; private set; }
            public string FaultSubCode { get; private set; }
            public string FaultReason { get; private set; }
        }
    }
}
