using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Domains.Security.Core;
using NWheels.Utilities;

//using NWheels.Stacks.Network;

namespace NWheels.Samples.SimpleChatApp
{
    internal class ChatService : IChatService
    {
        private readonly IChatServiceLogger _logger;
        private readonly IAuthenticationProvider _authenticationProvider;
        //private AbstractNetConnectorsManager _connectorsManager;
        private readonly Dictionary<long, object> _connectedUsers;
        private readonly Dictionary<long, ChatSession> _chatSessions;

        public ChatService(IChatServiceLogger logger, IAuthenticationProvider authenticationProvider)
        {
            _logger = logger;
            _authenticationProvider = authenticationProvider;
            _connectedUsers = new Dictionary<long, object>();
            _chatSessions = new Dictionary<long, ChatSession>();
        }

        public void StartListening()
        {
            throw new NotImplementedException();
        }

        public void Shutdown()
        {
            throw new NotImplementedException();
        }


        public bool LoginUser(ChatMessages.LoginRequest loginParams)
        {
            ChatMessages.LoginResponse response = new ChatMessages.LoginResponse();

            try
            {
                UserAccountPrincipal accountPrincipal = _authenticationProvider.Authenticate(loginParams.Username, loginParams.Password.ClearToSecure());
                response.Result = LoginErrorCode.Success;
            }
            catch (Exception ex)
            {
                _logger.LoginException(loginParams.Username, ex);
                response.Result = LoginErrorCode.InternalError;
            }

            return response.Result == LoginErrorCode.Success;
        }

    }
}
