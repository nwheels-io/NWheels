#pragma warning disable 1998

using System.Threading.Tasks;
using NWheels.Endpoints;
using NWheels.Exceptions;
using NWheels.Logging;
using NWheels.Samples.SimpleChat.Contracts;

namespace NWheels.Samples.SimpleChat.Server
{
    public class ChatService : IChatServiceApi, IDuplexNetworkApiTarget<IChatServiceApi, IChatClientApi>
    {
        private readonly ILogger _logger;
        private IDuplexNetworkApiEndpoint<IChatServiceApi, IChatClientApi> _endpoint;
        private IChatClientApi _myClient;
        private string _myName;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ChatService(ILogger logger)
        {
            _logger = logger;
        }

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

        public void RequestServerInfo()
        {
            _logger.ClientRequestedServerInfo();

            _myClient.PushServerInfo(new ServerInfo() {
                ServerProduct = "NWheels.Samples.SimpleChat.Server",
                ServerVersion = "1.2.3.4-alpha5"
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public async Task<UserInfo> Hello(string myName)
        {
            _logger.ClientSaidHello(name: myName);

            _myName = myName;

            var password = await _myClient.RequestPassword();

            if (password == "11111")
            {
                _myClient.PushMessage(who: "SERVER", what: "Hey! Welcome to the chat, " + myName);
                BroadcastToOthers("SERVER", myName + " has joined the chat");

                _logger.ClientJoinedChat(name: myName, userId: 123);

                return new UserInfo() {
                    UserId = 123,
                    FullName = myName,
                    RoleName = "Regular chatter"
                };
            }

            _logger.ClientAuthenticationFailed(name: myName, reason: "Login/password mismatch");
            throw new DomainFaultException<string>("LoginIncorrect");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void GoodBye()
        {
            _logger.ClientSaidGoodbye(name: _myName);
            BroadcastToOthers("SERVER", _myName + " is saying goodbye");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void SayToOthers(string message)
        {
            _logger.ClientBroadcastingMessage(name: _myName, message: message);
            BroadcastToOthers(_myName, message);
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void BroadcastToOthers(string who, string what)
        {
            _endpoint.Broadcast(client => {
                if (client != _myClient)
                {
                    client.PushMessage(who, what);
                }
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface ILogger : IApplicationEventLogger
        {
            [LogInfo]
            void ClientRequestedServerInfo();
            [LogInfo]
            void ClientSaidHello(string name);
            [LogInfo]
            void ClientJoinedChat(string name, int userId);
            [LogError]
            void ClientAuthenticationFailed(string name, string reason);
            [LogInfo]
            void ClientBroadcastingMessage(string name, string message);
            [LogInfo]
            void ClientSaidGoodbye(string name);
        }
    }
}
