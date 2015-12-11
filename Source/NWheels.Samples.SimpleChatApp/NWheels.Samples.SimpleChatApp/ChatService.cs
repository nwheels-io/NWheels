using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NWheels.Domains.Security;
using NWheels.Domains.Security.Core;
using NWheels.Utilities;

//using NWheels.Stacks.Network;

namespace NWheels.Samples.SimpleChatApp
{
    internal class ChatService : IChatService
    {
        private readonly IChatServiceLogger _logger;
        //private AbstractNetConnectorsManager _connectorsManager;
        private readonly Dictionary<long, object> _connectedUsers;
        private readonly Dictionary<long, ChatSession> _chatSessions;
        private readonly UserLoginTransactionScript _loginTx;

        public ChatService(UserLoginTransactionScript loginTx, IChatServiceLogger logger)
        {
            _loginTx = loginTx;
            _logger = logger;
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
                IUserAccountEntity userAccount;
                _loginTx.Execute(loginParams.Username, loginParams.Password);
                UserAccountPrincipal accountPrincipal = (UserAccountPrincipal)Thread.CurrentPrincipal;
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
