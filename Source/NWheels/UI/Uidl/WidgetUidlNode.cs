using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.UI.Uidl
{
    [DataContract(Namespace = UidlDocument.DataContractNamespace)]
    public abstract class WidgetUidlNode : ControlledUidlNode
    {
        protected WidgetUidlNode(string idName, ControlledUidlNode parent)
            : base(UidlNodeType.Widget, idName, parent)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual IEnumerable<WidgetUidlNode> GetNestedWidgets()
        {
            return new WidgetUidlNode[0];
        }
    }
}
