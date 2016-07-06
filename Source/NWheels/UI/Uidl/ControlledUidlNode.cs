using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using NWheels.UI.Core;

namespace NWheels.UI.Uidl
{
    [DataContract(Namespace = UidlDocument.DataContractNamespace)]
    public abstract class ControlledUidlNode : InteractiveUidlNode
    {
        private readonly List<UidlExtensionRegistration> _registeredExtensions = new List<UidlExtensionRegistration>();

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected ControlledUidlNode(UidlNodeType nodeType, string idName, ControlledUidlNode parent)
            : base(nodeType, idName, parent)
        {
            this.Commands = new List<UidlCommandBase>();
            this.Behaviors = new List<BehaviorUidlNode>();
            this.DataBindings = new List<DataBindingUidlNode>();

            this.Loaded = new UidlNotification("Loaded", this);
            base.Notifications.Add(this.Loaded);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void AddCommands(params UidlCommand[] commands)
        {
            this.Commands.AddRange(commands);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void AttachExtensions(UidlExtensionRegistration[] registeredExtensions)
        {
            for (int i = 0; i < registeredExtensions.Length; i++)
            {
                if (registeredExtensions[i].NodeType.IsInstanceOfType(this))
                {
                    _registeredExtensions.Add(registeredExtensions[i]);
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataMember]
        public List<UidlCommandBase> Commands { get; set; } 
        [DataMember]
        public List<BehaviorUidlNode> Behaviors { get; set; }
        [DataMember]
        public List<DataBindingUidlNode> DataBindings { get; set; }
        [DataMember, ManuallyAssigned]
        public List<WidgetUidlNode> PopupContents { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public UidlNotification Loaded { get; private set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override IEnumerable<string> GetTranslatables()
        {
            return base.GetTranslatables().Concat(Commands.SelectMany(c => c.GetTranslatables()));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal string GetUniqueBehaviorId()
        {
            int index = 1;

            while ( Behaviors.Any(b => b.IdName == "B" + index) )
            {
                index++;
            }

            return "B" + index;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal void ApplyExtensions(UidlBuilder builder)
        {
            for (int i = 0 ; i < _registeredExtensions.Count ; i++)
            {
                var extension = (IUidlExtension)builder.Components.Resolve(_registeredExtensions[i].ExtensionType);
                extension.ApplyTo(this, builder);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal string ModelDataType { get; set; }
        internal string ModelStateType { get; set; }
    }
}
