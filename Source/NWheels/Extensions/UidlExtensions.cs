using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.UI.Uidl;

namespace NWheels.Extensions
{
    public static class UidlExtensions
    {
        public static TWidget Conditional<TWidget>(this TWidget widget, bool condition, Action<TWidget> ifTrue)
            where TWidget : WidgetUidlNode
        {
            if (condition)
            {
                ifTrue(widget);
            }

            return widget;
        }
    }
}
