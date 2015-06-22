using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.UI.Uidl
{
    [DataContract(Name = "Command", Namespace = UidlDocument.DataContractNamespace)]
    public class UidlCommand : InteractiveUidlNode
    {
        public UidlCommand(string idName, AbstractUidlNode parent)
            : base(UidlNodeType.Command, idName, parent)
        {
            this.Executing = new UidlNotification("Executing", this);
            this.Updating = new UidlNotification("Updating", this);

            base.Notifications.Add(this.Executing);
            base.Notifications.Add(this.Updating);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public UidlNotification Executing { get; private set; }
        public UidlNotification Updating { get; private set; }
    }
}
