using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.UI.Core;

namespace NWheels.UI.Toolbox
{
    public class Container : WidgetComponent<Container, Empty.Data, Empty.State>
    {
        public override void DescribePresenter(IWidgetPresenter<Container, Empty.Data, Empty.State> presenter)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public UIElementList<IWidget> ContainedWidgets { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class GeneratedDescription : WidgetDescription
        {
            public GeneratedDescription(string idName, UIContentElementDescription parent)
                : base(idName, parent)
            {
                base.ElementType = "Container";
                this.ContainedWidgets = new List<WidgetDescription>();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public List<WidgetDescription> ContainedWidgets { get; private set; }
        }
    }
}
