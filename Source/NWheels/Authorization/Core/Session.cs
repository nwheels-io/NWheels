using System;
using System.Globalization;
using System.Security.Principal;
using System.Threading;
using NWheels.Concurrency;
using NWheels.Endpoints.Core;

namespace NWheels.Authorization.Core
{
    public class Session : ISession, ICoreSession, IAccessControlContext, IScopedConsumptionResource
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
            TimeSpan? absoluteExpiration,
            CultureInfo culture = null,
            TimeZoneInfo timeZone = null)
        {
            _absoluteExpiration = absoluteExpiration;
            _slidingExpiration = slidingExpiration;
            _framework = framework;

            Id = _framework.NewGuid().ToString("N");

            _touchLock = _framework.NewLock(ResourceLockMode.Exclusive, "Session[{0}]", Id);

            UserPrincipal = userPrincipal;
            UserIdentity = (userPrincipal as IIdentityInfo);
            Endpoint = originatorEndpoint;
            Culture = culture ?? _s_fallbackCulture;
            TimeZone = timeZone ?? _s_fallbackTimeZone;
            OpenedAtUtc = _framework.UtcNow;

            Touch();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IDisposable Join()
        {
            return new CallContextResourceConsumerScope<Session>(handle => this, externallyOwned: true, forceNewResource: true);
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

        ////-----------------------------------------------------------------------------------------------------------------------------------------------------

        //public TUser UserAccountAs<TUser>() where TUser : class
        //{
        //    UserIdentity.U
        //}

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string Id { get; private set; }
        public IPrincipal UserPrincipal { get; private set; }
        public IIdentityInfo UserIdentity { get; private set; }
        public IEndpoint Endpoint { get; private set; }
        public CultureInfo Culture { get; internal set; }
        public TimeZoneInfo TimeZone { get; internal set; }
        public DateTime OpenedAtUtc { get; private set; }
        public DateTime? ExpiresAtUtc { get; private set; }
        
        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool IsGlobalImmutable
        {
            get
            {
                return (UserIdentity.IsGlobalSystem || UserIdentity.IsGlobalAnonymous);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of ICoreSession

        ITransportConnection ICoreSession.EndpointConnection { get; set; }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of IRuntimeAccessContext

        ISession IAccessControlContext.Session
        {
            get
            {
                return this;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        string IAccessControlContext.UserStory
        {
            get
            {
                return null; //TODO: implement this property
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        Type IAccessControlContext.ApiContract
        {
            get
            {
                return null; //TODO: implement this property
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        string IAccessControlContext.ApiOperation
        {
            get
            {
                return null; //TODO: implement this property
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        Type IAccessControlContext.DomainContext
        {
            get
            {
                return null;
            }
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of Object

        public override string ToString()
        {
            return this.Id;
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        void IScopedConsumptionResource.ActiveScopeChanged(bool currentScopeIsActive)
        {
            if ( currentScopeIsActive )
            {
                Thread.CurrentPrincipal = this.UserPrincipal;
                Thread.CurrentThread.CurrentUICulture = this.Culture;
            }
            else
            {
                Thread.CurrentPrincipal = null;
                Thread.CurrentThread.CurrentUICulture = _s_fallbackCulture;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal void Authorize(IPrincipal userPrincipal)
        {
            this.UserPrincipal = userPrincipal;
            this.UserIdentity = (IIdentityInfo)userPrincipal.Identity;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static readonly CultureInfo _s_fallbackCulture = CultureInfo.GetCultureInfo("en-US");
        private static readonly TimeZoneInfo _s_fallbackTimeZone = TimeZoneInfo.Utc;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static void Clear()
        {
            CallContextResourceConsumerScope<Session>.Clear();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static ISession Current
        {
            get
            {
                return CallContextResourceConsumerScope<Session>.CurrentResource;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static IAccessControlContext CurrentAccessContext
        {
            get
            {
                return CallContextResourceConsumerScope<Session>.CurrentResource;
            }
        }
    }
}
