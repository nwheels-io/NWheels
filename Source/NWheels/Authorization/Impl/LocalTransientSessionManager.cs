using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Threading;
using Autofac;
using NWheels.Authorization.Core;
using NWheels.Concurrency;
using NWheels.Endpoints.Core;
using NWheels.Extensions;
using NWheels.Globalization;

namespace NWheels.Authorization.Impl
{
    public class LocalTransientSessionManager : ISessionManager, ICoreSessionManager
    {
        private readonly IComponentContext _components;
        private readonly IFramework _framework;
        private readonly ISessionEventLogger _logger;
        private readonly ConcurrentDictionary<string, Session> _sessionById;
        private readonly Session _anonymousSession;
        private readonly Session _systemSession;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public LocalTransientSessionManager(
            IComponentContext components,
            IFramework framework, 
            AnonymousPrincipal anonymousPrincipal, 
            SystemPrincipal systemPrincipal,
            ISessionEventLogger logger)
        {
            _components = components;
            _framework = framework;
            _logger = logger;
            _sessionById = new ConcurrentDictionary<string, Session>();
            
            _anonymousSession = new Session(framework, anonymousPrincipal, originatorEndpoint: null, slidingExpiration: null, absoluteExpiration: null);
            _systemSession = new Session(framework, systemPrincipal, originatorEndpoint: null, slidingExpiration: null, absoluteExpiration: null);

            // we do not add System session ID to the map, because joining System session from the web would be a security hole
            _sessionById.AddOrUpdate(_anonymousSession.Id, _anonymousSession, (k, v) => _anonymousSession);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool IsValidSessionId(string sessionId)
        {
            return (!string.IsNullOrEmpty(sessionId) && _sessionById.ContainsKey(sessionId));
        }
        
        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ISession OpenSession(IPrincipal userPrincipal, IEndpoint originatorEndpoint)
        {
            var defaultLocale = _components.Resolve<ILocalizationProvider>().GetDefaultLocale();
            var newSession = new Session(
                _framework, 
                userPrincipal, 
                originatorEndpoint, 
                slidingExpiration: null, 
                absoluteExpiration: null,
                culture: defaultLocale.Culture);
            
            _sessionById.AddOrUpdate(
                newSession.Id, 
                newSession,
                (key, existingSession) => {
                    throw _logger.DuplicateSessionId(sessionId: key);
                });

            _logger.SessionOpened(sessionId: newSession.Id, user: userPrincipal, endpoint: originatorEndpoint);
            return newSession;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ISession AuthorizeSession(IPrincipal principal)
        {
            var currentSession = Session.Current;

            if ( currentSession == _anonymousSession || currentSession == _systemSession )
            {
                throw new InvalidOperationException("Cannot mutate a global session.");
            }

            if ( currentSession != null )
            {
                currentSession.As<Session>().Authorize(principal);
                Thread.CurrentPrincipal = currentSession.UserPrincipal;
                return currentSession;
            }

            throw new InvalidOperationException("There is no session associated with current thread.");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IDisposable JoinSession(string sessionId)
        {
            if ( string.IsNullOrEmpty(sessionId) )
            {
                throw new ArgumentNullException("sessionId");
            }
            
            Session session;

            if ( _sessionById.TryGetValue(sessionId, out session) )
            {
                _logger.ThreadJoiningSession(session.Id, session.UserPrincipal, session.Endpoint);
                return session.Join();
            }

            throw _logger.SessionNotFound(sessionId);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IDisposable OpenAnonymous(IEndpoint endpoint)
        {
            var newSession = OpenSession(_anonymousSession.UserPrincipal, endpoint);
            return newSession.As<Session>().Join();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IDisposable JoinSessionOrOpenAnonymous(string sessionId, IEndpoint endpoint)
        {
            if ( !string.IsNullOrEmpty(sessionId) )
            {
                Session session;

                if ( _sessionById.TryGetValue(sessionId, out session) )
                {
                    _logger.ThreadJoiningSession(session.Id, session.UserPrincipal, session.Endpoint);
                    return session.Join();
                }

                _logger.SessionNotFound(sessionId);
            }

            return OpenAnonymous(endpoint); 
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IDisposable JoinGlobalAnonymous()
        {
            _logger.ThreadJoiningAnonymousSession();
            return _anonymousSession.Join();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IDisposable JoinGlobalSystem()
        {
            _logger.ThreadJoiningSystemSession();
            return _systemSession.Join();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of ICoreSessionManager

        public ISession[] GetOpenSessions()
        {
            return _sessionById.Values.ToArray().Cast<ISession>().ToArray();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ISession GetOpenSession(string sessionId)
        {
            return _sessionById[sessionId];
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void SetSessionUserInfo(string sessionId, CultureInfo newCulture = null, TimeZoneInfo newTimeZone = null)
        {
            Session session;

            if (_sessionById.TryGetValue(sessionId, out session))
            {
                if (newCulture != null)
                {
                    session.Culture = newCulture;
                }
                
                if (newTimeZone != null)
                {
                    session.TimeZone = newTimeZone;
                }
            }
            else
            {
                throw _logger.SessionNotFound(sessionId);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void CloseCurrentSession()
        {
            var session = this.CurrentSession;

            if ( session == null )
            {
                throw new InvalidOperationException("No session is associatd with the current thread.");
            }

            _logger.ClosingSession(session.Id);

            //TODO: session cleanup callbacks should be invoked

            Session.Clear();
            
            Session removedSession;
            _sessionById.TryRemove(session.Id, out removedSession);

            JoinGlobalAnonymous();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void DropSession(string sessionId)
        {
            _logger.DroppingSession(sessionId);

            Session session;
            _sessionById.TryRemove(sessionId, out session);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual byte[] EncryptSessionId(string clearSessionId)
        {
            var transform = CryptoProvider.CreateEncryptor();
            var output = new MemoryStream();
            var cryptoStream = new CryptoStream(output, transform, CryptoStreamMode.Write);
            var writer = new StreamWriter(cryptoStream);

            writer.Write(clearSessionId);
            writer.Close();
            cryptoStream.Close();

            return output.ToArray();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual string DecryptSessionId(byte[] encryptedSessionId)
        {
            var transform = CryptoProvider.CreateDecryptor();
            var input = new MemoryStream(encryptedSessionId);
            var cryptoStream = new CryptoStream(input, transform, CryptoStreamMode.Read);
            var reader = new StreamReader(cryptoStream);

            var cleasSessionId = reader.ReadToEnd();

            reader.Close();
            cryptoStream.Close();

            return cleasSessionId;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        public string SessionIdCookieName
        {
            get
            {
                return _s_sessionIdCookieName;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string SingleSignOnTokenName
        {
            get
            {
                return _s_singleSignOnTokenName;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ISession CurrentSession
        {
            get
            {
                return Session.Current;
            }
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static readonly string _s_sessionIdCookieName = "nw";
        private static readonly string _s_singleSignOnTokenName = "ssotkn";
        
        protected static readonly RijndaelManaged CryptoProvider;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        static LocalTransientSessionManager()
        {
            CryptoProvider = new RijndaelManaged();
            CryptoProvider.GenerateIV();
            CryptoProvider.GenerateKey();
        }
    }
}
