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

namespace DataContextDriverDemo.Astoria
{
	/// <summary>
	/// Interaction logic for ConnectionDialog.xaml
	/// </summary>
	public partial class ConnectionDialog : Window
	{
		AstoriaProperties _properties;

		public ConnectionDialog (IConnectionInfo cxInfo)
		{
			DataContext = _properties = new AstoriaProperties (cxInfo);
			Background = SystemColors.ControlBrush;
			InitializeComponent ();
		}	

		void btnOK_Click (object sender, RoutedEventArgs e)
		{
			DialogResult = true;
		}
	}
}
