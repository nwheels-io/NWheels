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

#if false

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using NWheels.Extensions;
using NWheels.UI.Core;
using NWheels.UI.Uidl;
using NWheels.Processing;

namespace NWheels.UI.Toolbox
{
    [DataContract(Namespace = UidlDocument.DataContractNamespace)]
    public class ChartReport<TContext, TCriteria, TChartScript> :
        WidgetBase<ChartReport<TContext, TCriteria, TChartScript>, Empty.Data, ChartReport<TContext, TCriteria, TChartScript>.IReportState>
        where TChartScript : ITransactionScript<TContext, TCriteria, ChartData>
        where TContext : class
        where TCriteria : class
    {
        public ChartReport(string idName, ControlledUidlNode parent)
            : base(idName, parent)
        {
            base.WidgetType = "ChartReport";
            base.TemplateName = "ChartReport";
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataMember]
        public Form<TCriteria> CriteriaForm { get; set; }
        [DataMember]
        public Chart SummaryChart { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public UidlCommand ShowReport { get; set; }
        public UidlCommand DownloadExcel { get; set; }
        public UidlNotification<TContext> ContextSetter { get; set; }
        public UidlNotification<ChartData> ChartReady { get; set; }
        public UidlNotification ResultsReady { get; set; }
        public UidlNotification<IPromiseFailureInfo> ResultsFailed { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void DescribePresenter(PresenterBuilder<ChartReport<TContext, TCriteria, TChartScript, TResultScript, TResultRow>, Empty.Data, IReportState> presenter)
        {
            CriteriaForm.UsePascalCase = true;
            CriteriaForm.Commands.Add(ShowReport);
            CriteriaForm.Commands.Add(DownloadExcel);
            ShowReport.Kind = CommandKind.Submit;
            DownloadExcel.Kind = CommandKind.Submit;
            SummaryChart.BindToModelSetter(this.ChartReady);

            var attribute = typeof(TChartScript).GetCustomAttribute<TransactionScriptAttribute>();

            if (attribute != null && attribute.SupportsInitializeInput)
            {
                presenter.On(ContextSetter)
                    .InvokeTransactionScript<TChartScript>()
                    .WaitForReply((script, vm) => script.InitializeInput(vm.Input))
                    .Then(b => b.AlterModel(alt => alt.Copy(m => m.Input).To(m => m.State.Criteria))
                    .Then(bb => bb.Broadcast(CriteriaForm.ModelSetter).WithPayload(m => m.Input).TunnelDown()));
            }

            presenter.On(ShowReport)
                .InvokeTransactionScript<TChartScript>().WaitForReply((script, vm) => script.Execute(vm.State.Criteria))
                .Then(
                    onSuccess: 
                        b => b.Broadcast(ChartReady).WithPayload(vm => vm.Input).TunnelDown()
                        .Then(bb => bb.UserAlertFrom<IReportUserAlerts>().ShowPopup((x, vm) => x.ReportIsReady())),
                    onFailure: 
                        b => b.UserAlertFrom<IReportUserAlerts>().ShowPopup((x, vm) => x.FailedToPrepareReport(), faultInfo: vm => vm.Input));

            presenter.On(DownloadExcel)
                .InvokeTransactionScript<TResultScript>(queryAsEntityType: typeof(TResultRow))
                .SetupEnityQueryFor(ResultTable)
                .WaitForResultsDownloadReady((script, vm) => script.Execute(vm.State.Criteria), exportFormat: "EXCEL")
                .Then(
                    onSuccess: b => b
                        .BeginDownloadContent(vm => vm.Input)
                        .Then(bb => bb.UserAlertFrom<IReportUserAlerts>().ShowPopup((x, vm) => x.ReportIsReady())
                        .Then(bbb => bbb.Broadcast(CriteriaForm.StateResetter).TunnelDown())),
                    onFailure: b => b
                        .UserAlertFrom<IReportUserAlerts>().ShowPopup((x, vm) => x.FailedToPrepareReport(), faultInfo: vm => vm.Input)
                        .Then(bb => bb.Broadcast(CriteriaForm.StateResetter).TunnelDown()));

            presenter.On(ResultsReady)
                .Broadcast(CriteriaForm.StateResetter).TunnelDown()
                .Then(b => b.UserAlertFrom<IReportUserAlerts>().ShowPopup((x, vm) => x.ReportIsReady()));

            presenter.On(ResultTable.QueryFailed)
                .Broadcast(CriteriaForm.StateResetter).TunnelDown()
                .Then(b => b.UserAlertFrom<IReportUserAlerts>().ShowPopup((x, vm) => x.FailedToPrepareReport(), faultInfo: vm => vm.Input));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of WidgetUidlNode

        public override IEnumerable<WidgetUidlNode> GetNestedWidgets()
        {
            return base.GetNestedWidgets().Concat(new WidgetUidlNode[] { CriteriaForm, SummaryChart, ResultTable });
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

#endif