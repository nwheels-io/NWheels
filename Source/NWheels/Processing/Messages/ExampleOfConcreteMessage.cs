using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace NWheels.Processing.Messages
{
    internal class ExampleOfConcreteMessage : MessageObjectBase
    {
        private readonly MessageActionHeader _action;

        private object _deserializedBody = null;

        public ExampleOfConcreteMessage(int connectorId, byte[] serializedBody, object serializer, Type recipientContract, MethodInfo recipientMethod)
        {
            ConnectorId = connectorId;
            SerializedBody = serializedBody;
            Serializer = serializer;

            _action = new MessageActionHeader(recipientContract, recipientMethod);
        }

        public int ConnectorId { get; private set; }
        public byte[] SerializedBody { get; private set; }
        public object Serializer { get; private set; }


        public override IReadOnlyCollection<IMessageHeader> Headers
        {
            get
            {
                return new IMessageHeader[] { _action };
            }
        }

        public override object Body
        {
            get
            {
                if ( _deserializedBody == null )
                {
                    //_deserializedBody = Serializer.Deserialize(SerializedBody);
                }

                return _deserializedBody;
            }
        }
    }
}
