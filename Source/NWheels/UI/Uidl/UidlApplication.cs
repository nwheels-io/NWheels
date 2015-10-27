using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

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

        [DataMember]
        public string Version { get; set; }
        [DataMember]
        public List<UidlScreen> Screens { get; set; }
        [DataMember]
        public List<UidlScreenPart> ScreenParts { get; set; }
        [DataMember]
        public Dictionary<string, UidlUserAlert> UserAlerts { get; set; }
        [DataMember]
        public string DefaultInitialScreenQualifiedName { get; set; }
        [DataMember]
        public string DefaultSkin { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public UidlNotification RequestNotAuthorized { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public List<Type> RequiredDomainApis { get; private set; }
        public List<Type> RequiredDomainContexts { get; private set; }

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
    }
}
