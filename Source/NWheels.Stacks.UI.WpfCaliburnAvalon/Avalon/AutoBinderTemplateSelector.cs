using System.Windows;
using System.Windows.Controls;

namespace NWheels.Stacks.UI.WpfCaliburnAvalon.Avalon
{
    public class AutobinderTemplateSelector : DataTemplateSelector
    {
        public DataTemplate Template { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            return Template;
        }
    }

    public class AutobinderTemplate : DataTemplate
    {

    }
}