using System.Collections.Generic;

namespace NWheels.UI.Core
{
    public abstract class WidgetDescription : UIContentElementDescription
    {
        protected WidgetDescription(string idName, UIContentElementDescription parent)
            : base(idName, parent)
        {
            base.NodeType = UINodeType.Widget;
        }
    }
}
