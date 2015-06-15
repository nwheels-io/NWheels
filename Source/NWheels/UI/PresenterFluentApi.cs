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
            IAbstractPresenterBuilder<TView, TData, TState> ToData(Expression<Func<TData, TValue>> dataProperty);
            IAbstractPresenterBuilder<TView, TData, TState> ToState(Expression<Func<TState, TValue>> stateProperty);
            
            IAbstractPresenterBuilder<TView, TData, TState> ToApi<TApiContract>(Expression<Func<TApiContract, TValue>> apiCall);
            IAbstractPresenterBuilder<TView, TData, TState> ToApi<TApiContract, TReply>(
                Expression<Func<TApiContract, TReply>> apiCall, 
                Expression<Func<TReply, TValue>> valueSelector);

            IAbstractPresenterBuilder<TView, TData, TState> ToEntity<TEntity>(
                Action<IQueryable<TEntity>> query,
                Expression<Func<TEntity[], TValue>> valueSelector)
                where TEntity : class;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface IBehaviorBuilder<TInput, TData, TState> : IWhenBehaviorBuilder<TInput, TData, TState>
            where TData : class
            where TState : class
        {
            ICallApiBehaviorBuilder<TInput, TData, TState, TContract> CallApi<TContract>();
            
            INavigateBehaviorBuilder<TInput, TData, TState> Navigate();
            
            IBroadcastBehaviorBuilder<TInput, TData, TState> Broadcast();
            IBroadcastBehaviorBuilder4<TInput, TData, TState> Broadcast(INotification notification);
            IBroadcastBehaviorBuilder3<TInput, TData, TState, TPayload> Broadcast<TPayload>(INotification<TPayload> notification);
            
            IShowAlertBehaviorBuilder<TInput, TData, TState> ShowAlert();

            IAlterModelBehaviorBuilder<TInput, TData, TState> AlterModel();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface ICallApiBehaviorBuilder<TInput, TData, TState, TContract>
            where TData : class
            where TState : class
        {
            IPromiseBuilder<TInput, TData, TState> OneWay(Expression<Action<TContract, TData, TState, TInput>> call);
            IPromiseBuilder<TInput, TData, TState> RequestReply(Expression<Action<TContract, TData, TState, TInput>> call);
            IPromiseBuilder<TReply, TData, TState> RequestReply<TReply>(Expression<Func<TContract, TData, TState, TInput, TReply>> call) where TReply : class;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface INavigateBehaviorBuilder<TInput, TData, TState>
            where TData : class 
            where TState : class
        {
            IPromiseBuilder<TInput, TData, TState> ToScreen<TScreen>() 
                where TScreen : IScreen<Empty.InputParam>;
            
            IPromiseBuilder<TInput, TData, TState> ToScreen<TScreen, TParam>(
                Expression<Func<TData, TState, TParam>> paramSelector) 
                where TParam : class 
                where TScreen : IScreen<TParam>;

            IPromiseBuilder<TInput, TData, TState> ToScreenPart<TScreenPart>(
                Toolbox.IScreenPartContainerWidget targetContainer)
                where TScreenPart : IScreenPart<Empty.InputParam>;

            IPromiseBuilder<TInput, TData, TState> ToScreenPart<TScreenPart, TParam>(
                Toolbox.IScreenPartContainerWidget targetContainer,
                Expression<Func<TData, TState, TParam>> paramSelector)
                where TParam : class
                where TScreenPart : IScreenPart<TParam>;

            IPromiseBuilder<ModalResult, TData, TState> ShowScreenPartDialog<TScreenPart>()
                where TScreenPart : IScreenPart<Empty.InputParam>;

            IPromiseBuilder<ModalResult, TData, TState> ShowScreenPartDialog<TScreenPart, TParam>(
                Expression<Func<TData, TState, TParam>> paramSelector)
                where TParam : class
                where TScreenPart : IScreenPart<TParam>;
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

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface IShowAlertBehaviorBuilder<TInput, TData, TState>
            where TData : class
            where TState : class
        {
            IShowAlertBehaviorBuilder2<TInput, TData, TState, TContainer> From<TContainer>() where TContainer : IUIElementContainer;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface IShowAlertBehaviorBuilder2<TInput, TData, TState, TContainer>
            where TData : class
            where TState : class
            where TContainer : IUIElementContainer
        {
            IShowAlertBehaviorBuilder3<TInput, TData, TState> Alert(Expression<Func<TContainer, TData, TState, TInput, IUserAlert>> alertCall);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface IShowAlertBehaviorBuilder3<TInput, TData, TState>
            where TData : class
            where TState : class
        {
            IPromiseBuilder<UserAlertResult, TData, TState> Inline();
            IPromiseBuilder<UserAlertResult, TData, TState> Popup();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface IAlterModelBehaviorBuilder<TInput, TData, TState>
            where TData : class
            where TState : class
        {
            //TBD
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
            void Otherwise(Expression<Action<IBehaviorBuilder<TInput, TData, TState>>> onOtherwise);
        }
    }
}
