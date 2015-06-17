using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.UI
{
    public static class PresenterFluentApi
    {
        public interface IBindingSourceBuilder<TView, TData, TState, TValue>
            where TView : IUIElementContainer
            where TData : class
            where TState : class
        {
            IAbstractPresenter<TView, TData, TState> ToData(Expression<Func<TData, TValue>> dataProperty);
            IAbstractPresenter<TView, TData, TState> ToState(Expression<Func<TState, TValue>> stateProperty);
            
            IAbstractPresenter<TView, TData, TState> ToApi<TApiContract>(Expression<Func<TApiContract, TValue>> apiCall);
            IAbstractPresenter<TView, TData, TState> ToApi<TApiContract, TReply>(
                Expression<Func<TApiContract, TReply>> apiCall, 
                Expression<Func<TReply, TValue>> valueSelector);

            IAbstractPresenter<TView, TData, TState> ToEntity<TEntity>(
                Action<IQueryable<TEntity>> query,
                Expression<Func<TEntity[], TValue>> valueSelector)
                where TEntity : class;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface IBehaviorBuilder<TInput, TData, TState> : IWhenBehaviorBuilder<TInput, TData, TState>
            where TData : class
            where TState : class
        {
            IPromiseBuilder<TInput, TData, TState> PerformCommand(ICommand command);

            ICallApiBehaviorBuilder<TInput, TData, TState, TContract> CallApi<TContract>();
            
            INavigateBehaviorBuilder<TInput, TData, TState> Navigate();
            
            IBroadcastBehaviorBuilder<TInput, TData, TState> Broadcast();
            IBroadcastBehaviorBuilder4<TInput, TData, TState> Broadcast(INotification notification);
            IBroadcastBehaviorBuilder3<TInput, TData, TState, TPayload> Broadcast<TPayload>(INotification<TPayload> notification);

            IShowAlertBehaviorBuilder2<TInput, TData, TState, TRepo> UserAlertFrom<TRepo>() where TRepo : IApplicationAlertRepository;

            IAlterModelBehaviorBuilder<TInput, TData, TState> AlterModel();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface ICallApiBehaviorBuilder<TInput, TData, TState, TContract>
            where TData : class
            where TState : class
        {
            IPromiseBuilder<TInput, TData, TState> OneWay(Expression<Action<TContract, TData, TState, TInput>> call);
            IPromiseBuilder<TInput, TData, TState> RequestReply(Expression<Action<TContract, TData, TState, TInput>> call);
            IPromiseBuilder<TReply, TData, TState> RequestReply<TReply>(Expression<Func<TContract, TData, TState, TInput, TReply>> call);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface INavigateBehaviorBuilder<TInput, TData, TState>
            where TData : class 
            where TState : class
        {
            IPromiseBuilder<TInput, TData, TState> ToScreen(IScreen screen);
            IPromiseBuilder<TInput, TData, TState> ToScreen(IScreenWithInput<Empty.Input> screen);
            IPromiseBuilder<TInput, TData, TState> ToScreen<TScreen>() where TScreen : IScreen;
            
            INavigateBehaviorInputBuilder<TInput, TData, TState, TScreenInput> ToScreen<TScreenInput>(IScreenWithInput<TScreenInput> screen);
            INavigateBehaviorInputBuilder<TInput, TData, TState, TScreenInput> ToScreen<TScreen, TScreenInput>() where TScreen : IScreenWithInput<TScreenInput>;

            INavigateBehaviorScreenPartSelector<TInput, TData, TState> FromContainer(Toolbox.ScreenPartContainer container);
            INavigateBehaviorModalScreenPartSelector<TInput, TData, TState> ShowModalDialog();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface INavigateBehaviorScreenPartSelector<TInput, TData, TState>
            where TData : class
            where TState : class
        {
            IPromiseBuilder<TInput, TData, TState> ToScreenPart(IScreenPart screenPart);
            IPromiseBuilder<TInput, TData, TState> ToScreenPart<TPart>() where TPart : IScreenPart;

            INavigateBehaviorInputBuilder<TInput, TData, TState, TPartInput> ToScreenPart<TPartInput>(IScreenPartWithInput<TPartInput> screenPart);
            INavigateBehaviorInputBuilder<TInput, TData, TState, TPartInput> ToScreenPart<TPart, TPartInput>() where TPart : IScreenWithInput<TPartInput>;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface INavigateBehaviorModalScreenPartSelector<TInput, TData, TState>
            where TData : class
            where TState : class
        {
            IPromiseBuilder<ModalResult, TData, TState> WithScreenPart(IScreenPart screenPart);
            IPromiseBuilder<ModalResult, TData, TState> WithScreenPart<TPart>() where TPart : IScreenPart;

            INavigateBehaviorModalInputBuilder<TInput, TData, TState, TPartInput> WithScreenPart<TPartInput>(IScreenPartWithInput<TPartInput> screenPart);
            INavigateBehaviorModalInputBuilder<TInput, TData, TState, TPartInput> WithScreenPart<TPart, TPartInput>() where TPart : IScreenWithInput<TPartInput>;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface INavigateBehaviorInputBuilder<TInput, TData, TState, TTargetInput>
            where TData : class
            where TState : class
        {
            IPromiseBuilder<TTargetInput, TData, TState> WithInput(Expression<Func<TInput, TData, TState, TTargetInput>> targetInputSelector);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface INavigateBehaviorModalInputBuilder<TInput, TData, TState, TTargetInput>
            where TData : class
            where TState : class
        {
            IPromiseBuilder<ModalResult, TData, TState> WithInput(Expression<Func<TInput, TData, TState, TTargetInput>> targetInputSelector);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface IBroadcastBehaviorBuilder<TInput, TData, TState>
            where TData : class
            where TState : class
        {
            IBroadcastBehaviorBuilder2<TInput, TData, TState, TContainer> From<TContainer>() where TContainer : IUIElementContainer;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface IBroadcastBehaviorBuilder2<TInput, TData, TState, TContainer>
            where TData : class
            where TState : class
            where TContainer : IUIElementContainer
        {
            IBroadcastBehaviorBuilder4<TInput, TData, TState> Notification(
                Expression<Func<TContainer, INotification>> notificationSelector);
            
            IBroadcastBehaviorBuilder3<TInput, TData, TState, TPayload> Notification<TPayload>(
                Expression<Func<TContainer, INotification<TPayload>>> notificationSelector);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface IBroadcastBehaviorBuilder3<TInput, TData, TState, TPayload>
            where TData : class
            where TState : class
        {
            IBroadcastBehaviorBuilder4<TInput, TData, TState> WithPayload(Expression<Func<TData, TState, TInput, TPayload>> payloadSelector);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface IBroadcastBehaviorBuilder4<TInput, TData, TState>
            where TData : class
            where TState : class
        {
            IPromiseBuilder<TInput, TData, TState> BubbleUp();
            IPromiseBuilder<TInput, TData, TState> TunnelDown();
            IPromiseBuilder<TInput, TData, TState> BubbleUpAndTunnelDown();
        }

        ////-----------------------------------------------------------------------------------------------------------------------------------------------------

        //public interface IShowAlertBehaviorBuilder<TInput, TData, TState>
        //    where TData : class
        //    where TState : class
        //{
        //    IShowAlertBehaviorBuilder2<TInput, TData, TState, TRepo> From<TRepo>() where TRepo : IApplicationAlertRepository;
        //}

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface IShowAlertBehaviorBuilder2<TInput, TData, TState, TRepo>
            where TData : class
            where TState : class
            where TRepo : IApplicationAlertRepository
        {
            IPromiseBuilder<UserAlertResult, TData, TState> ShowInline(Expression<Func<TRepo, IUserAlert>> alertCall);
            IPromiseBuilder<UserAlertResult, TData, TState> ShowInline(Expression<Func<TRepo, TData, TState, TInput, IUserAlert>> alertCall);

            IPromiseBuilder<UserAlertResult, TData, TState> ShowPopup(Expression<Func<TRepo, IUserAlert>> alertCall);
            IPromiseBuilder<UserAlertResult, TData, TState> ShowPopup(Expression<Func<TRepo, TData, TState, TInput, IUserAlert>> alertCall);
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

        public interface IAlterModelBehaviorBuilder<TInput, TData, TState>
            where TData : class
            where TState : class
        {
            IPromiseBuilder<TInput, TData, TState> SetData<TValue>(
                Expression<Func<TData, TValue>> propertySelector,
                Expression<Func<TData, TState, TInput, TValue>> newValue);

            IPromiseBuilder<TInput, TData, TState> SetData<TValue>(
                Expression<Func<TData, TValue>> propertySelector,
                Expression<Func<TValue>> newValue);

            IPromiseBuilder<TInput, TData, TState> SetState<TValue>(
                Expression<Func<TState, TValue>> propertySelector,
                Expression<Func<TData, TState, TInput, TValue>> newValue);

            IPromiseBuilder<TInput, TData, TState> SetState<TValue>(
                Expression<Func<TState, TValue>> propertySelector,
                Expression<Func<TValue>> newValue);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface IPromiseBuilder<TOutput, TData, TState>
            where TData : class
            where TState : class
        {
            void Then(
                Expression<Action<IBehaviorBuilder<TOutput, TData, TState>>> onSuccess,
                Expression<Action<IBehaviorBuilder<IPromiseFailureInfo, TData, TState>>> onFailure = null
                /*TBD: , Expression<Action<IBehaviorBuilder<TOutput, TData, TState>>> onProgress = null*/);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface IWhenBehaviorBuilder<TInput, TData, TState>
            where TData : class
            where TState : class
        {
            IWhenOtherwiseBehaviorBuilder<TInput, TData, TState> When(
                Expression<Func<TData, TState, TInput, bool>> conition,
                Expression<Action<IBehaviorBuilder<TInput, TData, TState>>> onTrue);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface IWhenOtherwiseBehaviorBuilder<TInput, TData, TState> : IWhenBehaviorBuilder<TInput, TData, TState>
            where TData : class
            where TState : class
        {
            IPromiseBuilder<TInput, TData, TState> EndWhen();
            IPromiseBuilder<TInput, TData, TState> Otherwise(Expression<Action<IBehaviorBuilder<TInput, TData, TState>>> onOtherwise);
        }
    }
}
