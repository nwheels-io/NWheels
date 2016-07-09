using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using NWheels.Globalization.Core;

namespace NWheels.UI.Uidl
{
    [DataContract(Name = "Command", Namespace = UidlDocument.DataContractNamespace)]
    public abstract class UidlCommandBase : InteractiveUidlNode
    {
        protected UidlCommandBase(UidlNodeType nodeType, string idName, AbstractUidlNode parent)
            : base(nodeType, idName, parent)
        {
            this.Severity = CommandSeverity.Change;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override IEnumerable<LocaleEntryKey> GetTranslatables()
        {
            return base.GetTranslatables().Concat(LocaleEntryKey.Enumerate(
                this, 
                this.Warning, "Warning"
            ));
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
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    [DataContract(Name = "Command", Namespace = UidlDocument.DataContractNamespace)]
    public class UidlCommand<TArgument> : UidlCommandBase
    {
        public UidlCommand(string idName, AbstractUidlNode parent)
            : this(UidlNodeType.Command, idName, parent)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected UidlCommand(UidlNodeType nodeType, string idName, AbstractUidlNode parent)
            : base(nodeType, idName, parent)
        {
            this.Executing = new UidlNotification<TArgument>("Executing", this);
            this.Updating = new UidlNotification<TArgument>("Updating", this);

            base.Notifications.Add(this.Executing);
            base.Notifications.Add(this.Updating);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public UidlNotification<TArgument> Executing { get; private set; }
        public UidlNotification<TArgument> Updating { get; private set; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    [DataContract(Name = "Command", Namespace = UidlDocument.DataContractNamespace)]
    public class UidlCommand : UidlCommand<Empty.Payload>
    {
        public UidlCommand(string idName, AbstractUidlNode parent)
            : base(idName, parent)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected UidlCommand(UidlNodeType nodeType, string idName, AbstractUidlNode parent)
            : base(nodeType, idName, parent)
        {
        }
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
