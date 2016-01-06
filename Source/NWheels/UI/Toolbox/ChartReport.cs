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
    public class ChartReport<TCriteria, TScript> : WidgetBase<ChartReport<TCriteria, TScript>, Empty.Data, ChartReport<TCriteria, TScript>.IReportState>
        where TScript : ITransactionScript
        where TCriteria : class
    {
        private Expression<Func<TScript, ViewModel<Empty.Data, IReportState, Empty.Payload>, ChartData>> _onExecuteCall;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ChartReport(string idName, ControlledUidlNode parent)
            : base(idName, parent)
        {
            base.WidgetType = "ChartReport";
            base.TemplateName = "ChartReport";
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void OnExecute(Expression<Func<TScript, ViewModel<Empty.Data, IReportState, Empty.Payload>, ChartData>> call)
        {
            _onExecuteCall = call;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataMember]
        public Form<TCriteria> CriteriaForm { get; set; }
        [DataMember]
        public Chart ResultChart { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public UidlCommand ShowReport { get; set; }
        public UidlNotification<ChartData> DataReceived { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void DescribePresenter(PresenterBuilder<ChartReport<TCriteria, TScript>, Empty.Data, IReportState> presenter)
        {
            CriteriaForm.Commands.Add(ShowReport);
            ResultChart.BindToModelSetter(DataReceived, x => x);

            presenter.On(ShowReport)
                .InvokeTransactionScript<TScript>()
                .WaitForReply(_onExecuteCall)
                .Then(b => b.Broadcast(DataReceived).WithPayload(m => m.Input).TunnelDown());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of WidgetUidlNode

        public override IEnumerable<WidgetUidlNode> GetNestedWidgets()
        {
            return base.GetNestedWidgets().Concat(new WidgetUidlNode[] { CriteriaForm, ResultChart });
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
