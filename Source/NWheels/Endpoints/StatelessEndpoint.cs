using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Endpoints.Core;

namespace NWheels.Endpoints
{
    public class StatelessEndpoint<TMessage, TSerialized> : IStatelessEndpoint<TMessage>, IStatlessEndpoint, IAnyEndpoint
        where TMessage : class
        where TSerialized : class
    {
        private readonly string _name;
        private readonly IEndpointSerializer<TMessage, TSerialized> _serializer;
        private readonly IStatelessEndpointTransport<TSerialized> _transport;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public StatelessEndpoint(string name, IEndpointSerializer<TMessage, TSerialized> serializer, IStatelessEndpointTransport<TSerialized> transport)
        {
            _name = name;
            _serializer = serializer;
            _transport = transport;
            _transport.MessageReceived += OnMessageReceived;
            _transport.SendFailed += OnSendFailed;
            _transport.ReceiveFailed += OnReceiveFailed;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void SendMessage(TMessage message)
        {
            var serialized = _serializer.Serialize(message);
            _transport.SendMessage(serialized);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Start()
        {
            _transport.Start();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void BeginStop()
        {
            _transport.BeginStop();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool WaitUntilStopped(TimeSpan timeout)
        {
            return _transport.WaitUntilStopped(timeout);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        void IStatlessEndpoint.SendMessage(object message)
        {
            this.SendMessage((TMessage)message);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        event Action<IStatlessEndpoint, object> IStatlessEndpoint.MessageReceived
        {
            add
            {
                this.AnyMessageReceived += value;
            }
            remove
            {
                this.AnyMessageReceived -= value;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string Name
        {
            get
            {
                return _name;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Type MessageType
        {
            get
            {
                return typeof(TMessage);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool IsStarted
        {
            get
            {
                return _transport.IsStarted;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public event Action<IStatelessEndpoint<TMessage>, TMessage> MessageReceived;
        public event Action<ITransportConnection, Exception> SendFailed;
        public event Action<ITransportConnection, Exception> ReceiveFailed;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void OnMessageReceived(TSerialized serialized)
        {
            var deserialized = _serializer.Deserialize(serialized);

            if (MessageReceived != null)
            {
                MessageReceived(this, deserialized);
            }

            if (AnyMessageReceived != null)
            {
                AnyMessageReceived(this, deserialized);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void OnSendFailed(ITransportConnection connection, Exception error)
        {
            if (SendFailed != null)
            {
                SendFailed(connection, error);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void OnReceiveFailed(ITransportConnection connection, Exception error)
        {
            if (ReceiveFailed != null)
            {
                ReceiveFailed(connection, error);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private event Action<IStatlessEndpoint, object> AnyMessageReceived;
    }
}
