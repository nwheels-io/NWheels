using System;
using System.Linq;
using System.Linq.Expressions;
using NWheels.Processing;
using NWheels.UI.Uidl;

namespace NWheels.UI.Toolbox
{
    public class ChartReportScreenPart<TInput, TCriteria, TChartScript, TRowScript, TRow> :
        ScreenPartBase<ChartReportScreenPart<TInput, TCriteria, TChartScript, TRowScript, TRow>, TInput, Empty.Data, Empty.State>
        where TChartScript : ITransactionScript<TInput, TCriteria, ChartData>
        where TRowScript : ITransactionScript<TInput, TCriteria, IQueryable<TRow>>
        where TInput : class
        where TCriteria : class
        where TRow : class
    {
        public ChartReportScreenPart(string idName, UidlApplication parent)
            : base(idName, parent)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void DescribePresenter(PresenterBuilder<ChartReportScreenPart<TInput, TCriteria, TChartScript, TRowScript, TRow>, Empty.Data, Empty.State> presenter)
        {
            ContentRoot = Report;

            presenter.On(base.NavigatedHere)
                .Broadcast(Report.ContextSetter).WithPayload(m => m.Input).TunnelDown();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ChartReport<TInput, TCriteria, TChartScript, TRowScript, TRow> Report { get; set; }
    }
}
