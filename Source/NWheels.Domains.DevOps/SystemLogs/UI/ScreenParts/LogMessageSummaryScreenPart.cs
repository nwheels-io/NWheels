using NWheels.Domains.DevOps.SystemLogs.Entities;
using NWheels.Domains.DevOps.SystemLogs.Transactions;
using NWheels.UI;
using NWheels.UI.Toolbox;
using NWheels.UI.Uidl;

namespace NWheels.Domains.DevOps.SystemLogs.UI.ScreenParts
{
    public class LogMessageSummaryScreenPart : ScreenPartBase<LogMessageSummaryScreenPart, Empty.Context, Empty.Data, Empty.State>
    {
        public LogMessageSummaryScreenPart(string idName, UidlApplication parent)
            : base(idName, parent)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of ScreenPartBase<SystemLogScreenPart,Input,Data,State>

        protected override void DescribePresenter(PresenterBuilder<LogMessageSummaryScreenPart, Empty.Data, Empty.State> presenter)
        {
            ContentRoot = Report;

            Report.AutoSubmitOnLoad = true;
            Report.EnableVisualization();
            Report.VisualizationChart.TemplateName = "ChartInline";
            Report.VisualizationChart.Height = WidgetSize.Large;
            Report.VisualizationChart.Palette = ChartData.SeriesPalette.PassFailSeries;
            Report.VisualizationChart.LogarithmicScale = true;
            Report.CriteriaForm.AutoSubmitOnChange = true;
            Report.CriteriaForm.TemplateName = "FormInline";
            Report.CriteriaForm.IsInlineStyle = true;
            Report.CriteriaForm.ConfigureLogTimeRange();

            Report.ResultTable
                .Column(x => x.Machine, size: FieldSize.Small)
                .Column(x => x.Environment)
                .Column(x => x.Node, size: FieldSize.Small)
                .Column(x => x.Instance, size: FieldSize.Small)
                //.Column(x => x.Replica, size: FieldSize.Small)
                .Column(x => x.Level, size: FieldSize.Small)
                .Column(x => x.Logger)
                .Column(x => x.MessageId, size: FieldSize.Large)
                .Column(x => x.ExceptionType)
                .Column(x => x.Count, size: FieldSize.Small, format: "#,##0")
                .Column(x => x.CriticalCount, title: "Critical", size: FieldSize.Small, format: "#,##0")
                .Column(x => x.ErrorCount, title: "Errors", size: FieldSize.Small, format: "#,##0")
                .Column(x => x.WarningCount, title: "Warnings", size: FieldSize.Small, format: "#,##0");

            presenter.On(base.NavigatedHere)
                .Broadcast(Report.ContextSetter).WithPayload(vm => vm.Input).TunnelDown();

            Report.EnableVisualRangeSelection(b => 
                b.AlterModel(
                    alt => alt.Copy(vm => vm.Input.From).To(vm => vm.State.Criteria.From),
                    alt => alt.Copy(vm => vm.Input.To).To(vm => vm.State.Criteria.Until),
                    alt => alt.Copy(vm => vm.Input.SeriesIndex).To(vm => vm.State.Criteria.SeriesIndex)));
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Report<
            Empty.Context, 
            ILogTimeRangeCriteria, 
            AbstractLogMessageSummaryTx, 
            ILogMessageSummaryEntity> Report { get; set; }
    }
}
