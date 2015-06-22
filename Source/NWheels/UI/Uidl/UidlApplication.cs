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

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public UidlNotification RequestNotAuthorized { get; set; }
    }
}
