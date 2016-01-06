using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using NWheels.DataObjects;
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
            this.UserAlertDisplayMode = UserAlertDisplayMode.Popup;

            this.InputMetaType = MetadataCache.GetTypeMetadata(typeof(TInput));

            var formOrTypeSelector = UidlUtility.CreateFormOrTypeSelector(InputMetaType, "InputForm", this, isInline: false);
            this.InputForm = formOrTypeSelector as Form<TInput>;
            this.InputFormTypeSelector = formOrTypeSelector as TypeSelector;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override IEnumerable<WidgetUidlNode> GetNestedWidgets()
        {
            return base.GetNestedWidgets().ConcatOneIf(InputForm).ConcatOneIf(InputFormTypeSelector);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataMember, ManuallyAssigned]
        public Form<TInput> InputForm { get; set; }
        [DataMember, ManuallyAssigned]
        public TypeSelector InputFormTypeSelector { get; set; }
        [DataMember]
        public UserAlertDisplayMode UserAlertDisplayMode { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public UidlCommand Execute { get; set; }
        public UidlCommand Reset { get; set; }
        public UidlNotification<TContext> ContextSetter { get; set; }
        public UidlNotification<TOutput> OutputReady { get; set; }
        public UidlNotification<IPromiseFailureInfo> OperationFailed { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected internal ITypeMetadata InputMetaType { get; private set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnBuild(UidlBuilder builder)
        {
            base.OnBuild(builder);
            builder.BuildManuallyInstantiatedNodes(InputForm, InputFormTypeSelector);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void DescribePresenter(PresenterBuilder<TransactionForm<TContext, TInput, TScript, TOutput>, Empty.Data, ITransactionFormState> presenter)
        {
            Icon = "edit";
            Execute.Kind = CommandKind.Submit;
            Execute.Severity = CommandSeverity.Change;
            Reset.Kind = CommandKind.Reject;
            Reset.Severity = CommandSeverity.None;

            if ( InputForm != null )
            {
                ConfigureInputForm(InputForm);
            }
            else
            {
                InputFormTypeSelector.ParentModelProperty = "Input";
                InputFormTypeSelector.ForEachWidgetOfType<IUidlForm>(ConfigureInputForm);
            }

            var attribute = typeof(TScript).GetCustomAttribute<TransactionScriptAttribute>();

            if ( attribute != null && attribute.SupportsInitializeInput )
            {
                presenter.On(Loaded)
                    .InvokeTransactionScript<TScript>()
                    .WaitForReply((script, vm) => script.InitializeInput(null))
                    .Then(b => b.AlterModel(alt => alt.Copy(m => m.Input).To(m => m.State.Input))
                    .Then(InvokeFormModelSetter));
            }
            else
            {
                presenter.On(Loaded)
                    .QueryModel(vm => vm.State.Input)
                    .Then(InvokeFormModelSetter);
            }

            presenter.On(Execute)
                .InvokeTransactionScript<TScript>()
                .WaitForReply((script, vm) => script.Execute(vm.State.Input))
                .Then(
                    onSuccess: b => b
                        .AlterModel(alt => alt.Copy(vm => vm.Input).To(vm => vm.State.Output))
                        .Then(bb => bb.Broadcast(OutputReady).WithPayload(vm => vm.Input).BubbleUp()
                        .Then(bbb => InvokeFormStateResetter(bbb)
                        .Then(bbbb => bbbb.UserAlertFrom<ITransactionUserAlerts>().Show<Empty.Payload>(UserAlertDisplayMode, (alerts, vm) => alerts.SuccessfullyCompleted())))),
                    onFailure: b => b
                        .Broadcast(OperationFailed).WithPayload(vm => vm.Input).BubbleUp()
                        .Then(bb => bb.UserAlertFrom<ITransactionUserAlerts>().Show<TOutput>(UserAlertDisplayMode, (alerts, vm) => alerts.FailedToCompleteRequestedAction(), faultInfo: vm => vm.Input)
                        .Then(bbb => InvokeFormStateResetter(bbb)))
                );

            if ( InputForm != null )
            {
                presenter.On(Reset).Broadcast(InputForm.StateResetter).TunnelDown();
            }
            else
            {
                presenter.On(Reset).Broadcast(InputFormTypeSelector.StateResetter).TunnelDown();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void ConfigureInputForm(IUidlForm form)
        {
            form.UsePascalCase = true;
            form.Commands.AddRange(this.Commands);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void InvokeFormModelSetter(
            PresenterBuilder<TransactionForm<TContext, TInput, TScript, TOutput>, Empty.Data, ITransactionFormState>.BehaviorBuilder<TInput> behaviorBuilder)
        {
            if ( InputForm != null )
            {
                behaviorBuilder.Broadcast(InputForm.ModelSetter).WithPayload(m => m.Input).TunnelDown();
            }
            else
            {
                behaviorBuilder.Broadcast(InputFormTypeSelector.ModelSetter).TunnelDown();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private PresenterBuilder<TransactionForm<TContext, TInput, TScript, TOutput>, Empty.Data, ITransactionFormState>.PromiseBuilder<TOutput> InvokeFormStateResetter(
            PresenterBuilder<TransactionForm<TContext, TInput, TScript, TOutput>, Empty.Data, ITransactionFormState>.BehaviorBuilder<TOutput> behaviorBuilder)
        {
            if ( InputForm != null )
            {
                return behaviorBuilder.Broadcast(InputForm.StateResetter).TunnelDown();
            }
            else
            {
                return behaviorBuilder.Broadcast(InputFormTypeSelector.StateResetter).TunnelDown();
            }
        }

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
