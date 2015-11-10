using System;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using NWheels.Authorization;
using NWheels.Authorization.Core;
using NWheels.Extensions;

namespace NWheels.Stacks.AspNet
{
    public class LoggingAndSessionHandler : DelegatingHandler
    {
        private readonly ISessionManager _sessionManager;
        private readonly IWebApplicationLogger _logger;
        private readonly string _sessionCookieName;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public LoggingAndSessionHandler(ISessionManager sessionManager, IWebApplicationLogger logger)
        {
            _sessionManager = sessionManager;
            _logger = logger;
            _sessionCookieName = _sessionManager.As<ICoreSessionManager>().SessionIdCookieName;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of DelegatingHandler

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var sessionCookie = request.GetCookie(_sessionCookieName);

            using ( _logger.Request(request.Method.ToString(), request.RequestUri.AbsolutePath, request.RequestUri.Query) )
            {
                string sessionId;
                AuthenticateRequest(sessionCookie, out sessionId);

                _sessionManager.JoinSessionOrOpenAnonymous(sessionId, null); //TODO: bring back using 
                //using ( _sessionManager.JoinSessionOrOpenAnonymous(sessionId, null) )
                //{
                var response = await base.SendAsync(request, cancellationToken);
                
                if ( response.Content != null )
                {
                    response.Content.ReadAsStringAsync().Wait();
                }
                
                SetResponseSessionCookie(response);
                return response;
                //}
            }
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void SetResponseSessionCookie(HttpResponseMessage response)
        {
            if ( Session.Current != null )
            {
                var encryptedSessionId = Convert.ToBase64String(_sessionManager.As<ICoreSessionManager>().EncryptSessionId(Session.Current.Id));
                var responseSessionCookie = new Cookie(_sessionCookieName, encryptedSessionId);
                responseSessionCookie.HttpOnly = true;
                response.Headers.SetCookie(responseSessionCookie);
            }
        }

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
