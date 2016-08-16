using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Processing.Messages
{
    public class MessageFromHeader : IMessageHeader
    {
        public MessageFromHeader(string sender)
        {
            this.Sender = sender;
        }

        public string Sender { get; private set; }

        string IMessageHeader.Name
        {
            get { return "From"; }
        }

        string IMessageHeader.Values
        {
            get { return this.Sender; }
        }
    }
}
