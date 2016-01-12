using NWheels.UI.Uidl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Hapil;
using NWheels.Extensions;
using NWheels.Processing;
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

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public BehaviorBuilder<TItem> On<TItem>(UidlCommandGroup<TItem> commandGroup)
        {
            return new BehaviorBuilder<TItem>(_ownerNode, commandGroup.Executing, _uidl);
        }

        ////-----------------------------------------------------------------------------------------------------------------------------------------------------

        //TElement New<TElement>() where TElement : IUIElement;

        ////-----------------------------------------------------------------------------------------------------------------------------------------------------

        //BehaviorDescription Behavior<TInput>(Action<IBehaviorBuilder<TInput, TData, TState>> describe);


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

            public SendServerCommandBehaviorBuilder<TInput, TScript> InvokeTransactionScript<TScript>(Type queryAsEntityType = null)
                where TScript : ITransactionScript
            {
                var behavior = new UidlCallApiBehavior(_ownerNode.GetUniqueBehaviorId(), _ownerNode);
                SetAndSubscribeBehavior(behavior);
                return new SendServerCommandBehaviorBuilder<TInput, TScript>(
                    _ownerNode, 
                    behavior, 
                    _uidl, 
                    ApiCallTargetType.TransactionScript, 
                    queryAsEntityType);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public SendServerCommandBehaviorBuilder<TInput, TScript> InvokeServiceMethod<TScript>(Type queryAsEntityType = null)
                where TScript : ITransactionScript
            {
                var behavior = new UidlCallApiBehavior(_ownerNode.GetUniqueBehaviorId(), _ownerNode);
                SetAndSubscribeBehavior(behavior);
                return new SendServerCommandBehaviorBuilder<TInput, TScript>(
                    _ownerNode, 
                    behavior, 
                    _uidl, 
                    ApiCallTargetType.ServiceMethod, 
                    queryAsEntityType);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public SendServerCommandBehaviorBuilder<TInput, TEntity> InvokeEntityMethod<TEntity>(Type queryAsEntityType = null)
                where TEntity : class
            {
                var behavior = new UidlCallApiBehavior(_ownerNode.GetUniqueBehaviorId(), _ownerNode);
                SetAndSubscribeBehavior(behavior);
                behavior.ContractName = behavior.MetadataCache.GetTypeMetadata(typeof(TEntity)).QualifiedName;
                return new SendServerCommandBehaviorBuilder<TInput, TEntity>(
                    _ownerNode, 
                    behavior, 
                    _uidl, 
                    ApiCallTargetType.EntityMethod, 
                    queryAsEntityType,
                    usePascalCase: true);
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

            public PromiseBuilder<TValue> QueryModel<TValue>(Expression<Func<ViewModel<TData, TState, TInput>, TValue>> source)
            {
                var behavior = new UidlQueryModelBehavior(_ownerNode.GetUniqueBehaviorId(), _ownerNode);
                SetAndSubscribeBehavior(behavior);
                behavior.SourceExpression = source.ToNormalizedNavigationString("model");
                return new PromiseBuilder<TValue>(_ownerNode, _behavior, _uidl);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public PromiseBuilder<TValue> QueryModelConstant<TValue>(TValue constantValue = default(TValue))
            {
                var behavior = new UidlQueryModelBehavior(_ownerNode.GetUniqueBehaviorId(), _ownerNode);
                SetAndSubscribeBehavior(behavior);
                behavior.ConstantValue = constantValue;
                return new PromiseBuilder<TValue>(_ownerNode, _behavior, _uidl);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public PromiseBuilder<TInput> AlterModel(params Action<AlterModelBehaviorBuilder<TInput>>[] alterations)
            {
                var behavior = new UidlAlterModelBehavior(_ownerNode.GetUniqueBehaviorId(), _ownerNode);
                SetAndSubscribeBehavior(behavior);

                var alterationBuilder = new AlterModelBehaviorBuilder<TInput>(_ownerNode, behavior, _uidl);

                foreach ( var alteration in alterations )
                {
                    alteration(alterationBuilder);
                }

                return new PromiseBuilder<TInput>(_ownerNode, _behavior, _uidl);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public PromiseBuilder<TInput> ActivateSessionTimeout(Expression<Func<ViewModel<TData, TState, TInput>, int>> timeoutMinutes = null)
            {
                var behavior = new UidlActivateSessionTimeoutBehavior(_ownerNode.GetUniqueBehaviorId(), _ownerNode);
                SetAndSubscribeBehavior(behavior);

                if ( timeoutMinutes != null )
                {
                    behavior.IdleMinutesExpression = timeoutMinutes.ToNormalizedNavigationString("model");
                }

                return new PromiseBuilder<TInput>(_ownerNode, _behavior, _uidl);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public PromiseBuilder<TInput> DeactivateSessionTimeout()
            {
                var behavior = new UidlDeactivateSessionTimeoutBehavior(_ownerNode.GetUniqueBehaviorId(), _ownerNode);
                SetAndSubscribeBehavior(behavior);
                return new PromiseBuilder<TInput>(_ownerNode, _behavior, _uidl);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public PromiseBuilder<TInput> RestartApp()
            {
                var behavior = new UidlRestartAppBehavior(_ownerNode.GetUniqueBehaviorId(), _ownerNode);
                SetAndSubscribeBehavior(behavior);
                return new PromiseBuilder<TInput>(_ownerNode, _behavior, _uidl);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public PromiseBuilder<TInput> BeginDownloadContent(Expression<Func<ViewModel<TData, TState, TInput>, string>> contentId)
            {
                var behavior = new UidlDownloadContentBehavior(_ownerNode.GetUniqueBehaviorId(), _ownerNode);
                SetAndSubscribeBehavior(behavior);
                behavior.ContentIdExpression = contentId.ToNormalizedNavigationString("model");
                return new PromiseBuilder<TInput>(_ownerNode, _behavior, _uidl);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public WhenOtherwiseBehaviorBuilder<TInput> When(Expression<Func<TData, TState, TInput, bool>> condition, Action<BehaviorBuilder<TInput>> onTrue)
            {
                var behavior = new UidlBranchByRuleBehavior(_ownerNode.GetUniqueBehaviorId(), _ownerNode);
                SetAndSubscribeBehavior(behavior);
                return new WhenOtherwiseBehaviorBuilder<TInput>(_ownerNode, behavior, _uidl);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public PromiseBuilder<TInput> PopupEntityMethodForm<TEntity, TMethodIn, TMethodOut>(
                EntityMethodForm<TEntity, TMethodIn, TMethodOut> form,
                Expression<Func<TEntity, Empty.Data, EntityMethodForm<TEntity, TMethodIn, TMethodOut>.IState, Empty.Payload, TMethodOut>> onExecute)
                where TEntity : class
                where TMethodIn : class
                where TMethodOut : class
            {
                form.OnExecute(onExecute);
                return Broadcast(form.ShowModal).TunnelDown();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public PromiseBuilder<TInput> PopupEntityMethodForm<TEntity, TMethodIn>(
                EntityMethodForm<TEntity, TMethodIn> form,
                Expression<Action<TEntity, Empty.Data, EntityMethodForm<TEntity, TMethodIn>.IState, Empty.Payload>> onExecute)
                where TEntity : class
                where TMethodIn : class
            {
                form.OnExecute(onExecute);
                return Broadcast(form.ShowModal).TunnelDown();
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
                _behavior.CallTargetType = ApiCallTargetType.DomainApi;
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

        public class SendServerCommandBehaviorBuilder<TInput, TContract>
        {
            private readonly ControlledUidlNode _ownerNode;
            private readonly UidlCallApiBehavior _behavior;
            private readonly UidlBuilder _uidl;
            private readonly bool _usePascalCase;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public SendServerCommandBehaviorBuilder(
                ControlledUidlNode ownerNode, 
                UidlCallApiBehavior behavior, 
                UidlBuilder uidl, 
                ApiCallTargetType targetType,
                Type queryAsEntityType,
                bool usePascalCase = false)
            {
                _ownerNode = ownerNode;
                _behavior = behavior;
                _behavior.CallTargetType = targetType;
                _behavior.CallResultType = (queryAsEntityType != null ? ApiCallResultType.EntityQuery : ApiCallResultType.Command);
                _behavior.QueryEntityName = (queryAsEntityType != null ? uidl.MetadataCache.GetTypeMetadata(queryAsEntityType).QualifiedName : null);
                _uidl = uidl;
                _usePascalCase = usePascalCase;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public SendServerCommandBehaviorBuilder<TInput, TContract> SetupEnityQueryFor(DataGrid dataGrid)
            {
                _behavior.QueryEntityName = dataGrid.EntityName;
                _behavior.QueryIncludeList = dataGrid.IncludedProperties;
                _behavior.QuerySelectList = dataGrid.DisplayColumns.Select(c => c.Expression).Where(name => !_behavior.QueryIncludeList.Contains(name)).ToList();
                return this;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public PromiseBuilder<TInput> FireAndForget(Expression<Action<TContract, ViewModel<TData, TState, TInput>>> call)
            {
                ParseMethodCall(call);
                _behavior.CallType = ApiCallType.OneWay;
                return new PromiseBuilder<TInput>(_ownerNode, _behavior, _uidl);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public PromiseBuilder<TInput> WaitForCompletion(Expression<Action<TContract, ViewModel<TData, TState, TInput>>> call)
            {
                ParseMethodCall(call);
                _behavior.CallType = ApiCallType.RequestReply;
                return new PromiseBuilder<TInput>(_ownerNode, _behavior, _uidl);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public PromiseBuilder<TReply> WaitForReply<TReply>(Expression<Func<TContract, ViewModel<TData, TState, TInput>, TReply>> call)
            {
                ParseMethodCall(call);
                _behavior.CallType = ApiCallType.RequestReply;
                return new PromiseBuilder<TReply>(_ownerNode, _behavior, _uidl);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public PromiseBuilder<TReply> WaitForReplyOrCompletion<TReply>(LambdaExpression call)
            {
                ParseMethodCall(call);
                _behavior.CallType = ApiCallType.RequestReply;
                return new PromiseBuilder<TReply>(_ownerNode, _behavior, _uidl);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public PromiseBuilder<object> PrepareWaitForReply<TReply>(Expression<Func<TContract, ViewModel<TData, TState, TInput>, TReply>> call)
            {
                _behavior.PrepareOnly = true;
                _behavior.CallType = ApiCallType.RequestReply;
                ParseMethodCall(call);
                return new PromiseBuilder<object>(_ownerNode, _behavior, _uidl);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public PromiseBuilder<string> WaitForResultsDownloadReady(Expression<Action<TContract, ViewModel<TData, TState, TInput>>> call, string exportFormat)
            {
                ParseMethodCall(call);
                _behavior.CallType = ApiCallType.RequestReply;
                _behavior.CallResultType = ApiCallResultType.EntityQueryExport;
                _behavior.ExportFormatId = exportFormat;
                return new PromiseBuilder<string>(_ownerNode, _behavior, _uidl);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public PromiseBuilder<TInput> WaitForCompletionAsync(Expression<Action<TContract, ViewModel<TData, TState, TInput>>> call)
            {
                ParseMethodCall(call);
                _behavior.CallType = ApiCallType.RequestReplyAsync;
                return new PromiseBuilder<TInput>(_ownerNode, _behavior, _uidl);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public PromiseBuilder<TReply> WaitForReplyAsync<TReply>(Expression<Func<TContract, ViewModel<TData, TState, TInput>, TReply>> call)
            {
                ParseMethodCall(call);
                _behavior.CallType = ApiCallType.RequestReplyAsync;
                return new PromiseBuilder<TReply>(_ownerNode, _behavior, _uidl);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public PromiseBuilder<TReply> WaitForReplyOrCompletionAsync<TReply>(LambdaExpression call)
            {
                ParseMethodCall(call);
                _behavior.CallType = ApiCallType.RequestReplyAsync;
                return new PromiseBuilder<TReply>(_ownerNode, _behavior, _uidl);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public PromiseBuilder<object> PrepareWaitForReplyAsync<TReply>(Expression<Func<TContract, ViewModel<TData, TState, TInput>, TReply>> call)
            {
                _behavior.PrepareOnly = true;
                _behavior.CallType = ApiCallType.RequestReplyAsync;
                ParseMethodCall(call);
                return new PromiseBuilder<object>(_ownerNode, _behavior, _uidl);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public PromiseBuilder<string> WaitForResultsDownloadReadyAsync(Expression<Action<TContract, ViewModel<TData, TState, TInput>>> call, string exportFormat)
            {
                ParseMethodCall(call);
                _behavior.CallType = ApiCallType.RequestReplyAsync;
                _behavior.CallResultType = ApiCallResultType.EntityQueryExport;
                _behavior.ExportFormatId = exportFormat;
                return new PromiseBuilder<string>(_ownerNode, _behavior, _uidl);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void ParseMethodCall(LambdaExpression methodCall)
            {
                Expression callTarget;
                Expression[] callArguments;
                var method = methodCall.GetMethodInfo(out callTarget, out callArguments);

                if ( _behavior.CallTargetType != ApiCallTargetType.EntityMethod )
                {
                    _behavior.ContractName = callTarget.Type.SimpleQualifiedName();
                }

                _behavior.OperationName = method.Name;
                _behavior.ParameterNames = method.GetParameters().Select(p => p.Name).ToArray();
                _behavior.ParameterExpressions = callArguments.Select(ToParameterExpressionString).ToArray();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private string ToParameterExpressionString(Expression parameter)
            {
                var effectiveExpression = string.Join(".", parameter.ToString().Split('.').Skip(1).ToArray());
                return effectiveExpression.ToString();

                //if ( _usePascalCase )
                //{
                //    return parameter.ToString();
                //}
                //else
                //{
                //    return parameter.ToString().ToCamelCaseExpression();
                //}
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

            public BroadcastBehaviorBuilder4<TInput> WithPayload(Expression<Func<ViewModel<TData, TState, TInput>, TPayload>> payloadSelector)
            {
                _behavior.PayloadExpression = (payloadSelector != null ? payloadSelector.ToString() : "null");
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

            public PromiseBuilder<TOutput> Show<TOutput>(
                UserAlertDisplayMode displayMode,
                Expression<Func<TRepo, ViewModel<TData, TState, TInput>, UidlUserAlert>> alertCall,
                Expression<Func<ViewModel<TData, TState, TInput>, IPromiseFailureInfo>> faultInfo = null)
            {
                ParseAlertCall(alertCall);
                ParseFaultInfoParameter(faultInfo);

                _behavior.DisplayMode = displayMode;

                return new PromiseBuilder<TOutput>(_ownerNode, _behavior, _uidl);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public PromiseBuilder<UserAlertResult> ShowInline(
                Expression<Func<TRepo, ViewModel<TData, TState, TInput>, UidlUserAlert>> alertCall,
                Expression<Func<ViewModel<TData, TState, TInput>, IPromiseFailureInfo>> faultInfo = null)
            {
                return Show<UserAlertResult>(UserAlertDisplayMode.Inline, alertCall, faultInfo);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public PromiseBuilder<TInput> ShowPopup(
                Expression<Func<TRepo, ViewModel<TData, TState, TInput>, UidlUserAlert>> alertCall,
                Expression<Func<ViewModel<TData, TState, TInput>, IPromiseFailureInfo>> faultInfo = null)
            {
                return Show<TInput>(UserAlertDisplayMode.Popup, alertCall, faultInfo);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public PromiseBuilder<UserAlertResult> ShowModal(
                Expression<Func<TRepo, ViewModel<TData, TState, TInput>, UidlUserAlert>> alertCall,
                Expression<Func<ViewModel<TData, TState, TInput>, IPromiseFailureInfo>> faultInfo = null)
            {
                return Show<UserAlertResult>(UserAlertDisplayMode.Modal, alertCall, faultInfo);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            //private void ParseAlertCall(LambdaExpression alertCall)
            //{
            //    Expression[] callArguments;
            //    var method = alertCall.GetMethodInfo(out callArguments);

            //    _behavior.AlertQualifiedName = _uidl.RegisterUserAlert(method);
            //    _behavior.ParameterExpressions = callArguments.Select(x => x.ToString()).ToArray();
            //}

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void ParseFaultInfoParameter(Expression<Func<ViewModel<TData, TState, TInput>, IPromiseFailureInfo>> parameter)
            {
                if ( parameter != null )
                {
                    _behavior.FaultInfoExpression = parameter.ToNormalizedNavigationString("model");
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void ParseAlertCall(LambdaExpression call)
            {
                MethodInfo method;
                string[] parameterExpressions;

                call.ParseNormalizedMethodAndParameters(out method, out parameterExpressions, "$repo", "model");

                _behavior.AlertQualifiedName = _uidl.RegisterUserAlert(method);
                _behavior.ParameterExpressions = parameterExpressions;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface IAlterModelSourceSelector<TInput>
        {
            IAlterModelDestinationSelector<TInput, TValue> Copy<TValue>(TValue value);
            IAlterModelDestinationSelector<TInput, TValue> Copy<TValue>(Expression<Func<ViewModel<TData, TState, TInput>, TValue>> source);
            IAlterModelDestinationSelector<TInput, ICollection<TValue>> InsertOne<TValue>(TValue value);
            IAlterModelDestinationSelector<TInput, ICollection<TValue>> InsertOne<TValue>(Expression<Func<ViewModel<TData, TState, TInput>, TValue>> source);
            IAlterModelDestinationSelector<TInput, ICollection<TValue>> RemoveOne<TValue>(TValue value);
            IAlterModelDestinationSelector<TInput, ICollection<TValue>> RemoveOne<TValue>(Expression<Func<ViewModel<TData, TState, TInput>, TValue>> source);
            IAlterModelDestinationSelector<TInput, ICollection<TValue>> InsertMany<TValue>(Expression<Func<ViewModel<TData, TState, TInput>, ICollection<TValue>>> source);
            IAlterModelDestinationSelector<TInput, ICollection<TValue>> RemoveMany<TValue>(Expression<Func<ViewModel<TData, TState, TInput>, ICollection<TValue>>> source);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface IAlterModelDestinationSelector<TInput, TDestination>
        {
            void To(Expression<Func<ViewModel<TData, TState, TInput>, TDestination>> destination);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class AlterModelBehaviorBuilder<TInput> : IAlterModelSourceSelector<TInput>
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

            #region Implementation of IAlterModelSourceSelector<TInput>

            public IAlterModelDestinationSelector<TInput, TValue> Copy<TValue>(TValue value)
            {
                CurrentAlteration = new UidlAlterModelBehavior.Alteration() {
                    Type = UidlAlterModelBehavior.AlterationType.Copy,
                    SourceExpression = value.ToString()
                };

                return new DestinationSelector<TValue>(this);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public IAlterModelDestinationSelector<TInput, TValue> Copy<TValue>(Expression<Func<ViewModel<TData, TState, TInput>, TValue>> source)
            {
                CurrentAlteration = new UidlAlterModelBehavior.Alteration() {
                    Type = UidlAlterModelBehavior.AlterationType.Copy,
                    SourceExpression = source.ToNormalizedNavigationString("model")
                };

                return new DestinationSelector<TValue>(this);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public IAlterModelDestinationSelector<TInput, ICollection<TValue>> InsertOne<TValue>(TValue value)
            {
                CurrentAlteration = new UidlAlterModelBehavior.Alteration() {
                    Type = UidlAlterModelBehavior.AlterationType.InsertOne,
                    SourceExpression = value.ToString()
                };

                return new DestinationSelector<ICollection<TValue>>(this);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public IAlterModelDestinationSelector<TInput, ICollection<TValue>> InsertOne<TValue>(
                Expression<Func<ViewModel<TData, TState, TInput>, TValue>> source)
            {
                CurrentAlteration = new UidlAlterModelBehavior.Alteration() {
                    Type = UidlAlterModelBehavior.AlterationType.InsertOne,
                    SourceExpression = source.ToNormalizedNavigationString("model")
                };

                return new DestinationSelector<ICollection<TValue>>(this);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public IAlterModelDestinationSelector<TInput, ICollection<TValue>> RemoveOne<TValue>(TValue value)
            {
                CurrentAlteration = new UidlAlterModelBehavior.Alteration() {
                    Type = UidlAlterModelBehavior.AlterationType.RemoveOne,
                    SourceExpression = value.ToString()
                };

                return new DestinationSelector<ICollection<TValue>>(this);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public IAlterModelDestinationSelector<TInput, ICollection<TValue>> RemoveOne<TValue>(
                Expression<Func<ViewModel<TData, TState, TInput>, TValue>> source)
            {
                CurrentAlteration = new UidlAlterModelBehavior.Alteration() {
                    Type = UidlAlterModelBehavior.AlterationType.RemoveOne,
                    SourceExpression = source.ToNormalizedNavigationString("model")
                };

                return new DestinationSelector<ICollection<TValue>>(this);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public IAlterModelDestinationSelector<TInput, ICollection<TValue>> InsertMany<TValue>(
                Expression<Func<ViewModel<TData, TState, TInput>, ICollection<TValue>>> source)
            {
                CurrentAlteration = new UidlAlterModelBehavior.Alteration() {
                    Type = UidlAlterModelBehavior.AlterationType.InsertMany,
                    SourceExpression = source.ToNormalizedNavigationString("model")
                };

                return new DestinationSelector<ICollection<TValue>>(this);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public IAlterModelDestinationSelector<TInput, ICollection<TValue>> RemoveMany<TValue>(
                Expression<Func<ViewModel<TData, TState, TInput>, ICollection<TValue>>> source)
            {
                CurrentAlteration = new UidlAlterModelBehavior.Alteration() {
                    Type = UidlAlterModelBehavior.AlterationType.RemoveMany,
                    SourceExpression = source.ToNormalizedNavigationString("model")
                };

                return new DestinationSelector<ICollection<TValue>>(this);
            }

            #endregion

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void FinalizeAlteration(LambdaExpression destinationExpression)
            {
                if ( CurrentAlteration != null )
                {
                    CurrentAlteration.DestinationExpression = destinationExpression.ToNormalizedNavigationString("model");
                    CurrentAlteration.DestinationNavigations = CurrentAlteration.DestinationExpression.Split('.');
                    _behavior.Alterations.Add(CurrentAlteration);
                    CurrentAlteration = null;
                }
                else
                {
                    throw new InvalidOperationException("No current alteration(?)");
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private UidlAlterModelBehavior.Alteration CurrentAlteration { get; set; }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private class DestinationSelector<TDestination> : IAlterModelDestinationSelector<TInput, TDestination>
            {
                private readonly AlterModelBehaviorBuilder<TInput> _ownerBuilder;

                //---------------------------------------------------------------------------------------------------------------------------------------------

                public DestinationSelector(AlterModelBehaviorBuilder<TInput> ownerBuilder)
                {
                    _ownerBuilder = ownerBuilder;
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                #region Implementation of IAlterModelDestinationSelector<TInput,TDestination>

                public void To(Expression<Func<ViewModel<TData, TState, TInput>, TDestination>> destination)
                {
                    _ownerBuilder.FinalizeAlteration(destination);
                }

                #endregion
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
