using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.UI.Toolbox
{
    public class Menu : WidgetComponent<Menu, Empty.Data, Empty.State>
    {
        public override void DescribePresenter(IWidgetPresenter<Menu, Empty.Data, Empty.State> presenter)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public UIElementList<Item> Items { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class Item : UIElementComponent
        {
            public Item(string icon = null, object subItems = null)
            {
            }

            public INotification Selected { get; set; }
            public UIElementList<Item> SubItems { get; set; }
        }
    }
}
