using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.UI.Uidl
{
    [DataContract(Namespace = UidlDocument.DataContractNamespace)]
    public abstract class InteractiveUidlNode : AbstractUidlNode
    {
        protected InteractiveUidlNode(UidlNodeType nodeType, string idName, AbstractUidlNode parent)
            : base(nodeType, idName, parent)
        {
            this.Notifications = new List<UidlNotification>();
            this.Translatables = new List<string>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataMember]
        public string Text { get; set; }
        [DataMember]
        public string HelpText { get; set; }
        [DataMember]
        public string Icon { get; set; }
        [DataMember]
        public bool Enabled { get; set; }
        [DataMember]
        public bool Authorized { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataMember]
        public List<UidlNotification> Notifications { get; set; }
        [DataMember]
        public List<string> Translatables { get; set; }
    }
}
