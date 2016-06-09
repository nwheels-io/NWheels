using System.Text;
using NWheels.Domains.DevOps.SystemLogs.Entities;
using NWheels.Domains.DevOps.SystemLogs.Transactions;
using NWheels.UI;
using NWheels.UI.Toolbox;
using NWheels.UI.Uidl;

namespace NWheels.Domains.DevOps.SystemLogs.UI.ScreenParts
{
    public class ThreadLogListScreenPart : ScreenPartBase<ThreadLogListScreenPart, Empty.Context, Empty.Data, Empty.State>
    {
        public ThreadLogListScreenPart(string idName, UidlApplication parent)
            : base(idName, parent)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of ScreenPartBase<SystemLogScreenPart,Input,Data,State>

        protected override void DescribePresenter(PresenterBuilder<ThreadLogListScreenPart, Empty.Data, Empty.State> presenter)
        {
            ContentRoot = Report;

            Report.AutoSubmitOnLoad = true;
            Report.EnableVisualization();
            Report.VisualizationChart.TemplateName = "ChartInline";
            Report.VisualizationChart.Height = WidgetSize.MediumLarge;
            Report.CriteriaForm.AutoSubmitOnChange = true;
            Report.CriteriaForm.TemplateName = "FormInline";
            Report.CriteriaForm.IsInlineStyle = true;
            Report.CriteriaForm.ConfigureLogTimeRange();

            Report.ResultTable
                .Column(x => x.Id)
                .Column(x => x.Timestamp, setup: c => c.SortByDefault(ascending: false))
                .Column(x => x.Machine, size: FieldSize.Small)
                .Column(x => x.Environment)
                .Column(x => x.Node, size: FieldSize.Small)
                .Column(x => x.Instance, size: FieldSize.Small)
                //.Column(x => x.Replica, size: FieldSize.Small)
                .Column(x => x.TaskType, size: FieldSize.Small)
                .Column(x => x.RootActivity, size: FieldSize.ExtraLarge)
                .Column(x => x.DurationMs, size: FieldSize.Small, format: "#,##0")
                .Column(x => x.Level, title: "Result", size: FieldSize.Small)
                .Column(x => x.RootMessageId, size: FieldSize.Large)
                .Column(x => x.ExceptionType, size: FieldSize.Large)
                //.Column(x => x.ThreadLogId)
                .Column(x => x.CorrelationId);

            Report.ResultTable.UseDetailPane(ThreadLogJson, expanded: false);

            presenter.On(base.NavigatedHere)
                .Broadcast(Report.ContextSetter).WithPayload(vm => vm.Input).TunnelDown();

            Report.EnableVisualRangeSelection(b => 
                b.AlterModel(
                    alt => alt.Copy(vm => vm.Input.From).To(vm => vm.State.Criteria.From),
                    alt => alt.Copy(vm => vm.Input.To).To(vm => vm.State.Criteria.Until)));
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Report<
            Empty.Context, 
            ILogTimeRangeCriteria, 
            AbstractThreadLogListTx, 
            IThreadLogEntity> Report { get; set; }

        public JsonText ThreadLogJson { get; set; }
    }
}
