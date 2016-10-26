using System;
using System.Collections.Generic;
using System.Text;
using NWheels.Endpoints;

namespace NWheels.Stacks.Network
{
    public abstract class NetConnectorsBinaryTransport
    {
        private int _nextConnectorId;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected MessagesDispatcher m_MessagesDispatcher;
        protected Dictionary<Int32, AbstractNetBinaryConnector> _connectors = new Dictionary<Int32, AbstractNetBinaryConnector>();
        protected Int32 MaxConnectorId = Int32.MaxValue;

        protected readonly INetworkEndpointLogger Logger;
        protected readonly NetworkApiEndpointRegistration Registration;
        protected readonly IAbstractNetwrokTransportConfig MyConfiguration;

        public NetConnectorsBinaryTransport(IAbstractNetwrokTransportConfig configuration, NetworkApiEndpointRegistration registration, INetworkEndpointLogger logger)
        {
            MyConfiguration = configuration;
            Registration = registration;
            Logger = logger;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual void SendAll(string str)
        {
            byte[] buf = Encoding.UTF8.GetBytes(str);
            SendAll(buf);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual void SendAll(byte[] buf)
        {
            List<AbstractNetBinaryConnector> allConnectors;
            lock (_connectors)
            {
                allConnectors = new List<AbstractNetBinaryConnector>(_connectors.Values);
            }

            foreach (AbstractNetBinaryConnector c in allConnectors)
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

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual void Send(Int32 id, string buf)
        {
            byte[] BufferToSend = Encoding.UTF8.GetBytes(buf);
            Send(id, BufferToSend);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual void Send(Int32 id, byte[] buf)
        {
            AbstractNetBinaryConnector c;
            lock (_connectors)
            {
                _connectors.TryGetValue(id, out c);
            }
            if (c != null)
            {
                c.Send(buf);
            }
            else
            {
                throw new Exception(String.Format("Trying to send packet to {0} failed. the connector does not exist", id));
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual void StartListening()
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual void StopListening()
        {
            List<AbstractNetBinaryConnector> allConnectors;
            lock (_connectors)
            {
                allConnectors = new List<AbstractNetBinaryConnector>(_connectors.Values);
            }
            foreach (AbstractNetBinaryConnector c in allConnectors)
            {
                c.Dispose();
            }

            lock (_connectors)
            {
                _connectors.Clear();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void RegisterMessageDispatcher(MessagesDispatcher MsgDisp)
        {
            m_MessagesDispatcher = MsgDisp;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal void RegisterConnector(AbstractNetBinaryConnector cc)
        {
            lock (_connectors)
            {
                while (_connectors.ContainsKey(_nextConnectorId)) { _nextConnectorId++; }
                cc.Id = _nextConnectorId++;
                _connectors.Add(cc.Id, cc);
            }
            cc.RegisterMessageDispatcher(m_MessagesDispatcher);
            cc.StartKeepAliveService();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal protected virtual void UnregisterConnector(AbstractNetBinaryConnector cc)
        {
            lock (_connectors)
            {
                _connectors.Remove(cc.Id);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Dictionary<Int32, AbstractNetBinaryConnector> Connectors
        {
            get { return _connectors; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
    }
}
