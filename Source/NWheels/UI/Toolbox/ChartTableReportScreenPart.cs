using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NWheels.Processing;
using NWheels.UI.Uidl;

namespace NWheels.UI.Toolbox
{
    public class ChartTableReportScreenPart<TInput, TCriteria, TChartScript, TRowScript, TRow> : 
        ScreenPartBase<ChartTableReportScreenPart<TInput, TCriteria, TChartScript, TRowScript, TRow>, TInput, Empty.Data, Empty.State>
        where TChartScript : ITransactionScript<TInput, TCriteria, ChartData>
        where TRowScript : ITransactionScript<TInput, TCriteria, IQueryable<TRow>>
        where TInput : class
        where TCriteria : class
        where TRow : class
    {
        public ChartTableReportScreenPart(string idName, UidlApplication parent)
            : base(idName, parent)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void ShowColumns(params Expression<Func<TRow, object>>[] propertySelectors)
        {
            foreach ( var property in propertySelectors )
            {
                Report.ResultTable.Column<object>(property);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void ShowColumns<TDerivedResultRow>(params Expression<Func<TDerivedResultRow, object>>[] propertySelectors)
            where TDerivedResultRow : TRow
        {
            foreach ( var property in propertySelectors )
            {
                Report.ResultTable.Column<TDerivedResultRow, object>(property);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void DescribePresenter(PresenterBuilder<ChartTableReportScreenPart<TInput, TCriteria, TChartScript, TRowScript, TRow>, Empty.Data, Empty.State> presenter)
        {
            ContentRoot = Report;

            presenter.On(base.NavigatedHere)
                .Broadcast(Report.ContextSetter).WithPayload(m => m.Input).TunnelDown();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ChartTableReport<TInput,TCriteria, TChartScript, TRowScript, TRow> Report { get; set; }
    }
}
