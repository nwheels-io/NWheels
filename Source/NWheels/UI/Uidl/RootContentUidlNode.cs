using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.UI.Uidl
{
    [DataContract(Namespace = UidlDocument.DataContractNamespace)]
    public abstract class RootContentUidlNode : NavigationTargetUidlNode
    {
        protected RootContentUidlNode(UidlNodeType nodeType, string idName, UidlApplication parent)
            : base(nodeType, idName, parent)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataMember]
        public WidgetUidlNode ContentRoot { get; set; }
    }
}
