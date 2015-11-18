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
    public class ReportScreenPart<TInput, TCriteria, TScript, TResultRow> : 
        ScreenPartBase<ReportScreenPart<TInput, TCriteria, TScript, TResultRow>, TInput, Empty.Data, Empty.State>
        where TScript : ITransactionScript<TInput, TCriteria, IQueryable<TResultRow>>
        where TInput : class
        where TCriteria : class
        where TResultRow : class
    {
        public ReportScreenPart(string idName, UidlApplication parent)
            : base(idName, parent)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void ShowColumns(params Expression<Func<TResultRow, object>>[] propertySelectors)
        {
            foreach ( var property in propertySelectors )
            {
                Report.ResultTable.Column(property);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void DescribePresenter(PresenterBuilder<ReportScreenPart<TInput, TCriteria, TScript, TResultRow>, Empty.Data, Empty.State> presenter)
        {
            ContentRoot = Report;

            presenter.On(base.NavigatedHere)
                .Broadcast(Report.ContextSetter).WithPayload(m => m.Input).TunnelDown();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Report<TInput,TCriteria, TScript, TResultRow> Report { get; set; }
    }
}
