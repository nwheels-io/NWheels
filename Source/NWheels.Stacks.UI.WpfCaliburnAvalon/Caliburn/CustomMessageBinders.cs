using System.Windows;
using Caliburn.Micro;
using Xceed.Wpf.AvalonDock;
using Xceed.Wpf.AvalonDock.Layout;

namespace NWheels.Stacks.UI.WpfCaliburnAvalon.Caliburn
{
    public static class CustomMessageBinders
    {
        public static void Configure()
        {
            ConfigureDocumentContextBinder();
            ConfigureOriginalSourceContextBinder();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static void ConfigureOriginalSourceContextBinder()
        {
            MessageBinder.SpecialValues.Add("$orignalsourcecontext", context => {
                var args = context.EventArgs as RoutedEventArgs;
                if (args == null)
                {
                    return null;
                }

                var fe = args.OriginalSource as FrameworkElement;
                if (fe == null)
                {
                    return null;
                }

                return fe.DataContext;
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static void ConfigureDocumentContextBinder()
        {
            MessageBinder.SpecialValues.Add("$documentcontext", context => {
                LayoutDocument doc = null;
                if (context.EventArgs is DocumentClosingEventArgs)
                {
                    var args = context.EventArgs as DocumentClosingEventArgs;

                    doc = args.Document;
                }
                else if (context.EventArgs is DocumentClosedEventArgs)
                {
                    var args = context.EventArgs as DocumentClosedEventArgs;
                    doc = args.Document;
                }

                return doc.Content;
            });
        }
    }
}
