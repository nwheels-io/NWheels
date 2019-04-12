using System;

namespace NWheels.UI.Model
{
    public class StackLayoutProps : PropsOf<StackLayout>
    {
        public StackLayoutProps Row(UIComponent component) => default;
        public StackLayoutProps Column(UIComponent component) => default;
    }
    
    public class StackLayout : LayoutComponent<StackLayoutProps, Empty.State>
    {
        public StackLayout(Action<StackLayoutProps> setProps)
        {
        }
    }
}
