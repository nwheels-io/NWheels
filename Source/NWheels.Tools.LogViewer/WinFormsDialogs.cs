using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NWheels.Tools.LogViewer
{
    public static class WinFormsDialogs
    {
        public static bool BrowseFolder(System.Windows.Media.Visual visual, ref string selectedPath)
        {
            using ( var dlg = new FolderBrowserDialog() )
            {
                dlg.RootFolder = Environment.SpecialFolder.MyComputer;

                if ( !string.IsNullOrEmpty(selectedPath) )
                {
                    dlg.SelectedPath = selectedPath;
                }

                var result = dlg.ShowDialog(GetIWin32Window(visual));

                if ( result == DialogResult.OK )
                {
                    selectedPath = dlg.SelectedPath;
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static System.Windows.Forms.IWin32Window GetIWin32Window(System.Windows.Media.Visual visual)
        {
            var source = System.Windows.PresentationSource.FromVisual(visual) as System.Windows.Interop.HwndSource;
            System.Windows.Forms.IWin32Window win = new OldWindow(source.Handle);
            return win;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class OldWindow : System.Windows.Forms.IWin32Window
        {
            private readonly System.IntPtr _handle;
            public OldWindow(System.IntPtr handle)
            {
                _handle = handle;
            }

            #region IWin32Window Members
            System.IntPtr System.Windows.Forms.IWin32Window.Handle
            {
                get { return _handle; }
            }
            #endregion
        }
    }
}
