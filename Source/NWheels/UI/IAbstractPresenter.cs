using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using NWheels.UI.Core;

namespace NWheels.UI
{
    /// <summary>
    /// Allows a Presenter describe itself through provided declarations.
    /// </summary>
    /// <typeparam name="TContents">
    /// Type of contents the Presenter is compatible with.
    /// </typeparam>
    /// <typeparam name="TData">
    /// Type of Data model contract, as required by the Presenter.
    /// </typeparam>
    /// <typeparam name="TState">
    /// Type of State model contract, as required by the Presenter.
    /// </typeparam>
    public interface IAbstractPresenter<TContents, TData, TState>
        where TContents : IUIElementContainer
        where TData : class
        where TState : class
    {
        PresenterFluentApi.IBindingSourceBuilder<TContents, TData, TState, TValue> Bind<TValue>(
            Expression<Func<TContents, TValue>> destination);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        PresenterFluentApi.IBehaviorBuilder<TValue, TData, TState> OnChange<TValue>(
            Expression<Func<TContents, TValue>> property);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        PresenterFluentApi.IBehaviorBuilder<TPayload, TData, TState> On<TPayload>(
            INotification<TPayload> notification)
            where TPayload : class;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        PresenterFluentApi.IBehaviorBuilder<Empty.Payload, TData, TState> On(INotification notification);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        PresenterFluentApi.IBehaviorBuilder<Empty.Payload, TData, TState> On(ICommand command);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        TElement New<TElement>() where TElement : IUIElement;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        BehaviorDescription Behavior<TInput>(Action<PresenterFluentApi.IBehaviorBuilder<TInput, TData, TState>> describe);
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IWidgetPresenter<TWidget, TData, TState> : IAbstractPresenter<TWidget, TData, TState>
        where TWidget : IWidget<TWidget, TData, TState>
        where TData : class
        where TState : class
    {
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IAbstractNavigationTargetPresenter<TTarget, out TInput, TData, TState> : 
        IAbstractPresenter<TTarget, TData, TState>
        where TTarget : IUIElementContainer, INavigationTarget<TInput>
        where TData : class
        where TState : class
    {
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IScreenPartPresenter<TScreenPart, out TInput, TData, TState> :
        IAbstractNavigationTargetPresenter<TScreenPart, TInput, TData, TState>
        where TScreenPart : IScreenPart<TScreenPart, TInput, TData, TState>
        where TData : class
        where TState : class
    {
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IScreenPresenter<TScreen, out TInput, TData, TState> :
        IAbstractNavigationTargetPresenter<TScreen, TInput, TData, TState>
        where TScreen : IScreen<TScreen, TInput, TData, TState>
        where TData : class
        where TState : class
    {
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IApplicationPresenterBuilder<TApp, TInput, TData, TState> : IAbstractPresenter<TApp, TData, TState>
        where TApp : IApplication<TApp, TInput, TData, TState>
        where TData : class
        where TState : class
    {
    }
}
