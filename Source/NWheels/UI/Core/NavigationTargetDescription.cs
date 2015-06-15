using System;

namespace NWheels.UI.Core
{
    public abstract class NavigationTargetDescription : UIElementContainerDescription
    {
        public Type InputParameterType { get; set; }
    }
}
