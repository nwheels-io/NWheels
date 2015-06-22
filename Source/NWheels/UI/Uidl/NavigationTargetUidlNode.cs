using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.UI.Uidl
{
    [DataContract(Namespace = UidlDocument.DataContractNamespace)]
    public abstract class NavigationTargetUidlNode : ControlledUidlNode
    {
        protected NavigationTargetUidlNode(UidlNodeType nodeType, string idName, ControlledUidlNode parent)
            : base(nodeType, idName, parent)
        {
            this.NavigatedHere = new UidlNotification("NavigatedHere", this);
            base.Notifications.Add(this.NavigatedHere);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataMember]
        public string InputParameterType { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public UidlNotification NavigatedHere { get; private set; }
    }
}
