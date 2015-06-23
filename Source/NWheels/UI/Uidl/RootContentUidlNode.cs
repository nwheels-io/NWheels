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
    public abstract class RootContentUidlNode : NavigationTargetUidlNode
    {
        protected RootContentUidlNode(UidlNodeType nodeType, string idName, UidlApplication parent)
            : base(nodeType, idName, parent)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override IEnumerable<string> GetTranslatables()
        {
            if ( ContentRoot != null )
            {
                return base.GetTranslatables().Concat(ContentRoot.GetTranslatables());
            }
            else
            {
                return base.GetTranslatables();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataMember, ManuallyAssigned]
        public WidgetUidlNode ContentRoot { get; set; }
    }
}
