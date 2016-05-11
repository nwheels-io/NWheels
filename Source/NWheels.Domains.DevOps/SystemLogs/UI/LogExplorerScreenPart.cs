using NWheels.Domains.DevOps.SystemLogs.Entities;
using NWheels.Domains.DevOps.SystemLogs.Transactions;
using NWheels.UI;
using NWheels.UI.Toolbox;
using NWheels.UI.Uidl;

namespace NWheels.Domains.DevOps.SystemLogs.UI
{
    public class LogExplorerScreenPart : ScreenPartBase<LogExplorerScreenPart, Empty.Context, Empty.Data, Empty.State>
    {
        public LogExplorerScreenPart(string idName, UidlApplication parent)
            : base(idName, parent)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of ScreenPartBase<SystemLogScreenPart,Input,Data,State>

        protected override void DescribePresenter(PresenterBuilder<LogExplorerScreenPart, Empty.Data, Empty.State> presenter)
        {
            ContentRoot = LogLevelSummary;

            presenter.On(base.NavigatedHere)
                .Broadcast(LogLevelSummary.ContextSetter).WithPayload(vm => vm.Input).TunnelDown();
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ChartTableReport<
            Empty.Context, 
            ILogTimeRangeCriteria, 
            LogLevelSummaryChartTx, 
            LogLevelSummaryListTx, 
            ILogLevelSummaryEntity> LogLevelSummary { get; set; }
    }
}
