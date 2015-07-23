using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;
using System.Windows.Threading;

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
    }
}
