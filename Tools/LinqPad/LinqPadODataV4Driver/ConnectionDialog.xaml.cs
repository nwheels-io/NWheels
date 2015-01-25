using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using LINQPad.Extensibility.DataContext;

namespace LinqPadODataV4Driver
{
	/// <summary>
	/// Interaction logic for ConnectionDialog.xaml
	/// </summary>
	public partial class ConnectionDialog : Window
	{
		ConnectionProperties _properties;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

		public ConnectionDialog (IConnectionInfo connectionInfo)
		{
            DataContext = _properties = new ConnectionProperties(connectionInfo);
			Background = SystemColors.ControlBrush;
			InitializeComponent();
		}

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

		void btnOK_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = true;
		}
	}
}
