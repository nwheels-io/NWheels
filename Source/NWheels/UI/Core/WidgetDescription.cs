using System.Collections.Generic;

namespace NWheels.UI.Core
{
    public class WidgetDescription : UIElementContainerDescription
    {
        public WidgetDescription()
        {
            this.Widgets = new List<WidgetDescription>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public List<WidgetDescription> Widgets { get; private set; }
    }
}
