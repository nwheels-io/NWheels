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
using NWheels.Endpoints.Core;
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
            //HttpContext.Current.Session[this.GetType().FullName] = true; // force ASP.NET to actually create a session

            using ( var activity = _logger.Request(request.Method.ToString(), request.RequestUri.AbsolutePath, request.RequestUri.Query) )
            {
                try
                {
                    var sessionId = HttpContext.Current.Session[_sessionCookieName] as string;

                    if ( _sessionManager.IsValidSessionId(sessionId) )
                    {
                        _sessionManager.JoinSession(sessionId);
                    }
                    else
                    {
                        _sessionManager.OpenAnonymous(HttpContext.Current.ApplicationInstance as IEndpoint);
                    }

                    //_sessionManager.JoinSessionOrOpenAnonymous(sessionId, null); //TODO: bring back the using 
                    //using ( _sessionManager.JoinSessionOrOpenAnonymous(sessionId, null) )
                    //{
                    var response = await base.SendAsync(request, cancellationToken);

                    //if ( response.Content != null )
                    //{
                    //    // cause response writers to work now, while in the context of current session and thread log.
                    //    // otherwise response writers will be invoked after current session is detached from the thread,
                    //    // and may be blocked by access control or function incorrectly, and will not be captured by thread log.
                    //    response.Content.ReadAsStringAsync().Wait();
                    //}

                    //SetResponseSessionCookie(request, response);
                    Session.Clear();
                    return response;
                    //}
                }
                catch ( Exception e )
                {
                    activity.Fail(e);
                    _logger.RequestFailed(e);
                    throw;
                }
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
        }
    }
}
