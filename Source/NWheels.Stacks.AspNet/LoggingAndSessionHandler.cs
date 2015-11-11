using System;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.SessionState;
using NWheels.Authorization;
using NWheels.Authorization.Core;
using NWheels.Extensions;

namespace NWheels.Stacks.AspNet
{
    public class LoggingAndSessionHandler : DelegatingHandler, IRequiresSessionState
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
            var sessionId = HttpContext.Current.Session[_sessionCookieName] as string;

            using ( _logger.Request(request.Method.ToString(), request.RequestUri.AbsolutePath, request.RequestUri.Query) )
            {
                AuthenticateRequest(ref sessionId);

                _sessionManager.JoinSessionOrOpenAnonymous(sessionId, null); //TODO: bring back the using 
                //using ( _sessionManager.JoinSessionOrOpenAnonymous(sessionId, null) )
                //{
                var response = await base.SendAsync(request, cancellationToken);
                
                if ( response.Content != null )
                {
                    response.Content.ReadAsStringAsync().Wait();
                }
                
                SetResponseSessionCookie(request, response);
                return response;
                //}
            }
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void SetResponseSessionCookie(HttpRequestMessage request, HttpResponseMessage response)
        {
            if ( Session.Current != null && Session.Current.UserIdentity.IsAuthenticated )
            {
                HttpContext.Current.Session[_sessionCookieName] = Session.Current.Id;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void AuthenticateRequest(ref string sessionId)
        {
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
