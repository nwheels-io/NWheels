﻿using System;
using System.Linq;
using NWheels.Authorization;
using NWheels.Authorization.Core;
using NWheels.DataObjects;
using NWheels.DataObjects.Core;
using NWheels.Domains.Security.Core;
using NWheels.Extensions;
using NWheels.Processing;
using NWheels.Utilities;

namespace NWheels.Domains.Security
{
    public class UserLogoutTransactionScript : ITransactionScript
    {
        private readonly ICoreSessionManager _sessionManager;
        private readonly ISecurityDomainLogger _logger;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public UserLogoutTransactionScript(ICoreSessionManager sessionManager, ISecurityDomainLogger logger)
        {
            _sessionManager = sessionManager;
            _logger = logger;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Execute()
        {
            var session = Session.Current;
            _sessionManager.CloseCurrentSession();
            _logger.UserLoggedOut(session.UserIdentity.LoginName, session.UserIdentity.UserId, session.UserIdentity.EmailAddress, session.Id);
        }
    }
}
