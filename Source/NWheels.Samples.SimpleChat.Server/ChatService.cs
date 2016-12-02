using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Endpoints;
using NWheels.Extensions;
using NWheels.Samples.SimpleChat.Contracts;

namespace NWheels.Samples.SimpleChat.Server
{
    public class ChatService : IChatServiceApi, IDuplexNetworkApiEvents
    {
        private readonly DuplexTcpServer<IChatServiceApi, IChatClientApi> _tcpServer;
        private readonly IChatClientApi _myClient;
        private string _myName;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ChatService(DuplexTcpServer<IChatServiceApi, IChatClientApi> tcpServer, IChatClientApi myClient)
        {
            _tcpServer = tcpServer;
            _myClient = myClient;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of IChatServerApi

        public void Hello(string myName)
        {
            _myName = myName;

            _myClient.SomeoneSaidSomething(who: "SERVER", what: "Hey! Welcome to the chat, " + myName);
            BroadcastToOthers("SERVER", myName + " has joined the chat");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void GoodBye()
        {
            BroadcastToOthers("SERVER", _myName + " is saying goodbye");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void SayToOthers(string message)
        {
            BroadcastToOthers(_myName, message);
        }

        #endregion
        
        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        void IDuplexNetworkApiEvents.OnSessionClosed(IDuplexNetworkEndpointApiProxy proxy, SessionCloseReason reason)
        {
            if (reason != SessionCloseReason.ByContract)
            {
                BroadcastToOthers("SERVER", _myName + " has gone silently....");
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void BroadcastToOthers(string who, string what)
        {
            _tcpServer.Broadcast(client => {
                if (client != _myClient)
                {
                    client.SomeoneSaidSomething(who, what);
                }
            });
        }
    }
}
