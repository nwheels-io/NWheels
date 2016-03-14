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
    public class ChartTableReport<TContext, TCriteria, TChartScript, TResultScript, TResultRow> :
        WidgetBase<ChartTableReport<TContext, TCriteria, TChartScript, TResultScript, TResultRow>, Empty.Data, ChartTableReport<TContext, TCriteria, TChartScript, TResultScript, TResultRow>.IReportState>
        where TChartScript : ITransactionScript<TContext, TCriteria, ChartData>
        where TResultScript : ITransactionScript<TContext, TCriteria, IQueryable<TResultRow>>
        where TContext : class
        where TCriteria : class
    {
        public ChartTableReport(string idName, ControlledUidlNode parent)
            : base(idName, parent)
        {
            base.WidgetType = "ChartTableReport";
            base.TemplateName = "ChartTableReport";
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataMember]
        public Form<TCriteria> CriteriaForm { get; set; }
        [DataMember]
        public Chart SummaryChart { get; set; }
        [DataMember]
        public DataGrid<TResultRow> ResultTable { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public UidlCommand ShowReport { get; set; }
        public UidlCommand DownloadExcel { get; set; }
        public UidlNotification<TContext> ContextSetter { get; set; }
        public UidlNotification<ChartData> ChartReady { get; set; }
        public UidlNotification ResultsReady { get; set; }
        public UidlNotification<IPromiseFailureInfo> ResultsFailed { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void DescribePresenter(PresenterBuilder<ChartTableReport<TContext, TCriteria, TChartScript, TResultScript, TResultRow>, Empty.Data, IReportState> presenter)
        {
            CriteriaForm.UsePascalCase = true;
            CriteriaForm.Commands.Add(ShowReport);
            CriteriaForm.Commands.Add(DownloadExcel);
            ResultTable.Mode = DataGridMode.Standalone;
            ResultTable.UsePascalCase = true;
            ResultTable.EnablePaging = true;
            ResultTable.EnableTotalRow = ResultTable.DisplayColumns.Any(c => c.IncludeInTotal);
            ResultTable.TotalRowOnTop = false;//TODO: fix on-top total row in Inspinia skin
            ShowReport.Kind = CommandKind.Submit;
            DownloadExcel.Kind = CommandKind.Submit;
            SummaryChart.BindToModelSetter(this.ChartReady);

            var attribute = typeof(TResultScript).GetCustomAttribute<TransactionScriptAttribute>();

            if ( attribute != null && attribute.SupportsInitializeInput )
            {
                presenter.On(ContextSetter)
                    .InvokeTransactionScript<TResultScript>()
                    .WaitForReply((script, vm) => script.InitializeInput(vm.Input))
                    .Then(b => b.AlterModel(alt => alt.Copy(m => m.Input).To(m => m.State.Criteria))
                    .Then(bb => bb.Broadcast(CriteriaForm.ModelSetter).WithPayload(m => m.Input).TunnelDown()));
            }

            presenter.On(ShowReport)
                .InvokeTransactionScript<TResultScript>(queryAsEntityType: typeof(TResultRow))
                .PrepareWaitForReply((script, vm) => script.Execute(vm.State.Criteria))
                .Then(b => b.Broadcast(ResultTable.RequestPrepared).WithPayload(vm => vm.Input).TunnelDown()
                .Then(bb => bb.InvokeTransactionScript<TChartScript>().WaitForReply((script, vm) => script.Execute(vm.State.Criteria))
                .Then(
                    onSuccess: bbb => bbb.Broadcast(ChartReady).WithPayload(vm => vm.Input).TunnelDown()
                        .Then(bbbb => bbbb.Broadcast(CriteriaForm.StateResetter).TunnelDown()),
                    onFailure: bbb => bbb.UserAlertFrom<IReportUserAlerts>().ShowPopup((x, vm) => x.FailedToPrepareReport(), faultInfo: vm => vm.Input)
                        .Then(bbbb => bbbb.Broadcast(CriteriaForm.StateResetter).TunnelDown())
                )));

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
