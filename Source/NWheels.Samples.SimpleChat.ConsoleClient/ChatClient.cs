using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Endpoints;
using NWheels.Samples.SimpleChat.Contracts;

namespace NWheels.Samples.SimpleChat.ConsoleClient
{
    public class ChatClient : IChatClientApi, IDuplexNetworkApiTarget<IChatClientApi, IChatServiceApi>
    {
        private IChatServiceApi _server;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of IDuplexNetworkApiTarget<IChatClientApi,IChatServiceApi>

        void IDuplexNetworkApiTarget<IChatClientApi, IChatServiceApi>.OnConnected(
            IDuplexNetworkApiEndpoint<IChatClientApi, IChatServiceApi> endpoint, 
            IChatServiceApi remoteProxy)
        {
            _server = remoteProxy;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        void IDuplexNetworkApiTarget<IChatClientApi, IChatServiceApi>.OnDisconnected(
            IDuplexNetworkApiEndpoint<IChatClientApi, IChatServiceApi> endpoint, 
            IChatServiceApi remoteProxy, 
            ConnectionCloseReason reason)
        {
            if (reason != ConnectionCloseReason.ByContract)
            {
                Console.WriteLine("ERROR > our server went down..!");
            }
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of IChatClientApi

        public async Task<string> RequestPassword()
        {
            string password = null;

            while (string.IsNullOrEmpty(password))
            {
                password = await ConsoleEx.ReadLineAsync(ConsoleColor.Yellow, "PASSWORD IS REQUIRED! Enter your password, then hit ENTER > ");
            }

            return password;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        void IChatClientApi.PushServerInfo(ServerInfo info)
        {
            ConsoleEx.WriteLine(ConsoleColor.Magenta, "SERVER INFO > {0}, version {1}", info.ServerProduct, info.ServerVersion);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        void IChatClientApi.PushMessage(string who, string what)
        {
            ConsoleEx.WriteLine(ConsoleColor.Cyan, "{0} > {1}", who, what);
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        public IChatServiceApi Server
        {
            get { return _server; }
        }
    }
}
