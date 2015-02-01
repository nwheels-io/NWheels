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

namespace NWheels.Tools.LogViewer
{
    /// <summary>
    /// Interaction logic for LogView.xaml
    /// </summary>
    public partial class LogView : UserControl
    {
        public LogView()
        {
            InitializeComponent();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void ItemExpandImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ((ITreeNodeItemEventHandlers)((FrameworkElement)sender).DataContext).ItemPreviewMouseLeftButtonDown(sender, e);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void ListViewItem_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            ((ITreeNodeItemEventHandlers)((FrameworkElement)sender).DataContext).ItemPreviewKeyDown(sender, e);
        }
        //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var viewModel = (LogPanelViewModel)this.DataContext;
            var selectedNodeItem = lvwLogs.SelectedItem as TreeNodeItem<ThreadLogViewModel.NodeItem>;

            if ( selectedNodeItem != null )
            {
                viewModel.SelectedNode = selectedNodeItem.Data;
            }
            else
            {
                viewModel.SelectedNode = null;
            }
        }
    }
}
