using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace NWheels.UI.Uidl
{
    [DataContract(Name = "CommandGroup", Namespace = UidlDocument.DataContractNamespace)]
    public class UidlCommandGroup<TItem> : UidlCommand
    {
        public UidlCommandGroup(string idName, AbstractUidlNode parent)
            : base(UidlNodeType.CommandGroup, idName, parent)
        {
            this.Executing = new UidlNotification<TItem>("Executing", this);
            this.Updating = new UidlNotification<TItem>("Updating", this);

            base.Notifications.Clear();
            base.Notifications.Add(this.Executing);
            base.Notifications.Add(this.Updating);

            this.Severity = CommandSeverity.Change;
            this.Items = new List<GroupItem>();

            TryAutoFillEnumGroupItems();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataMember]
        public List<GroupItem> Items { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public GroupItem this[TItem index]
        {
            get
            {
                var existingItem = this.Items.FirstOrDefault(item => item.Value.Equals(index));

                if ( existingItem != null )
                {
                    return existingItem;
                }

                var newItem = new GroupItem() {
                    Value = index
                };

                this.Items.Add(newItem);
                return newItem;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public new UidlNotification<TItem> Executing { get; private set; }
        public new UidlNotification<TItem> Updating { get; private set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override IEnumerable<string> GetTranslatables()
        {
            return base.GetTranslatables().Concat(Items.Select(item => item.Text));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void TryAutoFillEnumGroupItems()
        {
            if ( typeof(TItem).IsEnum )
            {
                Items.AddRange(Enum.GetValues(typeof(TItem)).Cast<TItem>().Select(v => new GroupItem() {
                    Value = v,
                    Text = v.ToString()
                }));
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataContract(Name = "GroupItem", Namespace = UidlDocument.DataContractNamespace)]
        public class GroupItem
        {
            [DataMember]
            public string Text { get; set; }
            [DataMember]
            public string Icon { get; set; }
            [DataMember]
            public TItem Value { get; set; }
            [DataMember]
            public bool IsChecked { get; set; }
        }
    }
}