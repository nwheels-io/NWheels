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

            LogLevelSummary.SummaryChart.TemplateName = "ChartInline";
            LogLevelSummary.SummaryChart.Height = WidgetSize.Large;

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
