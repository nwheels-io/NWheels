using System;
using System.Linq;
using System.Linq.Expressions;
using NWheels.Processing;
using NWheels.UI.Uidl;

namespace NWheels.UI.Toolbox
{
    public class ChartReportScreenPart<TInput, TCriteria, TChartScript> :
        ScreenPartBase<ChartReportScreenPart<TInput, TCriteria, TChartScript>, TInput, Empty.Data, Empty.State>
        where TChartScript : ITransactionScript<TInput, TCriteria, ChartData>
        where TInput : class
        where TCriteria : class
    {
        public ChartReportScreenPart(string idName, UidlApplication parent)
            : base(idName, parent)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void DescribePresenter(PresenterBuilder<ChartReportScreenPart<TInput, TCriteria, TChartScript>, Empty.Data, Empty.State> presenter)
        {
            ContentRoot = Report;

            presenter.On(base.NavigatedHere)
                .Broadcast(Report.ContextSetter).WithPayload(m => m.Input).TunnelDown();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ChartReport<TInput, TCriteria, TChartScript> Report { get; set; }
    }
}
