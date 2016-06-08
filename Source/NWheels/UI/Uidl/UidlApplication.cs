using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.UI;
using Autofac;
using Newtonsoft.Json;
using NWheels.Authorization;
using NWheels.Authorization.Core;
using NWheels.Concurrency;
using NWheels.Endpoints.Core;
using NWheels.UI.Toolbox;

namespace NWheels.UI.Uidl
{
    [DataContract(Name = "Application", Namespace = UidlDocument.DataContractNamespace)]
    public class UidlApplication : RootContentUidlNode
    {
        public UidlApplication(string idName)
            : base(UidlNodeType.Application, idName, parent: null)
        {
            this.Screens = new List<UidlScreen>();
            this.ScreenParts = new List<UidlScreenPart>();
            this.UserAlerts = new Dictionary<string, UidlUserAlert>();

            this.RequestNotAuthorized = new UidlNotification("RequestNotAuthorized", this);
            base.Notifications.Add(this.RequestNotAuthorized);

            this.RequiredDomainApis = new List<Type>();
            this.RequiredDomainContexts = new List<Type>();
            this.DefaultSkin = "inspinia";
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override IEnumerable<string> GetTranslatables()
        {
            return base.GetTranslatables()
                .Concat(Screens.SelectMany(s => s.GetTranslatables()))
                .Concat(ScreenParts.SelectMany(sp => sp.GetTranslatables()))
                .Concat(UserAlerts.Values.Select(ua => ua.Text));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual object CreateViewStateForCurrentUser(IComponentContext components)
        {
            return null;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool ValidateUser(IPrincipal user)
        {
            var identity = (IIdentityInfo)user.Identity;
            
            if (!Authorization.TryValidateUser(identity))
            {
                return false;
            }

            if (!ValidateUser(identity))
            {
                return false;
            }

            return true;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataMember]
        public string Version { get; set; }
        [DataMember]
        public List<UidlScreen> Screens { get; set; }
        [DataMember, JsonIgnore]
        public List<UidlScreenPart> ScreenParts { get; set; }
        [DataMember]
        public Dictionary<string, UidlUserAlert> UserAlerts { get; set; }
        [DataMember]
        public ModalUserAlert ModalAlert { get; set; }
        [DataMember]
        public string DefaultSkin { get; set; }
        [DataMember]
        public string HeaderNativeSnippet { get; set; }
        [DataMember]
        public string FooterNativeSnippet { get; set; }
        [DataMember]
        public List<NativeTriggeredCodeSnippet> NativeTriggeredSnippets { get; set; }


        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public UidlNotification RequestNotAuthorized { get; set; }
        public UidlNotification UserAlreadyAuthenticated { get; set; }
        public UidlNotification UserSessionExpired { get; set; }
        public UidlNotification ServerConnectionLost { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public List<Type> RequiredDomainApis { get; private set; }
        public List<Type> RequiredDomainContexts { get; private set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataMember]
        public string InitialScreenQualifiedName
        {
            get
            {
                var isAuthenticated = Thread.CurrentPrincipal.Identity.IsAuthenticated;

                if ( isAuthenticated && InitialScreenIfAuthenticated != null )
                {
                    return InitialScreenIfAuthenticated.QualifiedName;
                }

                if ( !isAuthenticated && InitialScreenIfNotAuthenticated != null )
                {
                    return InitialScreenIfNotAuthenticated.QualifiedName;
                }

                return Screens.First().QualifiedName;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataMember]
        public bool IsUserAuthenticated
        {
            get
            {
                return Thread.CurrentPrincipal.Identity.IsAuthenticated;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataMember]
        public int SessionIdleTimeoutMinutes
        {
            get
            {
                var endpoint = Session.Current.Endpoint;

                if ( endpoint != null )
                {
                    return (int)endpoint.SessionIdleTimeoutDefault.GetValueOrDefault(TimeSpan.Zero).TotalMinutes;
                }
                else
                {
                    return 0;
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal protected UidlScreen InitialScreenIfAuthenticated { get; protected set; }
        internal protected UidlScreen InitialScreenIfNotAuthenticated { get; protected set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual bool ValidateUser(IIdentityInfo identity)
        {
            return true;
        }
        
        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected void RequireDomainApi<TContract>() 
            where TContract : class
        {
            this.RequiredDomainApis.Add(typeof(TContract));            
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected void RequireDomainContext<TContract>()
            where TContract : class
        {
            this.RequiredDomainContexts.Add(typeof(TContract));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class NativeTriggeredCodeSnippet
        {
            public string Trigger { get; set; }
            public string Snippet { get; set; }
        }
    }
}
