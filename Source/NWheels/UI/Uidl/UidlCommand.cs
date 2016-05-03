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

        public override IEnumerable<string> GetTranslatables()
        {
            return base.GetTranslatables().ConcatOneIf(this.Warning);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataMember]
        public CommandKind Kind { get; set; }
        [DataMember]
        public CommandSeverity Severity { get; set; }
        [DataMember]
        public CommandUIStyle UIStyle { get; set; }
        [DataMember]
        public bool HiddenIfDisabled { get; set; }
        [DataMember]
        public string Warning { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public UidlNotification Executing { get; private set; }
        public UidlNotification Updating { get; private set; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public enum CommandSeverity
    {
        None = 0,
        Read = 10,
        Change = 20,
        Loose = 30,
        Destroy = 40,
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public enum CommandKind
    {
        Other = 0,
        Submit = 10,
        Reject = 20,
        Navigate = 30
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public enum CommandUIStyle
    {
        Unspecified = 0,
        Button = 10,
        Link = 20
    }
}
