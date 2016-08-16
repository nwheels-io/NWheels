using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Processing.Messages
{
    public class MessageToHeader : IMessageHeader
    {
        public MessageToHeader(string recipient)
        {
            this.Recipient = recipient;
        }

        public string Recipient { get; private set; }

        string IMessageHeader.Name
        {
            get { return "To"; }
        }

        string IMessageHeader.Values
        {
            get { return this.Recipient; }
        }
    }
}
