using System.Collections.Generic;
using NWheels.UI.Core;

namespace NWheels.UI.OldToolbox
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
                    base.Notifications.Add(Selected);
                    SubItems = new List<GeneratedDescription>();
                    SubItems.AddRange(subItems);
                }

                //-------------------------------------------------------------------------------------------------------------------------------------------------

                [DuplicateReference]
                public NotificationDescription Selected { get; set; }
                public List<GeneratedDescription> SubItems { get; set; }
                public int Level { get; set; }
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
