using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Authorization;
using NWheels.Processing.Commands;
using NWheels.Processing.Messages;

namespace NWheels.Processing.Documents
{
    public class DocumentFormatReplyMessage : CommandResultMessage
    {
        public DocumentFormatReplyMessage(
            IFramework framework, 
            ISession toSession, 
            Guid commandMessageId, 
            FormattedDocument document)
            : base(
                framework, 
                toSession, 
                commandMessageId,
                result: commandMessageId.ToString("N"), 
                success: true, 
                newSessionId: null, 
                faultCode: null, 
                faultSubCode: null, 
                faultReason:null , 
                technicalInfo: null)
        {
            this.Document = document;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public FormattedDocument Document { get; set; }
    }
}
