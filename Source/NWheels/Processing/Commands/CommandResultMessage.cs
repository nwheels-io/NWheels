using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NWheels.Authorization;
using NWheels.Extensions;
using NWheels.Processing.Messages;
using NWheels.UI;

namespace NWheels.Processing.Commands
{
    public class CommandResultMessage : AbstractSessionPushMessage, IPromiseFailureInfo
    {
        public CommandResultMessage(
            IFramework framework,
            ISession toSession,
            Guid commandMessageId,
            object result,
            bool success,
            string newSessionId = null,
            string redirectUrl = null,
            string faultType = null,
            string faultCode = null, 
            string faultSubCode = null, 
            string faultReason = null, 
            string technicalInfo = null)
            : base(framework, toSession)
        {
            this.CommandMessageId = commandMessageId;
            this.Result = result;
            this.Success = success;
            this.NewSessionId = newSessionId;
            this.RedirectUrl = redirectUrl;
            this.FaultType = faultType;
            this.FaultCode = faultCode;
            this.FaultSubCode = faultSubCode;
            this.FaultReason = faultReason;
            this.TechnicalInfo = technicalInfo;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override object TakeSerializableSnapshot()
        {
            return new Snapshot(this);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Guid CommandMessageId { get; private set; }
        public object Result { get; private set; }
        public bool Success { get; private set; }
        public string NewSessionId { get; private set; }
        public string RedirectUrl { get; private set; }
        public string FaultType { get; private set; }
        public string FaultCode { get; private set; }
        public string FaultSubCode { get; private set; }
        public string FaultReason { get; private set; }
        public string TechnicalInfo { get; private set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class Snapshot
        {
            public Snapshot(CommandResultMessage source)
            {
                this.Type = source.GetType().SimpleQualifiedName();
                this.CommandMessageId = source.CommandMessageId;
                this.Result = source.Result;
                this.Success = source.Success;
                this.FaultType = source.FaultType;
                this.FaultCode = source.FaultCode;
                this.FaultSubCode = source.FaultSubCode;
                this.FaultReason = source.FaultReason;
                this.TechnicalInfo = source.TechnicalInfo;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            [JsonProperty(PropertyName = "type")]
            public string Type { get; private set; }

            [JsonProperty(PropertyName = "commandMessageId")]
            public Guid CommandMessageId { get; private set; }

            public object Result { get; private set; }

            public bool Success { get; private set; }

            public string FaultType { get; private set; }

            public string FaultCode { get; private set; }

            public string FaultSubCode { get; private set; }

            public string FaultReason { get; private set; }

            public string TechnicalInfo { get; private set; }
            
            [JsonProperty(PropertyName = "result")]
            public object ResultLowercase { get { return this.Result;  } }

            [JsonProperty(PropertyName = "success")]
            public bool SuccessLowercase { get { return this.Success; } }

            [JsonProperty(PropertyName = "faultType")]
            public string FaultTypeLowercase { get { return this.FaultType; } }

            [JsonProperty(PropertyName = "faultCode")]
            public string FaultCodeLowercase { get { return this.FaultCode; } }

            [JsonProperty(PropertyName = "faultSubCode")]
            public string FaultSubCodeLowercase { get { return this.FaultSubCode; } }

            [JsonProperty(PropertyName = "faultReason")]
            public string FaultReasonLowercase { get { return this.FaultReason; } }

            [JsonProperty(PropertyName = "technicalInfo")]
            public string TechnicalInfoLowercase { get { return this.TechnicalInfo; } }
        }
    }
}
