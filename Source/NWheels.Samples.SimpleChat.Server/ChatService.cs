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
    public class ChatService : IChatServiceApi, IDuplexNetworkApiTarget<IChatServiceApi, IChatClientApi>
    {
        private IDuplexNetworkApiEndpoint<IChatServiceApi, IChatClientApi> _endpoint;
        private IChatClientApi _myClient;
        private string _myName;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of IDuplexNetworkApiTarget<IChatServiceApi,IChatClientApi>

        void IDuplexNetworkApiTarget<IChatServiceApi, IChatClientApi>.OnConnected(
            IDuplexNetworkApiEndpoint<IChatServiceApi, IChatClientApi> endpoint, 
            IChatClientApi remoteProxy)
        {
            _endpoint = endpoint;
            _myClient = remoteProxy;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        void IDuplexNetworkApiTarget<IChatServiceApi, IChatClientApi>.OnDisconnected(
            IDuplexNetworkApiEndpoint<IChatServiceApi, IChatClientApi> endpoint, 
            IChatClientApi remoteProxy, 
            ConnectionCloseReason reason)
        {
            if (reason != ConnectionCloseReason.ByContract)
            {
                BroadcastToOthers("SERVER", _myName + " has gone silently....");
            }
        }

        #endregion

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

        private void BroadcastToOthers(string who, string what)
        {
            _endpoint.Broadcast(client => {
                if (client != _myClient)
                {
                    client.SomeoneSaidSomething(who, what);
                }
            });
        }
    }
}
