using NWheels.Domains.DevOps.SystemLogs.Entities;
using NWheels.Domains.DevOps.SystemLogs.Transactions;
using NWheels.UI;
using NWheels.UI.Toolbox;
using NWheels.UI.Uidl;

namespace NWheels.Domains.DevOps.SystemLogs.UI
{
    public class SystemLogScreenPart : ScreenPartBase<SystemLogScreenPart, Empty.Context, Empty.Data, Empty.State>
    {
        public SystemLogScreenPart(string idName, UidlApplication parent)
            : base(idName, parent)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of ScreenPartBase<SystemLogScreenPart,Input,Data,State>

        protected override void DescribePresenter(PresenterBuilder<SystemLogScreenPart, Empty.Data, Empty.State> presenter)
        {
            ContentRoot = LogLevelSummary;

            LogLevelSummary.AutoSubmitOnLoad = true;
            LogLevelSummary.SummaryChart.TemplateName = "ChartInline";
            LogLevelSummary.SummaryChart.Height = WidgetSize.Large;
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
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ChartTableReport<
            Empty.Context, 
            ILogTimeRangeCriteria, 
            AbstractLogLevelSummaryChartTx, 
            AbstractLogLevelSummaryListTx, 
            ILogLevelSummaryEntity> LogLevelSummary { get; set; }
    }
}
