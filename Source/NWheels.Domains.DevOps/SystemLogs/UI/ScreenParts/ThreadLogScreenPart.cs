using NWheels.Domains.DevOps.SystemLogs.Entities;
using NWheels.Domains.DevOps.SystemLogs.Transactions;
using NWheels.Logging;
using NWheels.UI;
using NWheels.UI.Toolbox;
using NWheels.UI.Uidl;

namespace NWheels.Domains.DevOps.SystemLogs.UI.ScreenParts
{
    public class ThreadLogScreenPart : ScreenPartBase<ThreadLogScreenPart, Empty.Context, Empty.Data, Empty.State>
    {
        public ThreadLogScreenPart(string idName, UidlApplication parent)
            : base(idName, parent)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of ScreenPartBase<SystemLogScreenPart,Input,Data,State>

        protected override void DescribePresenter(PresenterBuilder<ThreadLogScreenPart, Empty.Data, Empty.State> presenter)
        {
            ContentRoot = Report;

            //Report.AutoSubmitOnLoad = true;
            //Report.EnableVisualization();
            //Report.VisualizationChart.TemplateName = "ChartInline";
            //Report.VisualizationChart.Height = WidgetSize.MediumLarge;
            //Report.CriteriaForm.AutoSubmitOnChange = true;
            Report.CriteriaForm.TemplateName = "FormInline";
            Report.CriteriaForm.IsInlineStyle = true;

            Report.ResultTable
                .Column(x => x.Text, size: FieldSize.Large, title: "Activity", setup: DisableSortingAndFiltering)
                .Column(x => x.TimeText, title: "Time", setup: f => {
                    DisableSortingAndFiltering(f);
                    f.Alignment = WidgetAlignment.Right;
                })
                .Column<IRootThreadLogUINodeEntity, string>(x => x.Node, size: FieldSize.Small, setup: DisableSortingAndFiltering)
                .Column(x => x.DurationMs, size: FieldSize.Small, format: "#,##0", setup: DisableSortingAndFiltering)
                .Column(x => x.DbDurationMs, size: FieldSize.Small, format: "#,##0", setup: DisableSortingAndFiltering)
                .Column(x => x.DbCount, size: FieldSize.Small, format: "#,##0", setup: DisableSortingAndFiltering)
                .Column(x => x.CpuTimeMs, size: FieldSize.Small, format: "#,##0", setup: DisableSortingAndFiltering)
                .Column<IRootThreadLogUINodeEntity, string>(x => x.LogId, setup: DisableSortingAndFiltering)
                .Column<IRootThreadLogUINodeEntity, string>(x => x.CorrelationId, setup: DisableSortingAndFiltering);

            Report.ResultTable.FlatStyle = true;
            Report.ResultTable
                .BindRowStyleTo(x => x.NodeType)
                .BindRowIconTo(x => x.Icon)
                .UseDetailPane(MessageDetails, expanded: false)
                .EnableExpandableTree(x => x.SubNodes);

            Report.ResultTable.EnablePaging = false;

            presenter.On(base.NavigatedHere)
                .Broadcast(Report.ContextSetter).WithPayload(vm => vm.Input).TunnelDown();
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public UidlNotification<string> ThreadLogRequested { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Report<
            Empty.Context,
            IThreadLogSearchCriteria,
            AbstractThreadLogUINodesTx,
            IRootThreadLogUINodeEntity> Report { get; set; }

        //public JsonText ThreadLogJson { get; set; }
        public PropertyGrid<IThreadLogUINodeEntity> MessageDetails { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        private void DisableSortingAndFiltering(DataGrid.GridColumn col)
        {
            col.IsFilterSupported = false;
            col.IsSortSupported = false;
        }
    }
}
