using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using NWheels.Extensions;
using NWheels.UI.Core;
using NWheels.UI.Uidl;
using NWheels.Processing;

namespace NWheels.UI.Toolbox
{
    [DataContract(Namespace = UidlDocument.DataContractNamespace)]
    public class Report<TCriteria, TScript, TResultRow> : WidgetBase<Report<TCriteria, TScript, TResultRow>, Empty.Data, Report<TCriteria, TScript, TResultRow>.IReportState>
        where TScript : ITransactionScript
    {
        private Expression<Func<TScript, Empty.Data, IReportState, Empty.Payload, TResultRow[]>> _onExecuteCall;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Report(string idName, ControlledUidlNode parent)
            : base(idName, parent)
        {
            base.WidgetType = "Report";
            base.TemplateName = "Report";
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void OnExecute(Expression<Func<TScript, Empty.Data, IReportState, Empty.Payload, TResultRow[]>> call)
        {
            _onExecuteCall = call;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataMember]
        public Form<TCriteria> CriteriaForm { get; set; }
        [DataMember]
        public DataGrid<TResultRow> ResultTable { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public UidlCommand ShowReport { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void DescribePresenter(PresenterBuilder<Report<TCriteria, TScript, TResultRow>, Empty.Data, IReportState> presenter)
        {
            CriteriaForm.Commands.Add(ShowReport);

            presenter.On(ShowReport)
                .InvokeTransactionScript<TScript>()
                .WaitForReply(_onExecuteCall)
                .Then(b => b.Broadcast(ResultTable.DataReceived).WithPayload(m => m.Input).TunnelDown());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of WidgetUidlNode

        public override IEnumerable<WidgetUidlNode> GetNestedWidgets()
        {
            return base.GetNestedWidgets().Concat(new WidgetUidlNode[] { CriteriaForm, ResultTable });
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [ViewModelContract]
        public interface IReportState
        {
            TCriteria Criteria { get; set; }
        }
    }
}
