using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace NWheels.Tools.TestBoard.Extensions
{
    public static class DependencyObjectExtensions
    {
        public static TElement VisualUpwardSearch<TElement>(this DependencyObject source)
            where TElement : FrameworkElement
        {
            while ( source != null && !(source is TElement) )
            {
                source = VisualTreeHelper.GetParent(source);
            }
            
            return source as TElement;
        }
    }
}
