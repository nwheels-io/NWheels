using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Processing.Messages;
using NWheels.TypeModel.Serialization;

namespace NWheels.Stacks.Network.AbstractConnectors
{
    public class BinaryContentMessage : MessageObjectBase
    {
        private byte[] _messageBuffer;
        private IObjectSerializer _serializer;

        public BinaryContentMessage(byte[] messageBuffer, IObjectSerializer serializer)
        {
            _messageBuffer = messageBuffer;
            _serializer = serializer;
        }

        #region Implementation of IMessageObject

        public override IReadOnlyCollection<IMessageHeader> Headers
        {
            get
            {
                byte[] headers = new byte[2];
                headers[0] = _messageBuffer[0];
                headers[1] = _messageBuffer[1];
                return null;
            }
        }

        public override object Body
        {
            get { return _serializer.Deserialize(this); }
        }

        #endregion
    }
}
