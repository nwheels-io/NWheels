using NWheels.Domains.DevOps.SystemLogs.Entities;
using NWheels.Domains.DevOps.SystemLogs.Transactions;
using NWheels.UI;
using NWheels.UI.Toolbox;
using NWheels.UI.Uidl;

namespace NWheels.Domains.DevOps.SystemLogs.UI.ScreenParts
{
    public class LogLevelSummaryScreenPart : ScreenPartBase<LogLevelSummaryScreenPart, Empty.Context, Empty.Data, Empty.State>
    {
        public LogLevelSummaryScreenPart(string idName, UidlApplication parent)
            : base(idName, parent)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of ScreenPartBase<SystemLogScreenPart,Input,Data,State>

        protected override void DescribePresenter(PresenterBuilder<LogLevelSummaryScreenPart, Empty.Data, Empty.State> presenter)
        {
            ContentRoot = LogLevelSummary;

            LogLevelSummary.AutoSubmitOnLoad = true;
            LogLevelSummary.EnableVisualization();
            LogLevelSummary.VisualizationChart.TemplateName = "ChartInline";
            LogLevelSummary.VisualizationChart.Height = WidgetSize.MediumLarge;
            LogLevelSummary.CriteriaForm.AutoSubmitOnChange = true;
            LogLevelSummary.CriteriaForm.TemplateName = "FormInline";
            LogLevelSummary.CriteriaForm.IsInlineStyle = true;
            LogLevelSummary.CriteriaForm.Range(
                "TimeRange",
                x => x.From,
                x => x.Until,
                TimeRangePreset.Today,
                TimeRangePreset.Yesterday,
                TimeRangePreset.ThisWeek,
                TimeRangePreset.LastHour,
                TimeRangePreset.Last3Hours,
                TimeRangePreset.Last6Hours,
                TimeRangePreset.Last12Hours,
                TimeRangePreset.Last24Hours,
                TimeRangePreset.Last3Days,
                TimeRangePreset.Last7Days);

            LogLevelSummary.ResultTable
                .Column(x => x.Machine)
                .Column(x => x.Environment)
                .Column(x => x.Node)
                .Column(x => x.Instance, size: FieldSize.Small)
                .Column(x => x.Replica, size: FieldSize.Small)
                .Column(x => x.CriticalCount, title: "Critical", size: FieldSize.Small, format: "#,##0")
                .Column(x => x.ErrorCount, title: "Errors", size: FieldSize.Small, format: "#,##0")
                .Column(x => x.WarningCount, title: "Warnings", size: FieldSize.Small, format: "#,##0")
                .Column(x => x.InfoCount, title: "Info", size: FieldSize.Small, format: "#,##0")
                .Column(x => x.VerboseCount, title: "Verbose", size: FieldSize.Small, format: "#,##0")
                .Column(x => x.DebugCount, title: "Debug", size: FieldSize.Small, format: "#,##0");

            presenter.On(base.NavigatedHere)
                .Broadcast(LogLevelSummary.ContextSetter).WithPayload(vm => vm.Input).TunnelDown();

            LogLevelSummary.EnableVisualRangeSelection(b => 
                b.AlterModel(
                    alt => alt.Copy(vm => vm.Input.From).To(vm => vm.State.Criteria.From),
                    alt => alt.Copy(vm => vm.Input.To).To(vm => vm.State.Criteria.Until)));
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Report<
            Empty.Context, 
            ILogTimeRangeCriteria, 
            AbstractLogLevelSummaryTx, 
            ILogLevelSummaryEntity> LogLevelSummary { get; set; }
    }
}
