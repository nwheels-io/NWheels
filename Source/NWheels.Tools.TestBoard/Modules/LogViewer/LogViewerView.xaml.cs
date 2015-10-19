using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Threading;
using NWheels.Extensions;

namespace NWheels.Tools.TestBoard.Modules.LogViewer
{
    /// <summary>
    /// Interaction logic for LogViewerDocumentView.xaml
    /// </summary>
    public partial class LogViewerView : UserControl
    {
        private readonly DispatcherTimer _displayTimer;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public LogViewerView()
        {
            InitializeComponent();

            _displayTimer = new DispatcherTimer();
            _displayTimer.Interval = TimeSpan.FromMilliseconds(1000);
            _displayTimer.Tick += OnDisplayTimerTick;
            _displayTimer.Start();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void OnDisplayTimerTick(object sender, EventArgs e)
        {
            ((LogViewerViewModel)DataContext).Logs.DisplayPendingLogs();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void ClearLogs(object sender, ExecutedRoutedEventArgs e)
        {
            ((LogViewerViewModel)DataContext).Logs.Clear();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void LogView_DragEnter(object sender, DragEventArgs e)
        {
            string[] fileNames;
            bool allowDrop = GetDroppedFileNames(e, out fileNames);
            
            e.Effects = allowDrop ? DragDropEffects.Link : DragDropEffects.None;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void LogView_Drop(object sender, DragEventArgs e)
        {
            string[] fileNames;
            bool allowDrop = GetDroppedFileNames(e, out fileNames);
            
            if ( allowDrop )
            {
                var logs = ((LogViewerViewModel)DataContext).Logs;

                foreach ( var fileName in fileNames )
                {
                    logs.AddLogFromFile(fileName);
                }

                logs.DisplayPendingLogs(bypassFilter: true);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private bool GetDroppedFileNames(DragEventArgs e, out string[] files)
        {
            bool isFileLegal = false;
            files = null;

            if ( e.Data.GetDataPresent(DataFormats.FileDrop) )
            {
                files = ((string[])e.Data.GetData(DataFormats.FileDrop)).Where(IsFilePathLegal).ToArray();
                isFileLegal = files.Any();
            }

            return isFileLegal;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static bool IsFilePathLegal(string filePath)
        {
            var extension = Path.GetExtension(filePath);
            return extension.EqualsIgnoreCase(".threadlog");
        }
    }
}
