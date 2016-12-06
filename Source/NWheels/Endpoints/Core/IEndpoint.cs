using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Authorization;
using NWheels.Processing.Messages;

namespace NWheels.Endpoints.Core
{
    public interface IEndpoint
    {
        void PushMessage(ISession session, IMessageObject message);
        string Name { get; }
        bool IsPushSupported { get; }
        TimeSpan? SessionIdleTimeoutDefault { get; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IAnyEndpoint
    {
        void Start();
        void BeginStop();
        bool WaitUntilStopped(TimeSpan timeout);
        string Name { get; }
        Type MessageType { get; }
        bool IsStarted { get; }
        event Action<ITransportConnection, Exception> SendFailed;
        event Action<ITransportConnection, Exception> ReceiveFailed;
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IStatlessEndpoint : IAnyEndpoint
    {
        void SendMessage(object message);
        event Action<IStatlessEndpoint, object> MessageReceived;
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IStatelessEndpoint<TMessage> : IAnyEndpoint
        where TMessage : class
    {
        void SendMessage(TMessage message);
        event Action<IStatelessEndpoint<TMessage>, TMessage> MessageReceived;
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface ISessionfulEndpoint : IAnyEndpoint
    {
        void SendMessage(ISession session, object message);
        void Broadcast(object message);
        void Broadcast(object message, Func<ISession, bool> predicate);
        ISession[] GetOpenSessions();
        void DropSession(ISession session, ConnectionCloseReason reason);
        void DropAllSessions(ConnectionCloseReason reason);
        int OpenSessionCount { get; }
        event Action<ISessionfulEndpoint, ISession, object> MessageReceived;
        event Action<IConnectEventArgs> Connected;
        event Action<IDisconnectEventArgs> Disconnected;
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface ISessionfulEndpoint<TMessage> : IAnyEndpoint
        where TMessage : class
    {
        void SendMessage(ISession session, TMessage message);
        void Broadcast(TMessage message);
        void Broadcast(TMessage message, Func<ISession, bool> predicate);
        ISession[] GetOpenSessions();
        void DropSession(ISession session, ConnectionCloseReason reason);
        void DropAllSessions(ConnectionCloseReason reason);
        int OpenSessionCount { get; }
        event Action<ISessionfulEndpoint<TMessage>, ISession, TMessage> MessageReceived;
        event Action<IConnectEventArgs> Connected;
        event Action<IDisconnectEventArgs> Disconnected;
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IAnyEndpointTransport
    {
        void Start();
        void BeginStop();
        bool WaitUntilStopped(TimeSpan timeout);
        bool IsStarted { get; }
        event Action<ITransportConnection, Exception> SendFailed;
        event Action<ITransportConnection, Exception> ReceiveFailed;
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IStatelessEndpointTransport<TSerialized> : IAnyEndpointTransport
        where TSerialized : class
    {
        void SendMessage(TSerialized message);
        event Action<TSerialized> MessageReceived;
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface ISessionfulEndpointTransport<TSerialized> : IAnyEndpointTransport
        where TSerialized : class
    {
        void SendMessage(ITransportConnection connection, TSerialized message);
        void Broadcast(TSerialized message);
        void Broadcast(TSerialized message, Func<ITransportConnection, bool> predicate);
        ITransportConnection[] GetOpenConnections();
        void DropConnection(ITransportConnection connection, ConnectionCloseReason reason);
        void DropAllConnections(ConnectionCloseReason reason);
        int OpenConnectionCount { get; }
        event Action<ITransportConnection, TSerialized> MessageReceived;
        event Action<ITransportConnection> RemotePartyConnected;
        event Action<ITransportConnection, ConnectionCloseReason> RemotePartyDisconnected;
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IEndpointSerializer<TDeserialized, TSerialized>
        where TDeserialized : class
        where TSerialized : class
    {
        TSerialized Serialize(TDeserialized deserialized);
        TDeserialized Deserialize(TSerialized serialized);
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface ITransportConnection
    {
        Uri RemoteParty { get; }
        ISession Session { get; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface ITransportConnectionSessionSetter
    {
        void SetSession(ISession session);
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IConnectEventArgs
    {
        ISessionfulEndpoint Endpoint { get; }
        Uri RemoteParty { get; }
        ISession Session { get; set; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IDisconnectEventArgs
    {
        ISessionfulEndpoint Endpoint { get; }
        Uri RemoteParty { get; }
        ISession Session { get; }
        ConnectionCloseReason Reason { get; }
    }
}
