using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using NWheels.Extensions;

namespace NWheels.UI.Uidl
{
    [DataContract(Name = "Command", Namespace = UidlDocument.DataContractNamespace)]
    public class UidlCommand : InteractiveUidlNode
    {
        public UidlCommand(string idName, AbstractUidlNode parent)
            : this(UidlNodeType.Command, idName, parent)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected UidlCommand(UidlNodeType nodeType, string idName, AbstractUidlNode parent)
            : base(nodeType, idName, parent)
        {
            this.Executing = new UidlNotification("Executing", this);
            this.Updating = new UidlNotification("Updating", this);

            base.Notifications.Add(this.Executing);
            base.Notifications.Add(this.Updating);

            this.Severity = CommandSeverity.Change;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataMember]
        public CommandKind Kind { get; set; }
        [DataMember]
        public CommandSeverity Severity { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public UidlNotification Executing { get; private set; }
        public UidlNotification Updating { get; private set; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public enum CommandSeverity
    {
        Read,
        Change,
        Loose,
        Destroy,
        None
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public enum CommandKind
    {
        Other,
        Submit,
        Reject
    }
}
