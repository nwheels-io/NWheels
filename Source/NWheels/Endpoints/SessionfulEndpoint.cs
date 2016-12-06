using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Authorization;
using NWheels.Endpoints.Core;

namespace NWheels.Endpoints
{
    public class SessionfulEndpoint<TMessage, TSerialized> : ISessionfulEndpoint<TMessage>, ISessionfulEndpoint, IAnyEndpoint
        where TMessage : class
        where TSerialized : class
    {
        private readonly string _name;
        private readonly IEndpointSerializer<TMessage, TSerialized> _serializer;
        private readonly ISessionfulEndpointTransport<TSerialized> _transport;
        private Hashtable _connectionBySession;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public SessionfulEndpoint(string name, IEndpointSerializer<TMessage, TSerialized> serializer, ISessionfulEndpointTransport<TSerialized> transport)
        {
            _name = name;
            _serializer = serializer;
            _transport = transport;

            _transport.RemotePartyConnected += OnRemotePartyConnected;
            _transport.RemotePartyDisconnected += OnRemotePartyDisconnected;
            _transport.MessageReceived += OnMessageReceived;
            _transport.SendFailed += OnSendFailed;
            _transport.ReceiveFailed += OnReceiveFailed;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of IAnyEndpoint

        public void Start()
        {
            _connectionBySession = new Hashtable();
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
            _connectionBySession = null;
            return false;//TODO: wait for transport to stop
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string Name
        {
            get { return _name; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Type MessageType
        {
            get { return typeof(TMessage); }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool IsStarted
        {
            get { return _transport.IsStarted; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public event Action<ITransportConnection, Exception> SendFailed;
        public event Action<ITransportConnection, Exception> ReceiveFailed;

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of ISessionfulEndpoint<TMessage>

        public void SendMessage(ISession session, TMessage message)
        {
            var connection = ((ICoreSession)session).EndpointConnection;
            var serializedMessage = _serializer.Serialize(message);
            _transport.SendMessage(connection, serializedMessage);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Broadcast(TMessage message)
        {
            var serializedMessage = _serializer.Serialize(message);
            _transport.Broadcast(serializedMessage);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Broadcast(TMessage message, Func<ISession, bool> predicate)
        {
            var serializedMessage = _serializer.Serialize(message);
            _transport.Broadcast(
                serializedMessage,
                connection => {
                    var predicateResult = predicate(connection.Session);
                    return predicateResult;
                });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ISession[] GetOpenSessions()
        {
            return _connectionBySession.Keys.Cast<ISession>().ToArray();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void DropSession(ISession session, ConnectionCloseReason reason)
        {
            _transport.DropConnection(((ICoreSession)session).EndpointConnection, reason);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void DropAllSessions(ConnectionCloseReason reason)
        {
            _transport.DropAllConnections(reason);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public int OpenSessionCount
        {
            get
            {
                return _connectionBySession.Count;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public event Action<ISessionfulEndpoint<TMessage>, ISession, TMessage> MessageReceived;
        public event Action<IConnectEventArgs> Connected;
        public event Action<IDisconnectEventArgs> Disconnected;

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of ISessionfulEndpoint

        void ISessionfulEndpoint.SendMessage(ISession session, object message)
        {
            this.SendMessage(session, (TMessage)message);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        void ISessionfulEndpoint.Broadcast(object message)
        {
            this.Broadcast((TMessage)message);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        void ISessionfulEndpoint.Broadcast(object message, Func<ISession, bool> predicate)
        {
            this.Broadcast((TMessage)message, predicate);
        }


        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        event Action<ISessionfulEndpoint, ISession, object> ISessionfulEndpoint.MessageReceived
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

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void OnMessageReceived(ITransportConnection connection, TSerialized serialized)
        {
            var deserialized = _serializer.Deserialize(serialized);

            if (MessageReceived != null)
            {
                MessageReceived(this, connection.Session, deserialized);
            }

            if (AnyMessageReceived != null)
            {
                AnyMessageReceived(this, connection.Session, deserialized);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void OnRemotePartyConnected(ITransportConnection connection)
        {
            if (Connected != null)
            {
                IConnectEventArgs args = (IConnectEventArgs)connection;
                Connected(args);

                if (args.Session != null)
                {
                    _connectionBySession[args.Session] = connection;
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void OnRemotePartyDisconnected(ITransportConnection connection, ConnectionCloseReason reason)
        {
            if (Disconnected != null)
            {
                IDisconnectEventArgs args = (IDisconnectEventArgs)connection;
                Disconnected(args);

                if (connection.Session != null)
                {
                    _connectionBySession.Remove(connection.Session);
                }
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

        private event Action<ISessionfulEndpoint, ISession, object> AnyMessageReceived;
    }
}
