using System;
using System.Collections.Concurrent;
using System.Security;
using System.Security.Principal;
using NWheels.Authorization.Core;
using NWheels.Endpoints.Core;

namespace NWheels.Authorization.Impl
{
    public class LocalTransientSessionManager : ISessionManager, ICoreSessionManager
    {
        private readonly IFramework _framework;
        private readonly ConcurrentDictionary<string, Session> _sessionById;
        private readonly Session _anonymousSession;
        private readonly Session _systemSession;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public LocalTransientSessionManager(IFramework framework)
        {
            _framework = framework;
            _sessionById = new ConcurrentDictionary<string, Session>();
            _anonymousSession = new Session(framework, AnonymousPrincipal.Instance, originatorEndpoint: null, slidingExpiration: null, absoluteExpiration: null);
            _systemSession = new Session(framework, SystemPrincipal.Instance, originatorEndpoint: null, slidingExpiration: null, absoluteExpiration: null);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IDisposable OpenSession(IPrincipal userPrincipal, IEndpoint originatorEndpoint)
        {
            var newSession = new Session(_framework, userPrincipal, originatorEndpoint, slidingExpiration: null, absoluteExpiration: null);
            
            _sessionById.AddOrUpdate(
                newSession.Id, 
                newSession,
                (key, existingSession) => {
                    throw new SecurityException("Duplicate session id");
                });

            return newSession.Join();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IDisposable JoinSession(string sessionId)
        {
            var session = _sessionById[sessionId];
            return session.Join();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IDisposable JoinAnonymous()
        {
            return _anonymousSession.Join();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IDisposable JoinSystem()
        {
            return _systemSession.Join();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of ICoreSessionManager

        public ISession[] GetOpenSessions()
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void DropSession(string sessionId)
        {
            Session session;
            _sessionById.TryRemove(sessionId, out session);
        }

        #endregion
    }
}
