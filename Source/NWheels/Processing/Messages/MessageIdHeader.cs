using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Processing.Messages
{
    public class MessageIdHeader : IMessageHeader
    {
        public MessageIdHeader(Guid id)
        {
            Id = id;
        }

        public Guid Id { get; private set; }

        string IMessageHeader.Name
        {
            get { return "Id"; }
        }

        string IMessageHeader.Values
        {
            get { return Id.ToString("D"); }
        }
    }
}
