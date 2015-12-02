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
    public class TransactionForm<TContext, TInput, TScript, TOutput> :
        WidgetBase<TransactionForm<TContext, TInput, TScript, TOutput>, Empty.Data, TransactionForm<TContext, TInput, TScript, TOutput>.ITransactionFormState>
        where TScript : ITransactionScript<TContext, TInput, TOutput>
        where TContext : class
        where TInput : class
    {
        public TransactionForm(string idName, ControlledUidlNode parent)
            : base(idName, parent)
        {
            base.WidgetType = "TransactionForm";
            base.TemplateName = "TransactionForm";
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataMember]
        public Form<TInput> InputForm { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public UidlCommand Execute { get; set; }
        public UidlCommand Reset { get; set; }
        public UidlNotification<TContext> ContextSetter { get; set; }
        public UidlNotification<TOutput> OutputReady { get; set; }
        public UidlNotification<IPromiseFailureInfo> OperationFailed { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void DescribePresenter(PresenterBuilder<TransactionForm<TContext, TInput, TScript, TOutput>, Empty.Data, ITransactionFormState> presenter)
        {
            Icon = "edit";
            InputForm.UsePascalCase = true;
            InputForm.Commands.Add(Execute);
            InputForm.Commands.Add(Reset);
            Execute.Kind = CommandKind.Submit;
            Execute.Severity = CommandSeverity.Change;
            Reset.Kind = CommandKind.Reject;
            Reset.Severity = CommandSeverity.None;

            var attribute = typeof(TScript).GetCustomAttribute<TransactionScriptAttribute>();

            if ( attribute != null && attribute.SupportsInitializeInput )
            {
                presenter.On(Loaded)
                    .InvokeTransactionScript<TScript>()
                    .WaitForReply((script, data, state, context) => script.InitializeInput(null))
                    .Then(b => b.AlterModel(alt => alt.Copy(m => m.Input).To(m => m.State.Input))
                    .Then(bb => bb.Broadcast(InputForm.ModelSetter).WithPayload(m => m.Input).TunnelDown()));
            }

            presenter.On(Execute)
                .InvokeTransactionScript<TScript>(queryAsEntityType: typeof(TOutput))
                .WaitForReply((script, data, state, input) => script.Execute(state.Input))
                .Then(b => b.AlterModel(alt => alt.Copy(vm => vm.Input).To(vm => vm.State.Output))
                .Then(bb => bb.Broadcast(OutputReady).WithPayload(vm => vm.Input).BubbleUp()
                .Then(bbb => bbb.Broadcast(InputForm.StateResetter).TunnelDown()
                .Then(bbbb => bbbb.UserAlertFrom<ITransactionUserAlerts>().ShowPopup((alerts, vm) => alerts.SuccessfullyCompleted())))));

            presenter.On(Reset)
                .Broadcast(InputForm.StateResetter).TunnelDown();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of WidgetUidlNode

        public override IEnumerable<WidgetUidlNode> GetNestedWidgets()
        {
            return base.GetNestedWidgets().Concat(new WidgetUidlNode[] { InputForm });
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [ViewModelContract]
        public interface ITransactionFormState
        {
            TInput Input { get; set; }
            TOutput Output { get; set; }
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    [DataContract(Namespace = UidlDocument.DataContractNamespace)]
    public class TransactionForm<TInput, TScript, TOutput> : TransactionForm<Empty.Context, TInput, TScript, TOutput>
        where TScript : ITransactionScript<Empty.Context, TInput, TOutput>
        where TInput : class
    {
        public TransactionForm(string idName, ControlledUidlNode parent)
            : base(idName, parent)
        {
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    [DataContract(Namespace = UidlDocument.DataContractNamespace)]
    public class TransactionForm<TScript, TData> : TransactionForm<Empty.Context, TData, TScript, Empty.Output>
        where TScript : ITransactionScript<Empty.Context, TData, Empty.Output>
        where TData : class
    {
        public TransactionForm(string idName, ControlledUidlNode parent)
            : base(idName, parent)
        {
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface ITransactionUserAlerts : IUserAlertRepository
    {
        [SuccessAlert]
        UidlUserAlert SuccessfullyCompleted();

        [ErrorAlert]
        UidlUserAlert FailedToCompleteRequestedAction();
    }
}
