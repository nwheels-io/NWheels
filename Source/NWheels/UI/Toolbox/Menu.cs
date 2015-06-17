using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.UI.Core;

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

        public class Item : UIElementContainerComponent
        {
            public Item(string icon = null, object subItems = null)
            {
            }

            public INotification Selected { get; set; }
            public UIElementList<Item> SubItems { get; set; }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public class GeneratedDescription : UIElementDescription
            {
                public GeneratedDescription(string idName, UIContentElementDescription parent, params GeneratedDescription[] subItems)
                    : base(idName, parent)
                {
                    Selected = new NotificationDescription("Selected", this);
                    SubItems = new List<GeneratedDescription>();
                    SubItems.AddRange(subItems);
                }

                //-------------------------------------------------------------------------------------------------------------------------------------------------

                public NotificationDescription Selected { get; set; }
                public List<GeneratedDescription> SubItems { get; set; }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class GeneratedDescription : WidgetDescription
        {
            public GeneratedDescription(string idName, UIContentElementDescription parent)
                : base(idName, parent)
            {
                this.Items = new List<Item.GeneratedDescription>();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public List<Item.GeneratedDescription> Items { get; set; }
        }
    }
}
