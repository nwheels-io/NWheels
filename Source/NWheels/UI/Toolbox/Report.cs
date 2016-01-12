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
    public class Report<TContext, TCriteria, TScript, TResultRow> :
        WidgetBase<Report<TContext, TCriteria, TScript, TResultRow>, Empty.Data, Report<TContext, TCriteria, TScript, TResultRow>.IReportState>
        where TScript : ITransactionScript<TContext, TCriteria, IQueryable<TResultRow>>
        where TContext : class
        where TCriteria : class
    {
        public Report(string idName, ControlledUidlNode parent)
            : base(idName, parent)
        {
            base.WidgetType = "Report";
            base.TemplateName = "Report";
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataMember]
        public Form<TCriteria> CriteriaForm { get; set; }
        [DataMember]
        public DataGrid<TResultRow> ResultTable { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public UidlCommand ShowReport { get; set; }
        public UidlCommand DownloadExcel { get; set; }
        public UidlNotification<TContext> ContextSetter { get; set; }
        public UidlNotification ReportReady { get; set; }
        public UidlNotification<IPromiseFailureInfo> ReportFailed { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void DescribePresenter(PresenterBuilder<Report<TContext, TCriteria, TScript, TResultRow>, Empty.Data, IReportState> presenter)
        {
            CriteriaForm.UsePascalCase = true;
            CriteriaForm.Commands.Add(ShowReport);
            CriteriaForm.Commands.Add(DownloadExcel);
            ResultTable.UsePascalCase = true;
            ResultTable.EnablePaging = true;
            ResultTable.EnableTotalRow = ResultTable.DisplayColumns.Any(c => c.IncludeInTotal);
            ResultTable.TotalRowOnTop = false;//TODO: fix on-top total row in Inspinia skin
            ShowReport.Kind = CommandKind.Submit;
            DownloadExcel.Kind = CommandKind.Submit;

            var attribute = typeof(TScript).GetCustomAttribute<TransactionScriptAttribute>();

            if ( attribute != null && attribute.SupportsInitializeInput )
            {
                presenter.On(ContextSetter)
                    .InvokeTransactionScript<TScript>()
                    .WaitForReply((script, vm) => script.InitializeInput(vm.Input))
                    .Then(b => b.AlterModel(alt => alt.Copy(m => m.Input).To(m => m.State.Criteria))
                    .Then(bb => bb.Broadcast(CriteriaForm.ModelSetter).WithPayload(m => m.Input).TunnelDown()));
            }

            presenter.On(ShowReport)
                .InvokeTransactionScript<TScript>(queryAsEntityType: typeof(TResultRow))
                .PrepareWaitForReply((script, vm) => script.Execute(vm.State.Criteria))
                .Then(b => b.Broadcast(ResultTable.RequestPrepared).WithPayload(vm => vm.Input).TunnelDown());

            presenter.On(DownloadExcel)
                .InvokeTransactionScript<TScript>(queryAsEntityType: typeof(TResultRow))
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

            presenter.On(ReportReady)
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

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IReportUserAlerts : IUserAlertRepository
    {
        [SuccessAlert]
        UidlUserAlert ReportIsReady();

        [SuccessAlert]
        UidlUserAlert ReportDownloadIsReady();

        [ErrorAlert]
        UidlUserAlert FailedToPrepareReport();
    }
}
