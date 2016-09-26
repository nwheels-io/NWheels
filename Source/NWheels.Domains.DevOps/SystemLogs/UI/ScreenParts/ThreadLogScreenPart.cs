using NWheels.Domains.DevOps.SystemLogs.Entities;
using NWheels.Domains.DevOps.SystemLogs.Transactions;
using NWheels.Domains.DevOps.SystemLogs.UI.Formatters;
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

            Report.CriteriaForm.TemplateName = "FormInline";
            Report.CriteriaForm.IsInlineStyle = true;
            Report.CriteriaForm
                .Field(x => x.Id, setup: f => f.Size = WidgetSize.ExtraLarge)
                .Field(x => x.CorrelationId, setup: f => f.Size = WidgetSize.ExtraLarge);

            Report.ResultTable
                .Column(x => x.Text, size: FieldSize.Jumbo, title: "Activity", setup: DisableSortingAndFiltering)
                .Column(x => x.TimeText, title: "Time", setup: f => {
                    DisableSortingAndFiltering(f);
                    f.Alignment = WidgetAlignment.Right;
                })
                //.Column<IRootThreadLogUINodeEntity, string>(x => x.Node, size: FieldSize.Small, setup: DisableSortingAndFiltering)
                .Column(x => x.DurationMilliseconds, title: "Duration, ms", size: FieldSize.Small, format: "#,##0.00", setup: DisableSortingAndFiltering)
                .Column(x => x.DbDurationMilliseconds, title: "DB, ms", size: FieldSize.Small, format: "#,##0.00", setup: DisableSortingAndFiltering)
                .Column(x => x.DbCount, title: "DB, times", size: FieldSize.Small, format: "#,##0", setup: DisableSortingAndFiltering)
                .Column(x => x.CpuTimeMilliseconds, title: "CPU, ms", size: FieldSize.Small, format: "#,##0.00", setup: DisableSortingAndFiltering)
                .Column(x => x.WaitTimeMilliseconds, title: "Wait, ms", size: FieldSize.Small, format: "#,##0.00", setup: DisableSortingAndFiltering)
                .Column(x => x.NodeType, columnType: GridColumnType.Hidden)
                .Column(x => x.Icon, columnType: GridColumnType.Hidden)
                .Column(x => x.SubNodes, columnType: GridColumnType.Hidden);
                //.Column<IRootThreadLogUINodeEntity, string>(x => x.LogId, setup: DisableSortingAndFiltering)
                //.Column<IRootThreadLogUINodeEntity, string>(x => x.CorrelationId, setup: DisableSortingAndFiltering);

            Report.ResultTable.FlatStyle = true;
            Report.ResultTable
                .BindRowStyleTo(x => x.NodeType)
                .BindRowIconTo(x => x.Icon)
                .UseDetailPane(MessageDetails, expression: x => x.Details, expanded: false, queryServer: true)
                .EnableExpandableTree(x => x.SubNodes);

            Report.ResultTable.EnablePaging = false;
            Report.DownloadFormatIdName = ThreadLogTextDocumentFormatter.Format.IdName;
            
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
            IThreadLogUINodeEntity> Report { get; set; }

        public JsonText MessageDetails { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        private void DisableSortingAndFiltering(DataGrid.GridColumn col)
        {
            col.IsFilterSupported = false;
            col.IsSortSupported = false;
        }
    }
}
