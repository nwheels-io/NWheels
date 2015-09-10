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
    public class Report : WidgetBase<Report, Empty.Data, Empty.State>
    {
        public Report(string idName, ControlledUidlNode parent)
            : base(idName, parent)
        {
            DisplayColumns = new List<string>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void DescribePresenter(PresenterBuilder<Report, Empty.Data, Empty.State> presenter)
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
        public string EntityMetaType { get; set; }
        [DataMember]
        public List<string> DisplayColumns { get; set; }
        [DataMember, ManuallyAssigned]
        public WidgetUidlNode RowTemplate { get; set; }
        public ReportDefaultRow DefaultRowTemplate { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override IEnumerable<string> GetTranslatables()
        {
            return base.GetTranslatables().Concat(DisplayColumns ?? new List<string>());
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class ReportDefaultRow : WidgetBase<ReportDefaultRow, Empty.Data, Empty.State>
    {
        public ReportDefaultRow(string idName, ControlledUidlNode parent)
            : base(idName, parent)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void DescribePresenter(PresenterBuilder<ReportDefaultRow, Empty.Data, Empty.State> presenter)
        {
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    [DataContract(Namespace = UidlDocument.DataContractNamespace, Name = "Crud")]
    public class Report<TEntity> : Report
        where TEntity : class
    {
        public Report(string idName, ControlledUidlNode parent)
            : base(idName, parent)
        {
            this.WidgetType = "Report";
            this.TemplateName = "Report";
            this.EntityName = typeof(TEntity).Name.TrimLead("I").TrimTail("Entity");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Report<TEntity> Column<T>(Expression<Func<TEntity, T>> propertySelector)
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

        protected override void OnBuild(UidlBuilder builder)
        {
            base.EntityMetaType = builder.RegisterMetaType(typeof(TEntity));
        }
    }
}
