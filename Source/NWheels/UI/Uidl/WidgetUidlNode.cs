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
            this.WidgetType = this.GetType().Name;
            this.TemplateName = this.GetType().Name;
            TemplateAttribute.ApplyIfDefined(this.GetType(), this);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual IEnumerable<WidgetUidlNode> GetNestedWidgets()
        {
            return new WidgetUidlNode[0];
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override IEnumerable<string> GetTranslatables()
        {
            return base.GetTranslatables().Concat(GetNestedWidgets().Where(widget => widget != null).SelectMany(widget => widget.GetTranslatables()));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataMember]
        public string WidgetType { get; set; }
        [DataMember]
        public string TemplateName { get; set; }
        [DataMember]
        public bool IsApplicationTemplate { get; set; }
    }
}
