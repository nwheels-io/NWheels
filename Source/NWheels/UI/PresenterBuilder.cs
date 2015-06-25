using NWheels.UI.Uidl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using NWheels.Extensions;
using NWheels.UI.Toolbox;
using ApiCallType = NWheels.UI.Uidl.ApiCallType;
using BroadcastDirection = NWheels.UI.Uidl.BroadcastDirection;

namespace NWheels.UI
{
    public class PresenterBuilder<TContents, TData, TState>
        where TContents : ControlledUidlNode
        where TData : class
        where TState : class
    {
        private readonly UidlBuilder _uidl;
        private readonly ControlledUidlNode _ownerNode;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public PresenterBuilder(UidlBuilder uidl, ControlledUidlNode ownerNode)
        {
            _uidl = uidl;
            _ownerNode = ownerNode;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public BindingSourceBuilder<TValue> Bind<TValue>(Expression<Func<TContents, TValue>> destination)
        {
            return null;
        }

        ////-----------------------------------------------------------------------------------------------------------------------------------------------------

        //public BehaviorBuilder<TValue> OnChange<TValue>(Expression<Func<TContents, TValue>> property)
        //{
        //    return new BehaviorBuilder<TValue>(_ownerNode, );
        //}

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public BehaviorBuilder<TPayload> On<TPayload>(UidlNotification<TPayload> notification) 
            where TPayload : class
        {
            return new BehaviorBuilder<TPayload>(_ownerNode, notification, _uidl);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public BehaviorBuilder<Empty.Payload> On(UidlNotification notification)
        {
            return new BehaviorBuilder<Empty.Payload>(_ownerNode, notification, _uidl);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public BehaviorBuilder<Empty.Payload> On(UidlCommand command)
        {
            return new BehaviorBuilder<Empty.Payload>(_ownerNode, command.Executing, _uidl);
        }

        ////-----------------------------------------------------------------------------------------------------------------------------------------------------

        //TElement New<TElement>() where TElement : IUIElement;

        ////-----------------------------------------------------------------------------------------------------------------------------------------------------

        //BehaviorDescription Behavior<TInput>(Action<IBehaviorBuilder<TInput, TData, TState>> describe);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class BindingSourceBuilder<TValue>
        {
            //internal BindingSourceBuilder(ControlledUidlNode targetNode, )

            public void ToData(Expression<Func<TData, TValue>> dataProperty) { }
            public void ToState(Expression<Func<TState, TValue>> stateProperty) { }

            public void ToApi<TApiContract>(Expression<Func<TApiContract, TValue>> apiCall) { }
            public void ToApi<TApiContract, TReply>(
                Expression<Func<TApiContract, TReply>> apiCall,
                Expression<Func<TReply, TValue>> valueSelector) { }

            public void ToEntity<TEntity>(
                Action<IQueryable<TEntity>> query,
                Expression<Func<TEntity[], TValue>> valueSelector)
                where TEntity : class { }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class BehaviorBuilder<TInput>
        {
            private readonly ControlledUidlNode _ownerNode;
            private readonly UidlNotification _notification;
            private BehaviorUidlNode _behavior;
            private readonly UidlBuilder _uidl;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            internal BehaviorBuilder(ControlledUidlNode ownerNode, UidlNotification notification, UidlBuilder uidl)
            {
                _ownerNode = ownerNode;
                _notification = notification;
                _uidl = uidl;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public PromiseBuilder<TInput> InvokeCommand(UidlCommand command)
            {
                var behavior = new UidlInvokeCommandBehavior(_ownerNode.GetUniqueBehaviorId(), _ownerNode) {
                    CommandQualifiedName = command.QualifiedName
                };
                
                SetAndSubscribeBehavior(behavior);
                return new PromiseBuilder<TInput>(_ownerNode, _behavior, _uidl);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public CallApiBehaviorBuilder<TInput, TContract> CallApi<TContract>()
            {
                var behavior = new UidlCallApiBehavior(_ownerNode.GetUniqueBehaviorId(), _ownerNode);
                SetAndSubscribeBehavior(behavior);
                return new CallApiBehaviorBuilder<TInput, TContract>(_ownerNode, behavior, _uidl);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------
            
            public NavigateBehaviorBuilder<TInput> Navigate()
            {
                var behavior = new UidlNavigateBehavior(_ownerNode.GetUniqueBehaviorId(), _ownerNode);
                SetAndSubscribeBehavior(behavior);
                return new NavigateBehaviorBuilder<TInput>(_ownerNode, behavior, _uidl);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------
            
            public BroadcastBehaviorBuilder4<TInput> Broadcast(UidlNotification notification)
            {
                var behavior = new UidlBroadcastBehavior(_ownerNode.GetUniqueBehaviorId(), _ownerNode);
                SetAndSubscribeBehavior(behavior);
                behavior.NotificationQualifiedName = notification.QualifiedName;
                return new BroadcastBehaviorBuilder4<TInput>(_ownerNode, behavior, _uidl);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public BroadcastBehaviorBuilder3<TInput, TPayload> Broadcast<TPayload>(UidlNotification<TPayload> notification)
            {
                var behavior = new UidlBroadcastBehavior(_ownerNode.GetUniqueBehaviorId(), _ownerNode);
                SetAndSubscribeBehavior(behavior);
                behavior.NotificationQualifiedName = notification.QualifiedName;
                return new BroadcastBehaviorBuilder3<TInput, TPayload>(_ownerNode, behavior, _uidl);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ShowAlertBehaviorBuilder2<TInput, TRepo> UserAlertFrom<TRepo>() where TRepo : IUserAlertRepository
            {
                var behavior = new UidlAlertUserBehavior(_ownerNode.GetUniqueBehaviorId(), _ownerNode);
                SetAndSubscribeBehavior(behavior);
                return new ShowAlertBehaviorBuilder2<TInput, TRepo>(_ownerNode, behavior, _uidl);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public AlterModelBehaviorBuilder<TInput> AlterModel()
            {
                var behavior = new UidlAlterModelBehavior(_ownerNode.GetUniqueBehaviorId(), _ownerNode);
                SetAndSubscribeBehavior(behavior);
                return new AlterModelBehaviorBuilder<TInput>(_ownerNode, behavior, _uidl);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public WhenOtherwiseBehaviorBuilder<TInput> When(Expression<Func<TData, TState, TInput, bool>> condition, Action<BehaviorBuilder<TInput>> onTrue)
            {
                var behavior = new UidlBranchByRuleBehavior(_ownerNode.GetUniqueBehaviorId(), _ownerNode);
                SetAndSubscribeBehavior(behavior);
                return new WhenOtherwiseBehaviorBuilder<TInput>(_ownerNode, behavior, _uidl);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            internal BehaviorUidlNode Behavior
            {
                get { return _behavior; }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void SetAndSubscribeBehavior(BehaviorUidlNode behavior)
            {
                _behavior = behavior;

                if ( _notification != null )
                {
                    _notification.SubscribedBehaviorQualifiedNames.Add(behavior.QualifiedName);

                    behavior.Subscription = new BehaviorUidlNode.SubscriptionToNotification() {
                        NotificationQualifiedName = _notification.QualifiedName
                    };

                    _ownerNode.Behaviors.Add(behavior);
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class CallApiBehaviorBuilder<TInput, TContract>
        {
            private readonly ControlledUidlNode _ownerNode;
            private readonly UidlCallApiBehavior _behavior;
            private readonly UidlBuilder _uidl;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public CallApiBehaviorBuilder(ControlledUidlNode ownerNode, UidlCallApiBehavior behavior, UidlBuilder uidl)
            {
                _ownerNode = ownerNode;
                _behavior = behavior;
                _uidl = uidl;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public PromiseBuilder<TInput> OneWay(Expression<Action<TContract, TData, TState, TInput>> call)
            {
                ParseApiCall(call);
                _behavior.CallType = ApiCallType.OneWay;
                return new PromiseBuilder<TInput>(_ownerNode, _behavior, _uidl);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public PromiseBuilder<TInput> RequestReply(Expression<Action<TContract, TData, TState, TInput>> call)
            {
                ParseApiCall(call);
                _behavior.CallType = ApiCallType.RequestReply;
                return new PromiseBuilder<TInput>(_ownerNode, _behavior, _uidl);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public PromiseBuilder<TReply> RequestReply<TReply>(Expression<Func<TContract, TData, TState, TInput, TReply>> call)
            {
                ParseApiCall(call);
                _behavior.CallType = ApiCallType.RequestReply;
                return new PromiseBuilder<TReply>(_ownerNode, _behavior, _uidl);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void ParseApiCall(LambdaExpression apiCall)
            {
                Expression[] callArguments;
                var method = apiCall.GetMethodInfo(out callArguments);

                _behavior.ContractName = method.DeclaringType.Name;
                _behavior.OperationName = method.Name;
                _behavior.ParameterNames = method.GetParameters().Select(p => p.Name).ToArray();
                _behavior.ParameterExpressions = callArguments.Select(x => x.ToString()).ToArray();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class NavigateBehaviorBuilder<TInput>
        {
            private readonly ControlledUidlNode _ownerNode;
            private readonly UidlNavigateBehavior _behavior;
            private readonly UidlBuilder _uidl;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public NavigateBehaviorBuilder(ControlledUidlNode ownerNode, UidlNavigateBehavior behavior, UidlBuilder uidl)
            {
                _ownerNode = ownerNode;
                _behavior = behavior;
                _uidl = uidl;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public PromiseBuilder<TInput> ToScreen(IScreenWithInput<Empty.Input> screen)
            {
                _behavior.TargetType = UidlNodeType.Screen;
                _behavior.TargetQualifiedName = ((UidlScreen)screen).QualifiedName;
                _behavior.NavigationType = NavigationType.LoadInline;

                return new PromiseBuilder<TInput>(_ownerNode, _behavior, _uidl);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public NavigateBehaviorInputBuilder<TInput, TScreenInput> ToScreen<TScreenInput>(IScreenWithInput<TScreenInput> screen)
            {
                _behavior.TargetType = UidlNodeType.Screen;
                _behavior.TargetQualifiedName = ((UidlScreen)screen).QualifiedName;
                _behavior.NavigationType = NavigationType.LoadInline;

                return new NavigateBehaviorInputBuilder<TInput, TScreenInput>(_ownerNode, _behavior, _uidl);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public NavigateBehaviorScreenPartSelector<TInput> FromContainer(ScreenPartContainer container)
            {
                _behavior.TargetType = UidlNodeType.ScreenPart;
                _behavior.NavigationType = NavigationType.LoadInline;
                _behavior.TargetContainerQualifiedName = container.QualifiedName;

                return new NavigateBehaviorScreenPartSelector<TInput>(_ownerNode, _behavior, _uidl);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public NavigateBehaviorModalScreenPartSelector<TInput> ShowModalDialog()
            {
                _behavior.TargetType = UidlNodeType.ScreenPart;
                _behavior.NavigationType = NavigationType.PopupModal;

                return new NavigateBehaviorModalScreenPartSelector<TInput>(_ownerNode, _behavior, _uidl);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class NavigateBehaviorScreenPartSelector<TInput>
        {
            private readonly ControlledUidlNode _ownerNode;
            private readonly UidlNavigateBehavior _behavior;
            private readonly UidlBuilder _uidl;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public NavigateBehaviorScreenPartSelector(ControlledUidlNode ownerNode, UidlNavigateBehavior behavior, UidlBuilder uidl)
            {
                _ownerNode = ownerNode;
                _behavior = behavior;
                _uidl = uidl;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public PromiseBuilder<TInput> ToScreenPart(UidlScreenPart screenPart)
            {
                _behavior.TargetType = UidlNodeType.ScreenPart;
                _behavior.TargetQualifiedName = screenPart.QualifiedName;
                return new PromiseBuilder<TInput>(_ownerNode, _behavior, _uidl);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public NavigateBehaviorInputBuilder<TInput, TPartInput> ToScreenPart<TPartInput>(IScreenPartWithInput<TPartInput> screenPart)
            {
                _behavior.TargetType = UidlNodeType.ScreenPart;
                _behavior.TargetQualifiedName = ((UidlScreenPart)screenPart).QualifiedName;
                return new NavigateBehaviorInputBuilder<TInput, TPartInput>(_ownerNode, _behavior, _uidl);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class NavigateBehaviorModalScreenPartSelector<TInput>
        {
            private readonly ControlledUidlNode _ownerNode;
            private readonly UidlNavigateBehavior _behavior;
            private readonly UidlBuilder _uidl;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public NavigateBehaviorModalScreenPartSelector(ControlledUidlNode ownerNode, UidlNavigateBehavior behavior, UidlBuilder uidl)
            {
                _ownerNode = ownerNode;
                _behavior = behavior;
                _uidl = uidl;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public PromiseBuilder<ModalResult> WithScreenPart(UidlScreenPart screenPart)
            {
                _behavior.NavigationType = NavigationType.PopupModal;
                _behavior.TargetQualifiedName = screenPart.QualifiedName;
                return new PromiseBuilder<ModalResult>(_ownerNode, _behavior, _uidl);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public NavigateBehaviorModalInputBuilder<TInput, TPartInput> WithScreenPart<TPartInput>(IScreenPartWithInput<TPartInput> screenPart)
            {
                _behavior.NavigationType = NavigationType.PopupModal;
                _behavior.TargetQualifiedName = ((UidlScreenPart)screenPart).QualifiedName;
                return new NavigateBehaviorModalInputBuilder<TInput, TPartInput>(_ownerNode, _behavior, _uidl);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class NavigateBehaviorInputBuilder<TInput, TTargetInput>
        {
            private readonly ControlledUidlNode _ownerNode;
            private readonly UidlNavigateBehavior _behavior;
            private readonly UidlBuilder _uidl;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public NavigateBehaviorInputBuilder(ControlledUidlNode ownerNode, UidlNavigateBehavior behavior, UidlBuilder uidl)
            {
                _ownerNode = ownerNode;
                _behavior = behavior;
                _uidl = uidl;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public PromiseBuilder<TTargetInput> WithInput(Expression<Func<TInput, TData, TState, TTargetInput>> targetInputSelector)
            {
                _behavior.InputExpression = targetInputSelector.ToString();
                return new PromiseBuilder<TTargetInput>(_ownerNode, _behavior, _uidl);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class NavigateBehaviorModalInputBuilder<TInput, TTargetInput>
        {
            private readonly ControlledUidlNode _ownerNode;
            private readonly UidlNavigateBehavior _behavior;
            private readonly UidlBuilder _uidl;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public NavigateBehaviorModalInputBuilder(ControlledUidlNode ownerNode, UidlNavigateBehavior behavior, UidlBuilder uidl)
            {
                _ownerNode = ownerNode;
                _behavior = behavior;
                _uidl = uidl;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public PromiseBuilder<ModalResult> WithInput(Expression<Func<TInput, TData, TState, TTargetInput>> targetInputSelector)
            {
                _behavior.InputExpression = targetInputSelector.ToString();
                return new PromiseBuilder<ModalResult>(_ownerNode, _behavior, _uidl);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class BroadcastBehaviorBuilder3<TInput, TPayload>
        {
            private readonly ControlledUidlNode _ownerNode;
            private readonly UidlBroadcastBehavior _behavior;
            private readonly UidlBuilder _uidl;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public BroadcastBehaviorBuilder3(ControlledUidlNode ownerNode, UidlBroadcastBehavior behavior, UidlBuilder uidl)
            {
                _ownerNode = ownerNode;
                _behavior = behavior;
                _uidl = uidl;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public BroadcastBehaviorBuilder4<TInput> WithPayload(Expression<Func<TData, TState, TInput, TPayload>> payloadSelector)
            {
                _behavior.PayloadExpression = payloadSelector.ToString();
                return new BroadcastBehaviorBuilder4<TInput>(_ownerNode, _behavior, _uidl);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class BroadcastBehaviorBuilder4<TInput>
        {
            private readonly ControlledUidlNode _ownerNode;
            private readonly UidlBroadcastBehavior _behavior;
            private readonly UidlBuilder _uidl;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public BroadcastBehaviorBuilder4(ControlledUidlNode ownerNode, UidlBroadcastBehavior behavior, UidlBuilder uidl)
            {
                _ownerNode = ownerNode;
                _behavior = behavior;
                _uidl = uidl;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public PromiseBuilder<TInput> BubbleUp()
            {
                _behavior.Direction = BroadcastDirection.BubbleUp;
                return new PromiseBuilder<TInput>(_ownerNode, _behavior, _uidl);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public PromiseBuilder<TInput> TunnelDown()
            {
                _behavior.Direction = BroadcastDirection.TunnelDown;
                return new PromiseBuilder<TInput>(_ownerNode, _behavior, _uidl);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public PromiseBuilder<TInput> BubbleUpAndTunnelDown()
            {
                _behavior.Direction = BroadcastDirection.BubbleUpAndTunnelDown;
                return new PromiseBuilder<TInput>(_ownerNode, _behavior, _uidl);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public PromiseBuilder<TInput> Direction(BroadcastDirection direction)
            {
                _behavior.Direction = direction;
                return new PromiseBuilder<TInput>(_ownerNode, _behavior, _uidl);
            }
        }

        ////-----------------------------------------------------------------------------------------------------------------------------------------------------

        //public interface IShowAlertBehaviorBuilder<TInput, TData, TState>
        //    where TData : class
        //    where TState : class
        //{
        //    IShowAlertBehaviorBuilder2<TInput, TData, TState, TRepo> From<TRepo>() where TRepo : IApplicationAlertRepository;
        //}

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class ShowAlertBehaviorBuilder2<TInput, TRepo>
            where TRepo : IUserAlertRepository
        {
            private readonly ControlledUidlNode _ownerNode;
            private readonly UidlAlertUserBehavior _behavior;
            private readonly UidlBuilder _uidl;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            internal ShowAlertBehaviorBuilder2(ControlledUidlNode ownerNode, UidlAlertUserBehavior behavior, UidlBuilder uidl)
            {
                _behavior = behavior;
                _ownerNode = ownerNode;
                _uidl = uidl;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public PromiseBuilder<UserAlertResult> ShowInline(Expression<Func<TRepo, UidlUserAlert>> alertCall)
            {
                ParseAlertCall(alertCall);
                _behavior.DisplayMode = UserAlertDisplayMode.Inline;

                return new PromiseBuilder<UserAlertResult>(_ownerNode, _behavior, _uidl);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public PromiseBuilder<UserAlertResult> ShowInline(Expression<Func<TRepo, TData, TState, TInput, UidlUserAlert>> alertCall)
            {
                ParseAlertCall(alertCall);
                _behavior.DisplayMode = UserAlertDisplayMode.Inline;

                return new PromiseBuilder<UserAlertResult>(_ownerNode, _behavior, _uidl);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public PromiseBuilder<UserAlertResult> ShowPopup(Expression<Func<TRepo, UidlUserAlert>> alertCall)
            {
                ParseAlertCall(alertCall);
                _behavior.DisplayMode = UserAlertDisplayMode.Popup;

                return new PromiseBuilder<UserAlertResult>(_ownerNode, _behavior, _uidl);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public PromiseBuilder<UserAlertResult> ShowPopup(Expression<Func<TRepo, TData, TState, TInput, UidlUserAlert>> alertCall)
            {
                ParseAlertCall(alertCall);
                _behavior.DisplayMode = UserAlertDisplayMode.Popup;

                return new PromiseBuilder<UserAlertResult>(_ownerNode, _behavior, _uidl);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void ParseAlertCall(LambdaExpression alertCall)
            {
                Expression[] callArguments;
                var method = alertCall.GetMethodInfo(out callArguments);

                _behavior.AlertQualifiedName = _uidl.RegisterUserAlert(method);
                _behavior.ParameterExpressions = callArguments.Select(x => x.ToString()).ToArray();
            }
        }

        ////-----------------------------------------------------------------------------------------------------------------------------------------------------

        //public interface IShowAlertBehaviorBuilder3<TInput, TData, TState>
        //    where TData : class
        //    where TState : class
        //{
        //    IPromiseBuilder<UserAlertResult, TData, TState> Inline();
        //    IPromiseBuilder<UserAlertResult, TData, TState> Popup();
        //}

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class AlterModelBehaviorBuilder<TInput>
        {
            private readonly ControlledUidlNode _ownerNode;
            private readonly UidlAlterModelBehavior _behavior;
            private readonly UidlBuilder _uidl;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            internal AlterModelBehaviorBuilder(ControlledUidlNode ownerNode, UidlAlterModelBehavior behavior, UidlBuilder uidl)
            {
                _behavior = behavior;
                _ownerNode = ownerNode;
                _uidl = uidl;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public PromiseBuilder<TInput> SetData<TValue>(
                Expression<Func<TData, TValue>> propertySelector,
                Expression<Func<TData, TState, TInput, TValue>> newValue)
            {
                _behavior.WriteExpression = "model.data." + propertySelector.ToString();
                _behavior.ReadExpression = newValue.ToString();

                return new PromiseBuilder<TInput>(_ownerNode, _behavior, _uidl);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public PromiseBuilder<TInput> SetData<TValue>(Expression<Func<TData, TValue>> propertySelector, Expression<Func<TValue>> newValue)
            {
                _behavior.WriteExpression = "model.data." + propertySelector.ToString();
                _behavior.ReadExpression = newValue.ToString();

                return new PromiseBuilder<TInput>(_ownerNode, _behavior, _uidl);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public PromiseBuilder<TInput> SetState<TValue>(
                Expression<Func<TState, TValue>> propertySelector,
                Expression<Func<TData, TState, TInput, TValue>> newValue)
            {
                _behavior.WriteExpression = "model.state." + propertySelector.ToString();
                _behavior.ReadExpression = newValue.ToString();

                return new PromiseBuilder<TInput>(_ownerNode, _behavior, _uidl);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public PromiseBuilder<TInput> SetState<TValue>(
                Expression<Func<TState, TValue>> propertySelector,
                Expression<Func<TValue>> newValue)
            {
                _behavior.WriteExpression = "model.state." + propertySelector.ToString();
                _behavior.ReadExpression = newValue.ToString();

                return new PromiseBuilder<TInput>(_ownerNode, _behavior, _uidl);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class PromiseBuilder<TOutput>
        {
            private readonly ControlledUidlNode _ownerNode;
            private readonly BehaviorUidlNode _behaviorToComplete;
            private readonly UidlBuilder _uidl;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            internal PromiseBuilder(ControlledUidlNode ownerNode, BehaviorUidlNode behaviorToComplete, UidlBuilder uidl)
            {
                _uidl = uidl;
                _ownerNode = ownerNode;
                _behaviorToComplete = behaviorToComplete;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void Then(
                Action<BehaviorBuilder<TOutput>> onSuccess,
                Action<BehaviorBuilder<IPromiseFailureInfo>> onFailure = null
                /*TBD: , Action<IBehaviorBuilder<TOutput, TData, TState>> onProgress = null*/)
            {
                if ( onSuccess != null )
                {
                    var builder = new BehaviorBuilder<TOutput>(_ownerNode, notification: null, uidl: _uidl);
                    onSuccess(builder);
                    _behaviorToComplete.OnSuccess = builder.Behavior;
                }

                if ( onFailure != null )
                {
                    var builder = new BehaviorBuilder<IPromiseFailureInfo>(_ownerNode, notification: null, uidl: _uidl);
                    onFailure(builder);
                    _behaviorToComplete.OnFailure = builder.Behavior;
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class WhenBehaviorBuilder<TInput>
        {
            public WhenBehaviorBuilder(ControlledUidlNode ownerNode, UidlBuilder uidl)
            {
                this.OwnerNode = ownerNode;
                this.Uidl = uidl;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public UidlBranchByRuleBehavior Behavior { get; set; }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected ControlledUidlNode OwnerNode { get; private set; }
            protected UidlBuilder Uidl { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class WhenOtherwiseBehaviorBuilder<TInput>
        {
            private readonly ControlledUidlNode _ownerNode;
            private readonly UidlBranchByRuleBehavior _behavior;
            private readonly UidlBuilder _uidl;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            internal WhenOtherwiseBehaviorBuilder(
                ControlledUidlNode ownerNode, UidlBranchByRuleBehavior behavior, UidlBuilder uidl)
            {
                _uidl = uidl;
                _ownerNode = ownerNode;
                _behavior = behavior;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public PromiseBuilder<TInput> EndWhen()
            {
                return new PromiseBuilder<TInput>(_ownerNode, _behavior, _uidl);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public WhenOtherwiseBehaviorBuilder<TInput> When(
                Expression<Func<TData, TState, TInput, bool>> condition,
                Action<BehaviorBuilder<TInput>> onTrue)
            {
                var builder = new BehaviorBuilder<TInput>(_ownerNode, notification: null, uidl: _uidl);
                onTrue(builder);

                var rule = new UidlBranchByRuleBehavior.BranchRule() {
                    ConditionExpression = condition.ToString(),
                    OnTrue = builder.Behavior
                };

                _behavior.BranchRules.Add(rule);
                return this;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public PromiseBuilder<TInput> Otherwise(Action<BehaviorBuilder<TInput>> onOtherwise)
            {
                var builder = new BehaviorBuilder<TInput>(_ownerNode, notification: null, uidl: _uidl);
                onOtherwise(builder);
                _behavior.Otherwise = builder.Behavior;

                return new PromiseBuilder<TInput>(_ownerNode, _behavior.Otherwise, _uidl);
            }
        }
    }
}
