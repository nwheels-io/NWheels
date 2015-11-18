using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using NWheels.UI.Core;

namespace NWheels.UI.Uidl
{
    [DataContract(Namespace = UidlDocument.DataContractNamespace)]
    public abstract class NavigationTargetUidlNode : ControlledUidlNode
    {
        protected NavigationTargetUidlNode(UidlNodeType nodeType, string idName, ControlledUidlNode parent)
            : base(nodeType, idName, parent)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataMember]
        public string InputParameterType { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [ManuallyAssigned]
        public UidlNotification NavigatedHere { get; protected set; }
    }
}
