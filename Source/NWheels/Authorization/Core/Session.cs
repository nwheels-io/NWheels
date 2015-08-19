using System;
using System.Security.Principal;
using System.Threading;
using NWheels.Concurrency;
using NWheels.Endpoints.Core;

namespace NWheels.Authorization.Core
{
    public class Session : ISession
    {
        private readonly IFramework _framework;
        private readonly IResourceLock _touchLock;
        private readonly TimeSpan? _slidingExpiration;
        private readonly TimeSpan? _absoluteExpiration;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Session(
            IFramework framework,
            IPrincipal userPrincipal,
            IEndpoint originatorEndpoint,
            TimeSpan? slidingExpiration,
            TimeSpan? absoluteExpiration)
        {
            _absoluteExpiration = absoluteExpiration;
            _slidingExpiration = slidingExpiration;
            _framework = framework;

            Id = _framework.NewGuid().ToString("N");

            _touchLock = _framework.NewLock(ResourceLockMode.Exclusive, "Session[{0}]", Id);

            UserPrincipal = userPrincipal;
            UserIdentity = (userPrincipal as IIdentityInfo);
            OriginatorEndpoint = originatorEndpoint;
            OpenedAtUtc = _framework.UtcNow;

            Touch();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IDisposable Join()
        {
            return new CallContextResourceConsumerScope<Session>(handle => this, externallyOwned: true);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Touch()
        {
            //using ( _touchLock.AcquireWriteAccess(forWhat: "Touch", holdDurationMs: 1) )
            //{
            //    if ( _slidingExpiration.HasValue )
            //    {
            //        ExpiresAtUtc = _framework.UtcNow.Add(_slidingExpiration.Value);
            //    }
                    
            //    if (  )
            //        if (_absoluteExpiration.HasValue && _framework.UtcNow >= OpenedAtUtc.Add(_absoluteExpiration.Value))
            //    {
            //        return;
            //    }

            //}
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string Id { get; private set; }
        public IPrincipal UserPrincipal { get; private set; }
        public IIdentityInfo UserIdentity { get; private set; }
        public IEndpoint OriginatorEndpoint { get; private set; }
        public DateTime OpenedAtUtc { get; private set; }
        public DateTime? ExpiresAtUtc { get; private set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of Object

        public override string ToString()
        {
            return this.Id;
        }

        #endregion
    }
}
