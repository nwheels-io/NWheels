using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NWheels.Authorization;
using NWheels.Authorization.Core;
using NWheels.Extensions;

namespace NWheels.Stacks.ODataBreeze
{
    public class LoggingAndSessionHandler : DelegatingHandler
    {
        private readonly BreezeEndpointComponent _ownerEndpoint;
        private readonly ISessionManager _sessionManager;
        private readonly BreezeEndpointComponent.ILogger _logger;
        private readonly string _sessionCookieName;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public LoggingAndSessionHandler(BreezeEndpointComponent ownerEndpoint, ISessionManager sessionManager, BreezeEndpointComponent.ILogger logger)
        {
            _ownerEndpoint = ownerEndpoint;
            _sessionManager = sessionManager;
            _logger = logger;
            _sessionCookieName = _sessionManager.As<ICoreSessionManager>().SessionIdCookieName;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of DelegatingHandler

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var sessionCookie = request.GetCookie(_sessionCookieName);

            using (_logger.Request(request.Method, request.RequestUri.AbsolutePath, request.RequestUri.Query))
            {
                string sessionId;
                AuthenticateRequest(sessionCookie, out sessionId);

                _sessionManager.JoinSessionOrOpenAnonymous(sessionId, null); //TODO: bring back using 
                //using ( _sessionManager.JoinSessionOrOpenAnonymous(sessionId, null) )
                //{
                var response = await base.SendAsync(request, cancellationToken);
                if (response.Content != null)
                {
                    response.Content.ReadAsStringAsync().Wait();
                }
                _sessionManager.JoinSessionOrOpenAnonymous(sessionId, null); //TODO: remove this, should be carried of logical call context
                return response;
                //}
            }
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void AuthenticateRequest(string sessionCookie, out string sessionId)
        {
            sessionId = null;
            var encryptedSessionCookie = sessionCookie;//WebUtility.UrlDecode(sessionCookie);

            if ( encryptedSessionCookie != null )
            {
                try
                {
                    sessionId = _sessionManager.As<ICoreSessionManager>().DecryptSessionId(Convert.FromBase64String(encryptedSessionCookie));
                }
                catch ( CryptographicException e )
                {
                    _logger.FailedToDecryptSessionCookie(error: e);
                    sessionId = null;
                }
            }

            if ( _sessionManager.IsValidSessionId(sessionId) )
            {
                _sessionManager.JoinSession(sessionId);
            }
            else
            {
                sessionId = null;
                _sessionManager.JoinGlobalAnonymous();
            }
        }
    }
}
