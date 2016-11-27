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
        where TOutput : class
    {
        private UidlBuilder _builder;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

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

        public void UseOutputForm()
        {
            this.OutputForm = new Form<TOutput>("Output", this);
            this.OutputForm.Commands.Clear();

            if (_builder != null)
            {
                _builder.BuildManuallyInstantiatedNodes(OutputForm);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void AttachAsPopupTo<TController, TControllerData, TControllerState>(
            PresenterBuilder<TController, TControllerData, TControllerState> controller, 
            UidlCommand command)
            where TController : ControlledUidlNode
            where TControllerData : class
            where TControllerState : class
        {
            SetPopupMode();

            this.Text = command.Text;
            this.Icon = command.Icon;

            controller.On(command).Broadcast(this.ShowModal).TunnelDown();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void AttachAsPopupTo<TController, TControllerData, TControllerState>(
            PresenterBuilder<TController, TControllerData, TControllerState> controller,
            UidlNotification notification)
            where TController : ControlledUidlNode
            where TControllerData : class
            where TControllerState : class
        {
            SetPopupMode();
            controller.On(notification).Broadcast(this.ShowModal).TunnelDown();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void SetPopupMode()
        {
            this.TemplateName = "TransactionFormPopupMode";
            this.InputForm.Commands.Clear();
            this.InputForm.IsModalPopup = true;
            this.IsPopupContent = true;

            if (this.OutputForm != null)
            {
                this.OutputForm.IsModalPopup = true;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void UseInlineStyle()
        {
            this.TemplateName = "TransactionFormInline";
            this.InputForm.IsInlineStyle = true;
            this.InputForm.TemplateName = "FormInline";
            this.InputForm.AutoSubmitOnChange = true;

            if (this.OutputForm != null)
            {
                this.OutputForm.IsInlineStyle = true;
                this.OutputForm.TemplateName = "FormInline";
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void UseOKCancelNotation(bool withIcons = false)
        {
            this.Execute.Text = "OK";
            this.Reset.Text = "Cancel";

            if (withIcons)
            {
                this.Execute.Icon = "check";
                this.Reset.Icon = "times";
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override IEnumerable<WidgetUidlNode> GetNestedWidgets()
        {
            return base.GetNestedWidgets().ConcatOneIf(InputForm).ConcatOneIf(InputFormTypeSelector).ConcatOneIf(OutputForm);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataMember, ManuallyAssigned]
        public Form<TInput> InputForm { get; set; }
        [DataMember, ManuallyAssigned]
        public TypeSelector InputFormTypeSelector { get; set; }
        [DataMember]
        public UserAlertDisplayMode UserAlertDisplayMode { get; set; }
        [DataMember, ManuallyAssigned]
        public Form<TOutput> OutputForm { get; set; }
        [DataMember]
        public bool IsNextDialog { get; set; }
        [DataMember]
        public WidgetSize Size { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public UidlNotification ShowModal { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public UidlCommand Execute { get; set; }
        public UidlCommand Reset { get; set; }
        public UidlNotification<TContext> ContextSetter { get; set; }
        public UidlNotification<TOutput> OutputReady { get; set; }
        public UidlNotification<IPromiseFailureInfo> OperationFailed { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Type ContextEntityType { get; set; }
        public string OutputDownloadFormat { get; set; }
        public bool RefreshUponCompletion { get; set; } 

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected internal ITypeMetadata InputMetaType { get; private set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnBuild(UidlBuilder builder)
        {
            _builder = builder;

            base.OnBuild(builder);
            builder.BuildManuallyInstantiatedNodes(InputForm, InputFormTypeSelector, OutputForm);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void DescribePresenter(PresenterBuilder<TransactionForm<TContext, TInput, TScript, TOutput>, Empty.Data, ITransactionFormState> presenter)
        {
            if (string.IsNullOrWhiteSpace(this.Icon))
            {
                Icon = "edit";
            }

            Execute.Kind = CommandKind.Submit;
            Execute.Severity = CommandSeverity.Change;
            Reset.Kind = CommandKind.Reject;
            Reset.Severity = CommandSeverity.None;

            var txAttribute = typeof(TScript).GetCustomAttribute<TransactionScriptAttribute>();
            var shouldInvokeInitializeInput = (txAttribute != null && txAttribute.SupportsInitializeInput);
            var hasCustomContext = (typeof(TContext) != typeof(Empty.Context));

            if (InputForm != null)
            {
                ConfigureInputForm(InputForm, shouldInvokeInitializeInput);
            }
            else
            {
                InputFormTypeSelector.ParentModelProperty = "Input";
                InputFormTypeSelector.ForEachWidgetOfType<IUidlForm>(form => ConfigureInputForm(form, shouldInvokeInitializeInput));
            }

            if (hasCustomContext)
            {
                presenter.On(ContextSetter)
                    .AlterModel(alt => alt.Copy(vm => vm.Input).To(vm => vm.State.Context));
            }

            if (IsPopupContent)
            {
                if (shouldInvokeInitializeInput)
                {
                    presenter.On(ShowModal)
                        .InvokeTransactionScript<TScript>(ContextEntityType, ApiCallResultType.Command)
                        .WaitForReply((script, vm) => script.InitializeInput(vm.State.Context))
                        .Then(b => b.AlterModel(alt => alt.Copy(m => m.Input).To(m => m.State.Input))
                        .Then(InvokeFormModelSetter));
                }
                else
                {
                    presenter.On(ShowModal)
                        .QueryModel(vm => vm.State.Input)
                        .Then(InvokeFormModelSetter);
                }
            }
            else 
            {
                if (shouldInvokeInitializeInput)
                {
                    presenter.On(Loaded)
                        .InvokeTransactionScript<TScript>(ContextEntityType, ApiCallResultType.Command)
                        .WaitForReply((script, vm) => script.InitializeInput(vm.State.Context))
                        .Then(b => b.AlterModel(alt => alt.Copy(m => m.Input).To(m => m.State.Input)).Then(InvokeFormModelSetter));
                }
                else
                {
                    presenter.On(Loaded)
                        .QueryModel(vm => vm.State.Input)
                        .Then(InvokeFormModelSetter);
                }
            }

            if (string.IsNullOrWhiteSpace(OutputDownloadFormat))
            { 
                presenter.On(Execute)
                    .InvokeTransactionScript<TScript>(ContextEntityType, ApiCallResultType.Command)
                    .WaitForReply((script, vm) => script.Execute(vm.State.Input))
                    .Then(
                        onSuccess: b => b
                            .AlterModel(alt => alt.Copy(vm => vm.Input).To(vm => vm.State.Output))
                            .Then(bb => bb.Broadcast(OutputReady).WithPayload(vm => vm.Input).BubbleUp()
                            .Then(bbb => InvokeFormStateResetter(bbb)
                            .ThenIf(this.OutputForm != null, b2 => b2.Broadcast(OutputForm.ModelSetter).WithPayload(vm => vm.Input).TunnelDown()
                            .Then(bbbb => bbbb.UserAlertFrom<ITransactionUserAlerts>().Show<Empty.Payload>(UserAlertDisplayMode, (alerts, vm) => alerts.SuccessfullyCompleted())
                            .ThenIf(this.RefreshUponCompletion, bbbbb => bbbbb
                                .InvokeTransactionScript<TScript>(ContextEntityType, ApiCallResultType.Command)
                                .WaitForReply((script, vm) => script.InitializeInput(null))
                                .Then(b6 => b6.AlterModel(alt => alt.Copy(m => m.Input).To(m => m.State.Input))
                                .Then(InvokeFormModelSetter))))))),
                        onFailure: b => b
                            .Broadcast(OperationFailed).WithPayload(vm => vm.Input).BubbleUp()
                            .Then(bb => bb.UserAlertFrom<ITransactionUserAlerts>().Show<TOutput>(UserAlertDisplayMode, (alerts, vm) => alerts.FailedToCompleteRequestedAction(), faultInfo: vm => vm.Input)
                            .Then(bbb => InvokeFormStateResetter(bbb)))
                    );
            }
            else
            {
                presenter.On(Execute)
                    .InvokeTransactionScript<TScript>(ContextEntityType, ApiCallResultType.Command)
                    .WaitForResultsDownloadReady((script, vm) => script.Execute(vm.State.Input), exportFormat: "EXCEL")
                    .Then(
                        onSuccess: b => b
                            .BeginDownloadContent(vm => vm.Input)
                            .Then(bb => bb.Broadcast(InputForm.StateResetter).TunnelDown()
                            .Then(bbb => bbb.UserAlertFrom<ITransactionUserAlerts>().Show<Empty.Payload>(UserAlertDisplayMode, (alerts, vm) => alerts.SuccessfullyCompleted())
                            .ThenIf(this.RefreshUponCompletion, bbbbb => bbbbb
                                .InvokeTransactionScript<TScript>(ContextEntityType, ApiCallResultType.Command)
                                .WaitForReply((script, vm) => script.InitializeInput(null))
                                .Then(b6 => b6.AlterModel(alt => alt.Copy(m => m.Input).To(m => m.State.Input))),
                        onFailure: bf => bf
                            .Broadcast(OperationFailed).WithPayload(vm => vm.Input).BubbleUp()
                            .Then(bbf => bbf.UserAlertFrom<ITransactionUserAlerts>().Show<TOutput>(UserAlertDisplayMode, (alerts, vm) => alerts.FailedToCompleteRequestedAction(), faultInfo: vm => vm.Input)
                            .Then(bbbf => InvokeFormStateResetter(bbbf)))))));
            }

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

        private void ConfigureInputForm(IUidlForm form, bool needsInitialModel)
        {
            form.UsePascalCase = true;
            form.NeedsInitialModel = needsInitialModel;

            if (!form.IsModalPopup)
            {
                form.Commands.AddRange(this.Commands);
            }
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
            TContext Context { get; set; }
            TInput Input { get; set; }
            TOutput Output { get; set; }
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    [DataContract(Namespace = UidlDocument.DataContractNamespace)]
    public class TransactionForm<TInput, TScript, TOutput> : TransactionForm<Empty.Context, TInput, TScript, TOutput>
        where TScript : ITransactionScript<Empty.Context, TInput, TOutput>
        where TInput : class
        where TOutput : class
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
