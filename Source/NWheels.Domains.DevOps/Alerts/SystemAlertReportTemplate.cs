using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Processing.Messages;

namespace NWheels.Domains.DevOps.Alerts
{
    public static class SystemAlertReportTemplate
    {
        public enum TemplateType
        {
            AlertsGroupedPerRecipient
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class AlertDetails
        {
            public string AlertId { get; set; }
            public string Problem { get; set; }
            public string Explanation { get; set; }
            public string RequiredAction { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class AlertsGroupedPerRecipient
        {
            public OutgoingEmailMessage.SenderRecipient Recipient { get; set; }
            public List<AlertDetails> Alerts { get; set; }
        }
    }
}
