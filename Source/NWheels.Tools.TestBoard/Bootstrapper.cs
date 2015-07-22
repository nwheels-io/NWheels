using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Gemini;
using Gemini.Modules.Output;
using NWheels.Tools.TestBoard.Modules.Main;

namespace NWheels.Tools.TestBoard
{
    public class Bootstrapper : AppBootstrapper
    {
        protected override void OnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            try
            {
                var output = base.Container.GetExportedValue<IOutput>();
                
                output.AppendLine(string.Format("ERROR: {0}", e.Exception.Message));
                output.AppendLine("-------- error details --------");
                output.AppendLine(e.Exception.ToString());
                output.AppendLine("----- end of error details ----");
                
                e.Handled = true;
            }
            catch
            {
                try
                {
                    MessageBox.Show(
                        string.Format("An internal error has occurred. See details below.\n\n{0}", e.Exception),
                        MainModule.MainWindowTitle,
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    
                    e.Handled = true;
                }
                catch
                {
                }
            }
        }
    }
}
