using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using NWheels.Extensions;
using NWheels.UI.Core;
using NWheels.UI.Uidl;

namespace NWheels.UI.Toolbox
{
    [DataContract(Namespace = UidlDocument.DataContractNamespace)]
    public class DataGrid : WidgetBase<DataGrid, Empty.Data, Empty.State>
    {
        public DataGrid(string idName, ControlledUidlNode parent)
            : base(idName, parent)
        {
            DisplayColumns = new List<string>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void DescribePresenter(PresenterBuilder<DataGrid, Empty.Data, Empty.State> presenter)
        {
            if ( RowTemplate == null )
            {
                RowTemplate = DefaultRowTemplate;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataMember]
        public string EntityName { get; set; }
        [DataMember]
        public string DataQuery { get; set; }
        [DataMember]
        public List<string> DisplayColumns { get; set; }
        [DataMember, ManuallyAssigned]
        public WidgetUidlNode RowTemplate { get; set; }
        [DataMember]
        public DataGridDefaultRow DefaultRowTemplate { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override IEnumerable<string> GetTranslatables()
        {
            return base.GetTranslatables().Concat(DisplayColumns ?? new List<string>());
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class DataGridDefaultRow : WidgetBase<DataGridDefaultRow, Empty.Data, Empty.State>
    {
        public DataGridDefaultRow(string idName, ControlledUidlNode parent)
            : base(idName, parent)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void DescribePresenter(PresenterBuilder<DataGridDefaultRow, Empty.Data, Empty.State> presenter)
        {
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    [DataContract(Namespace = UidlDocument.DataContractNamespace, Name = "Crud")]
    public class DataGrid<TDataRow> : DataGrid
    {
        public DataGrid(string idName, ControlledUidlNode parent)
            : base(idName, parent)
        {
            this.WidgetType = "DataGrid";
            this.TemplateName = "DataGrid";
            this.EntityName = typeof(TDataRow).Name.TrimLead("I").TrimTail("Entity");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public DataGrid<TDataRow> Column<T>(Expression<Func<TDataRow, T>> propertySelector)
        {
            var property = propertySelector.GetPropertyInfo();

            if ( this.DisplayColumns == null )
            {
                this.DisplayColumns = new List<string>();
            }

            this.DisplayColumns.Add(property.Name);
            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public UidlNotification<TDataRow[]> DataReceived { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnBuild(UidlBuilder builder)
        {
            builder.RegisterMetaType(typeof(TDataRow));
            base.EntityName = MetadataCache.GetTypeMetadata(typeof(TDataRow)).QualifiedName;
        }
    }
}
