using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using NWheels.UI.Core;
using NWheels.UI.Uidl;

namespace NWheels.UI.Toolbox
{
    public class Container : WidgetUidlNode, UidlBuilder.IBuildableUidlNode
    {
        public Container(string idName, ControlledUidlNode parent)
            : base(idName, parent)
        {
            this.ContainedWidgets = new List<WidgetUidlNode>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void AddWidget(WidgetUidlNode widget)
        {
            this.ContainedWidgets.Add(widget);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override IEnumerable<WidgetUidlNode> GetNestedWidgets()
        {
            return ContainedWidgets;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataMember]
        public List<WidgetUidlNode> ContainedWidgets { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region IBuildableUidlNode Members

        void UidlBuilder.IBuildableUidlNode.Build(UidlBuilder builder)
        {
            builder.BuildNodes(ContainedWidgets.Cast<AbstractUidlNode>().ToArray());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        void UidlBuilder.IBuildableUidlNode.DescribePresenter(UidlBuilder builder)
        {
            builder.DescribeNodePresenters(ContainedWidgets.Cast<AbstractUidlNode>().ToArray());
        }

        #endregion
    }
}
