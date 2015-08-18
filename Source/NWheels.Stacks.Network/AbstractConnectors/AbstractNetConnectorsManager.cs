using System;
using System.Collections.Generic;
using System.Text;
using NWheels.Endpoints;

namespace NWheels.Stacks.Network
{
    public abstract class AbstractNetConnectorsManager
    {
        protected Int32 Id;
        protected MessagesDispatcher m_MessagesDispatcher;
        protected Dictionary<Int32, AbstractNetConnector> _connectors = new Dictionary<Int32, AbstractNetConnector>();
        protected Int32 MaxConnectorId = Int32.MaxValue;

        protected readonly INetworkEndpointLogger Logger;
        protected readonly NetworkApiEndpointRegistration Registration;

        private static Random ms_Random = new Random();

        public AbstractNetConnectorsManager(Int32 id, NetworkApiEndpointRegistration registration, INetworkEndpointLogger logger)
        {
            Id = id;
            Registration = registration;
            Logger = logger;
            LastConnectorsIds = new Dictionary<int, DateTime>();
        }

        //-----------------

        public virtual void SendAll(string buf)
        {
            byte[] BufferToSend = Encoding.UTF8.GetBytes(buf);
            SendAll(BufferToSend);
        }

        public virtual void SendAll(byte[] buf)
        {
            lock (_connectors)
            {
                foreach (AbstractNetConnector c in _connectors.Values)
                {
                    try
                    {
                        c.Send(buf);
                    }
                    catch (Exception e)
                    {
                        Logger.SendFailed(Registration.Address.ToString(), Registration.Contract.FullName, e);
                    }
                }
            }
        }

        public virtual void Send(Int32 id, string buf)
        {
            byte[] BufferToSend = Encoding.UTF8.GetBytes(buf);
            Send(id, BufferToSend);
        }

        public virtual void Send(Int32 id, byte[] buf)
        {
            AbstractNetConnector c;
            lock (_connectors)
            {
                if (_connectors.TryGetValue(id, out c))
                {
                    c.Send(buf);
                }
                else
                {
                    throw new Exception(String.Format("Trying to send packet to {0} failed. the connector does not exist", id));
                }
            }
        }


        public virtual void StartListening(int port, bool isBlockingAcceptLoop)
        {
        }

        public virtual void StopListening()
        {
            List<AbstractNetConnector> allConnectors = new List<AbstractNetConnector>(_connectors.Values);
            foreach (AbstractNetConnector c in allConnectors)
            {
                c.Dispose();
            }

            _connectors.Clear();
        }

        //-----------------
        public void RegisterMessageDispatcher(MessagesDispatcher MsgDisp)
        {
            m_MessagesDispatcher = MsgDisp;
        }
        //-----------------  

        internal void RegisterConnector(AbstractNetConnector cc)
        {
            lock (_connectors)
            {
                _connectors.Add(cc.Id, cc);
                LastConnectorsIds.Remove(cc.Id);
                TimeSpan timeSpan = new TimeSpan(0, 0, 10);
                List<int> connectionsToRemove = new List<int>();
                foreach (KeyValuePair<int, DateTime> keyVal in LastConnectorsIds)
                {
                    if (DateTime.UtcNow - keyVal.Value > timeSpan)
                    {
                        connectionsToRemove.Add(keyVal.Key);
                    }
                }
                foreach (int cId in connectionsToRemove)
                {
                    LastConnectorsIds.Remove(cId);
                }
            }
            cc.RegisterMessageDispatcher(m_MessagesDispatcher);
            cc.StartKeepAliveService();
        }

        //in case that the client side dosent have unique id
        //the server allocate a temporary random id, and validate its uniqueness.
        internal Int32 GenerateNewConnectorId()
        {
            Int32 newId = 0;

            lock (this.Connectors)
            {
                while (newId == 0)
                {
                    Int32 checkNewId = ms_Random.Next() % this.MaxConnectorId;
                    if (!this.Connectors.ContainsKey(checkNewId) && !this.LastConnectorsIds.ContainsKey(checkNewId))
                    {
                        newId = checkNewId;
                    }
                }
                this.LastConnectorsIds.Add(newId, DateTime.UtcNow);
            }
            return newId;
        }

        internal protected virtual void UnregisterConnector(AbstractNetConnector cc)
        {
            lock (_connectors)
            {
                _connectors.Remove(cc.Id);
            }
        }

        public Dictionary<Int32, AbstractNetConnector> Connectors
        {
            get { return _connectors; }
        }

        internal protected Dictionary<Int32, DateTime> LastConnectorsIds { get; private set; }

    }
}
