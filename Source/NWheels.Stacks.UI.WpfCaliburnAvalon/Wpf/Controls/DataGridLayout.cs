using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows.Controls;
using System.Windows.Data;

namespace NWheels.Stacks.UI.WpfCaliburnAvalon.Wpf.Controls
{
    [DataContract(Namespace = "", Name = "DataGrid")]
    public class DataGridLayout
    {
        public DataGridLayout(DataGrid grid)
        {
            Columns = grid.Columns.Select(c => new ColumnInfo(c)).ToList();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void ApplyTo(DataGrid grid)
        {
            foreach (var columnInfo in Columns)
            {
                var gridColumn = grid.Columns.FirstOrDefault(x => columnInfo.Header.Equals(x.Header));

                if (gridColumn != null)
                {
                    columnInfo.Apply(gridColumn, Columns.Count, grid.Items.SortDescriptions);
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        [DataMember]
        public IList<ColumnInfo> Columns { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataContract(Namespace = "", Name = "Column")]
        public class ColumnInfo
        {
            public ColumnInfo(DataGridColumn column)
            {
                Header = column.Header;
                PropertyPath = ((Binding) ((DataGridBoundColumn) column).Binding).Path.Path;
                WidthValue = column.Width.DisplayValue;
                WidthType = column.Width.UnitType;
                SortDirection = column.SortDirection;
                DisplayIndex = column.DisplayIndex;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void Apply(DataGridColumn column, int gridColumnCount, SortDescriptionCollection sortDescriptions)
            {
                column.Width = new DataGridLength(WidthValue, WidthType);
                column.SortDirection = SortDirection;

                if (SortDirection != null)
                {
                    sortDescriptions.Add(new SortDescription(PropertyPath, SortDirection.Value));
                }

                if (column.DisplayIndex != DisplayIndex)
                {
                    var maxIndex = (gridColumnCount == 0) ? 0 : gridColumnCount - 1;
                    column.DisplayIndex = (DisplayIndex <= maxIndex) ? DisplayIndex : maxIndex;
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            [DataMember]
            public object Header { get; set; }
            [DataMember]
            public string PropertyPath { get; set; }
            [DataMember]
            public ListSortDirection? SortDirection { get; set; }
            [DataMember]
            public int DisplayIndex { get; set; }
            [DataMember]
            public double WidthValue { get; set; }
            [DataMember]
            public DataGridLengthUnitType WidthType { get; set; }
        }
    }
}
