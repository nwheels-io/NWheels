using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.Cookies;
using NWheels.Authorization;
using NWheels.Authorization.Core;
using NWheels.Extensions;

namespace NWheels.Stacks.NancyFx
{
    public class WebModuleSessionHook
    {
        public static readonly string SessionIdKey = "WebModuleSessionHook.SessionIdKey";

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private readonly WebApplicationModule _module;
        private readonly ISessionManager _sessionManager;
        private readonly IWebApplicationLogger _logger;
        private readonly string _sessionCookieName;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public WebModuleSessionHook(WebApplicationModule module, ISessionManager sessionManager, IWebApplicationLogger logger)
        {
            _module = module;
            _sessionManager = sessionManager;
            _logger = logger;
            _sessionCookieName = sessionManager.As<ICoreSessionManager>().SessionIdCookieName;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Attach(IPipelines pipelines)
        {
            pipelines.BeforeRequest.AddItemToEndOfPipeline(ctx => {
                string sessionCookieValue = null;
                string clearSessionId = null;

                if ( ctx.Request.Cookies.TryGetValue(_sessionCookieName, out sessionCookieValue) )
                {
                    var encryptedSessionId = WebUtility.UrlDecode(sessionCookieValue);
                    
                    try
                    {
                        clearSessionId = _sessionManager.As<ICoreSessionManager>().DecryptSessionId(Convert.FromBase64String(encryptedSessionId));
                    }
                    catch ( CryptographicException e )
                    {
                        _logger.FailedToDecryptSessionCookie(error: e);
                        clearSessionId = null;
                    }
                }

                _sessionManager.JoinSessionOrOpenAnonymous(clearSessionId, _module);
                return null;
            });

            pipelines.AfterRequest.AddItemToStartOfPipeline(ctx => {
                var currentSession = _sessionManager.CurrentSession;

                if ( currentSession != null )
                {
                    var encryptedSessionId = Convert.ToBase64String(_sessionManager.As<ICoreSessionManager>().EncryptSessionId(currentSession.Id));
                    ctx.Response.WithCookie(_sessionCookieName, encryptedSessionId);
                }
                else
                {
                    ctx.Response.WithCookie(SessionIdKey, null);
                }
            });
        }
    }
}
