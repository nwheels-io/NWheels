using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using NWheels.Extensions;
using NWheels.Globalization.Core;
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

        public override IEnumerable<LocaleEntryKey> GetTranslatables()
        {
            if (ContentRoot != null)
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

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of AbstractUidlNode

        protected internal override void OnDeclaredMemberNodeCreated(PropertyInfo declaration, AbstractUidlNode instance)
        {
            base.OnDeclaredMemberNodeCreated(declaration, instance);

            var widget = (instance as WidgetUidlNode);

            if (widget != null && declaration.HasAttribute<ContentRootAttribute>())
            {
                this.ContentRoot = widget;
            }
        }

        #endregion
    }
}
