using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using NWheels.UI.Uidl;

namespace NWheels.UI.Toolbox
{
    [DataContract(Namespace = UidlDocument.DataContractNamespace)]
    public class Dashboard : WidgetBase<Dashboard, Empty.Data, Empty.State>, UidlBuilder.IBuildableUidlNode
    {
        public Dashboard(string idName, ControlledUidlNode parent)
            : base(idName, parent)
        {
            this.Widgets = new List<WidgetUidlNode>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void AddWidgets(params WidgetUidlNode[] widgets)
        {
            this.Widgets.AddRange(widgets);
        }
        
        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void DescribePresenter(PresenterBuilder<Dashboard, Empty.Data, Empty.State> presenter)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override IEnumerable<WidgetUidlNode> GetNestedWidgets()
        {
            return Widgets;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataMember]
        public List<WidgetUidlNode> Widgets { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region IBuildableUidlNode Members

        void UidlBuilder.IBuildableUidlNode.Build(UidlBuilder builder)
        {
            builder.BuildNodes(Widgets.Cast<AbstractUidlNode>().ToArray());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        void UidlBuilder.IBuildableUidlNode.DescribePresenter(UidlBuilder builder)
        {
            builder.DescribeNodePresenters(Widgets.Cast<AbstractUidlNode>().ToArray());
        }

        #endregion
    }
}
