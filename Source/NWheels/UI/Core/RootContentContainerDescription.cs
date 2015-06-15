using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.UI.Core
{
    public abstract class RootContentContainerDescription : NavigationTargetDescription
    {
        public WidgetDescription ContentRoot { get; set; }
    }
}
