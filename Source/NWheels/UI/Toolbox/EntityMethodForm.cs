using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using Hapil;
using NWheels.Extensions;
using NWheels.UI.Core;
using NWheels.UI.Uidl;
using NWheels.Processing;

namespace NWheels.UI.Toolbox
{
    [DataContract(Namespace = UidlDocument.DataContractNamespace)]
    public class EntityMethodForm<TContext, TEntity, TInput, TOutput> :
        WidgetBase<EntityMethodForm<TContext, TEntity, TInput, TOutput>, Empty.Data, EntityMethodForm<TContext, TEntity, TInput, TOutput>.IState>
        where TContext : class
        where TEntity : class
        where TInput : class
    {
        private LambdaExpression _methodCallExpression;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public EntityMethodForm(string idName, ControlledUidlNode parent)
            : base(idName, parent)
        {
            base.WidgetType = "EntityMethodForm";
            base.TemplateName = "EntityMethodForm";
            base.IsPopupContent = true;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataMember]
        public Form<TInput> InputForm { get; set; }
        [DataMember]
        public bool QueryAsEntity { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public UidlNotification ShowModal { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public UidlCommand OK { get; set; }
        public UidlCommand Cancel { get; set; }
        public UidlNotification<TContext> ContextSetter { get; set; }
        public UidlNotification OperationStarting { get; set; }
        public UidlNotification<TOutput> OperationCompleted { get; set; }
        public UidlNotification<IPromiseFailureInfo> OperationFailed { get; set; }
        public UidlNotification<TEntity> EntitySetter { get; set; }
        public UidlNotification NoEntityWasSelected { get; set; }
        
        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void AttachTo<TController, TControllerData, TControllerState>(
            PresenterBuilder<TController, TControllerData, TControllerState> controller, 
            UidlCommand command,
            Expression<Action<TEntity, ViewModel<Empty.Data, IState, Empty.Payload>>> onExecute)
            where TController : ControlledUidlNode
            where TControllerData : class 
            where TControllerState : class
        {
            this.Text = command.Text;
            this.Icon = command.Icon;
            _methodCallExpression = onExecute;
            command.Authorization.OperationName = _methodCallExpression.GetMethodInfo().Name;
            controller.On(command).Broadcast(this.ShowModal).TunnelDown();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void AttachTo<TController, TControllerData, TControllerState, TMethodOut>(
            PresenterBuilder<TController, TControllerData, TControllerState> controller,
            UidlCommand command,
            Expression<Func<TEntity, ViewModel<Empty.Data, IState, Empty.Payload>, TMethodOut>> onExecute)
            where TController : ControlledUidlNode
            where TControllerData : class
            where TControllerState : class
        {
            this.Text = command.Text;
            this.Icon = command.Icon;
            _methodCallExpression = onExecute;
            command.Authorization.OperationName = _methodCallExpression.GetMethodInfo().Name;
            controller.On(command).Broadcast(this.ShowModal).TunnelDown();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void OnExecute(Expression<Func<TEntity, ViewModel<Empty.Data, IState, Empty.Payload>, TOutput>> callExpression)
        {
            _methodCallExpression = callExpression;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void OnExecute(Expression<Action<TEntity, ViewModel<Empty.Data, IState, Empty.Payload>>> callExpression)
        {
            _methodCallExpression = callExpression;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void DescribePresenter(PresenterBuilder<EntityMethodForm<TContext, TEntity, TInput, TOutput>, Empty.Data, IState> presenter)
        {
            InputForm.UsePascalCase = true;
            InputForm.IsModalPopup = true;

            OK.Kind = CommandKind.Submit;
            OK.Severity = CommandSeverity.Change;
            OK.Icon = "check";
            Cancel.Kind = CommandKind.Reject;
            Cancel.Severity = CommandSeverity.Loose;
            Cancel.Icon = "times";

            presenter.On(ContextSetter).AlterModel(alt => alt.Copy(vm => vm.Input).To(vm => vm.State.Context));
            presenter.On(EntitySetter).AlterModel(alt => alt.Copy(vm => vm.Input).To(vm => vm.State.Entity));
            presenter.On(OK)
                .Broadcast(OperationStarting).BubbleUp()
                .Then(b => b.InvokeEntityMethod<TEntity>(TryGetQueryAsEntityType()).WaitForReplyOrCompletion<TOutput>(_methodCallExpression)
                .Then(
                    onSuccess: 
                        bb => bb.Broadcast(OperationCompleted).WithPayload(vm => vm.Input).BubbleUp()
                        .Then(bbb => bbb.UserAlertFrom<IEntityMethodUserAlerts>().ShowPopup((alerts, vm) => alerts.RequestedOperationSuccessfullyCompleted())),
                    onFailure:
                        bb => bb.Broadcast(OperationFailed).WithPayload(vm => vm.Input).BubbleUp()
                        .Then(bbb => bbb.UserAlertFrom<IEntityMethodUserAlerts>().ShowPopup((alerts, vm) => alerts.RequestedOperationHasFailed(), faultInfo: vm => vm.Input))
                ));

            presenter.On(NoEntityWasSelected).UserAlertFrom<IEntityMethodUserAlerts>().ShowPopup((alerts, vm) => alerts.NoEntityWasSelected());
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
        public interface IState
        {
            TContext Context { get; set; }
            TEntity Entity { get; set; }
            TInput Input { get; set; }
            TOutput Output { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private Type TryGetQueryAsEntityType()
        {
            if ( QueryAsEntity )
            {
                Type collectionElementType;

                if ( typeof(TOutput).IsCollectionType(out collectionElementType) )
                {
                    return collectionElementType;
                }
                else
                {
                    return typeof(TOutput);
                }
            }

            return null;
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    [DataContract(Namespace = UidlDocument.DataContractNamespace)]
    public class EntityMethodForm<TEntity, TInput, TOutput> :
        EntityMethodForm<Empty.Context, TEntity, TInput, TOutput>
        where TEntity : class
        where TInput : class
    {
        public EntityMethodForm(string idName, ControlledUidlNode parent)
            : base(idName, parent)
        {
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    [DataContract(Namespace = UidlDocument.DataContractNamespace)]
    public class EntityMethodForm<TEntity, TInput> :
        EntityMethodForm<Empty.Context, TEntity, TInput, Empty.Output>
        where TEntity : class
        where TInput : class
    {
        public EntityMethodForm(string idName, ControlledUidlNode parent)
            : base(idName, parent)
        {
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IEntityMethodUserAlerts : IUserAlertRepository
    {
        [SuccessAlert]
        UidlUserAlert RequestedOperationSuccessfullyCompleted();
        
        [ErrorAlert]
        UidlUserAlert RequestedOperationHasFailed();

        [InfoAlert()]
        UidlUserAlert NoEntityWasSelected();
    }
}
