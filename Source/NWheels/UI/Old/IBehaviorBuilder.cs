using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.UI
{
    public interface IBehaviorBuilder<TInput, TModel, TState> : IWhenBehaviorBuilder<TInput, TModel, TState>
    {
        IApiCallBehaviorBuilder<TInput, TModel, TState, TContract> CallApi<TContract>();
        INavigationBehaviorBuilder<TInput, TModel, TState> Navigate();

        IPromiseBuilder<TInput, TModel, TState> Broadcast<TRepo>(Expression<Action<TRepo>> notification) where TRepo : IUINotificationRepository;
        IPromiseBuilder<TInput, TModel, TState> Broadcast<TRepo>(Expression<Action<TRepo, TModel>> notification) where TRepo : IUINotificationRepository;
        IPromiseBuilder<TInput, TModel, TState> Broadcast<TRepo>(Expression<Action<TRepo, TModel, TState>> notification) where TRepo : IUINotificationRepository;
        IPromiseBuilder<TInput, TModel, TState> Broadcast<TRepo>(Expression<Action<TRepo, TModel, TState, TInput>> notification) where TRepo : IUINotificationRepository;

        IPromiseBuilder<TInput, TModel, TState> ShowAlert<TRepo>(Expression<Action<TRepo>> alert) where TRepo : IUIAlertRepository;
        IPromiseBuilder<TInput, TModel, TState> ShowAlert<TRepo>(Expression<Action<TRepo, TModel>> alert) where TRepo : IUIAlertRepository;
        IPromiseBuilder<TInput, TModel, TState> ShowAlert<TRepo>(Expression<Action<TRepo, TModel, TState>> alert) where TRepo : IUIAlertRepository;
        IPromiseBuilder<TInput, TModel, TState> ShowAlert<TRepo>(Expression<Action<TRepo, TModel, TState, TInput>> alert) where TRepo : IUIAlertRepository;
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IWhenBehaviorBuilder<TInput, TModel, TState>
    {
        IWhenBehaviorBuilder<TInput, TModel, TState> When(
            Expression<Func<TModel, TState, TInput, bool>> conition,
            Expression<Action<IBehaviorBuilder<TModel, TState, TInput>>> onTrue);
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IWhenOtherwieBehaviorBuilder<TInput, TModel, TState> : IWhenBehaviorBuilder<TInput, TModel, TState>
    {
        void Otherwise(Expression<Action<IBehaviorBuilder<TModel, TState, TInput>>> onOtherwise);
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IApiCallBehaviorBuilder<TInput, TModel, TState, TContract>
    {
        IPromiseBuilder<TInput, TModel, TState> OneWay(Expression<Action<TContract>> call);
        IPromiseBuilder<TInput, TModel, TState> OneWay(Expression<Action<TContract, TModel>> call);
        IPromiseBuilder<TInput, TModel, TState> OneWay(Expression<Action<TContract, TModel, TState>> call);
        IPromiseBuilder<TInput, TModel, TState> OneWay(Expression<Action<TContract, TModel, TState, TInput>> call);

        IPromiseBuilder<TInput, TModel, TState> RequestReply(Expression<Action<TContract>> call);
        IPromiseBuilder<TInput, TModel, TState> RequestReply(Expression<Action<TContract, TModel>> call);
        IPromiseBuilder<TInput, TModel, TState> RequestReply(Expression<Action<TContract, TModel, TState>> call);
        IPromiseBuilder<TInput, TModel, TState> RequestReply(Expression<Action<TContract, TModel, TState, TInput>> call);

        IPromiseBuilder<TReply, TModel, TState> RequestReply<TReply>(Expression<Func<TContract, TReply>> call);
        IPromiseBuilder<TReply, TModel, TState> RequestReply<TReply>(Expression<Func<TContract, TModel, TReply>> call);
        IPromiseBuilder<TReply, TModel, TState> RequestReply<TReply>(Expression<Func<TContract, TModel, TState, TReply>> call);
        IPromiseBuilder<TReply, TModel, TState> RequestReply<TReply>(Expression<Func<TContract, TModel, TState, TInput, TReply>> call);
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface INavigationBehaviorBuilder<TInput, TModel, TState>
    {
        IPromiseBuilder<TInput, TModel, TState> LoadScreen<TApp>(Expression<Func<TApp, IScreen>> screenSelector) where TApp : IApplication;
        INavigationViewContainerSelector<TInput, TModel, TState> LoadView<TView>() where TView : IView;
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface INavigationViewContainerSelector<TInput, TModel, TState>
    {
        IPromiseBuilder<TInput, TModel, TState> Into<TApp>(Expression<Func<TApp, IView>> containerSelector) where TApp : IApplication;
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IPromiseBuilder<TOutput, TModel, TState>
    {
        void Then(
            Expression<Action<IBehaviorBuilder<TOutput, TModel, TState>>> onSuccess,
            Expression<Action<IBehaviorBuilder<TOutput, TModel, TState>>> onError = null);
    }
}
