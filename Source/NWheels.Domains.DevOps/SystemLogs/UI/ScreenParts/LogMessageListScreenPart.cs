using NWheels.Domains.DevOps.SystemLogs.Entities;
using NWheels.Domains.DevOps.SystemLogs.Transactions;
using NWheels.UI;
using NWheels.UI.Toolbox;
using NWheels.UI.Uidl;

namespace NWheels.Domains.DevOps.SystemLogs.UI.ScreenParts
{
    public class LogMessageListScreenPart : ScreenPartBase<LogMessageListScreenPart, Empty.Context, Empty.Data, Empty.State>
    {
        public LogMessageListScreenPart(string idName, UidlApplication parent)
            : base(idName, parent)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of ScreenPartBase<SystemLogScreenPart,Input,Data,State>

        protected override void DescribePresenter(PresenterBuilder<LogMessageListScreenPart, Empty.Data, Empty.State> presenter)
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
                .Column(x => x.Timestamp, setup: c => c.SortByDefault(ascending: false))
                .Column(x => x.Machine, size: FieldSize.Small)
                .Column(x => x.Environment)
                .Column(x => x.Node, size: FieldSize.Small)
                .Column(x => x.Instance, size: FieldSize.Small)
                //.Column(x => x.Replica, size: FieldSize.Small)
                .Column(x => x.Level)
                .Column(x => x.Logger)
                .Column(x => x.MessageId, size: FieldSize.Large)
                .Column(x => x.ExceptionType, size: FieldSize.Large)
                .Column(x => x.ThreadLogId)
                //.Column(x => x.CorrelationId)
                .Column(x => x.KeyValues, size: FieldSize.Large);

            Report.ResultTable.UseDetailPane(MessageJson, expanded: false);

            presenter.On(base.NavigatedHere)
                .Broadcast(Report.ContextSetter).WithPayload(vm => vm.Input).TunnelDown();

            Report.EnableVisualRangeSelection(b => 
                b.AlterModel(
                    alt => alt.Copy(vm => vm.Input.From).To(vm => vm.State.Criteria.From),
                    alt => alt.Copy(vm => vm.Input.To).To(vm => vm.State.Criteria.Until)));

            MessageJson.ExpandedByDefault = true;
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Report<
            Empty.Context, 
            ILogTimeRangeCriteria, 
            AbstractLogMessageListTx, 
            ILogMessageEntity> Report { get; set; }

        public JsonText MessageJson { get; set; }
    }
}
