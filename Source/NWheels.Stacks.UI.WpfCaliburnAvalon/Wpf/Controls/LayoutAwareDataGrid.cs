using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace NWheels.Stacks.UI.WpfCaliburnAvalon.Wpf.Controls
{
    public class LayoutAwareDataGrid  : DataGrid
    {
        private bool _inWidthChange = false;
        private bool _updatingColumnInfo = false;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public DataGridLayout DataGridLayout
        {
            get
            {
                return (DataGridLayout)GetValue(DataGridLayoutProperty);
            }
            set
            {
                SetValue(DataGridLayoutProperty, value);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnInitialized(EventArgs e)
        {
            EventHandler sortDirectionChangedHandler = (sender, x) => UpdateDataGridLayout();
            EventHandler widthPropertyChangedHandler = (sender, x) => _inWidthChange = true;
            var sortDirectionPropertyDescriptor = DependencyPropertyDescriptor.FromProperty(DataGridColumn.SortDirectionProperty, typeof(DataGridColumn));
            var widthPropertyDescriptor = DependencyPropertyDescriptor.FromProperty(DataGridColumn.WidthProperty, typeof(DataGridColumn));

            Loaded += (sender, x) => {
                foreach (var column in Columns)
                {
                    sortDirectionPropertyDescriptor.AddValueChanged(column, sortDirectionChangedHandler);
                    widthPropertyDescriptor.AddValueChanged(column, widthPropertyChangedHandler);
                }
            };
            Unloaded += (sender, x) => {
                foreach (var column in Columns)
                {
                    sortDirectionPropertyDescriptor.RemoveValueChanged(column, sortDirectionChangedHandler);
                    widthPropertyDescriptor.RemoveValueChanged(column, widthPropertyChangedHandler);
                }
            };

            base.OnInitialized(e);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void UpdateDataGridLayout()
        {
            _updatingColumnInfo = true;
            DataGridLayout = new DataGridLayout(this);
            _updatingColumnInfo = false;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        protected override void OnColumnReordered(DataGridColumnEventArgs e)
        {
            UpdateDataGridLayout();
            base.OnColumnReordered(e);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        protected override void OnPreviewMouseLeftButtonUp(System.Windows.Input.MouseButtonEventArgs e)
        {
            if (_inWidthChange)
            {
                _inWidthChange = false;
                UpdateDataGridLayout();
            }
            base.OnPreviewMouseLeftButtonUp(e);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static readonly DependencyProperty DataGridLayoutProperty = DependencyProperty.Register(
            "DataGridLayout",
            typeof(DataGridLayout),
            typeof(LayoutAwareDataGrid),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, DataGridLayoutChangedCallback));

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static void DataGridLayoutChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var grid = (LayoutAwareDataGrid)dependencyObject;
            var layout = e.NewValue as DataGridLayout;

            if (layout != null && !grid._updatingColumnInfo)
            {
                layout.ApplyTo(grid);
            }
        }
    }
}
