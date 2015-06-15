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
    public interface IAbstractPresenterBuilder<TContents, TData, TState>
        where TContents : IUIElementContainer
        where TData : class
        where TState : class
    {
        PresenterFluentApi.IBindingSourceBuilder<TContents, TData, TState, TValue> Bind<TValue>(
            Expression<Func<TContents, TValue>> destination);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        PresenterFluentApi.IBehaviorBuilder<TPayload, TData, TState> On<TPayload>(
            INotification<TPayload> notification)
            where TPayload : class;
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IWidgetPresenterBuilder<TContents, TData, TState> : IAbstractPresenterBuilder<TContents, TData, TState>
        where TContents : IWidget
        where TData : class
        where TState : class
    {
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IAbstractNavigationTargetPresenterBuilder<TInputParam, TContents, TData, TState> : IAbstractPresenterBuilder<TContents, TData, TState>
        where TContents : IUIElementContainer, INavigationTarget<TInputParam>
        where TData : class
        where TState : class
    {
        void SetContentRoot(IWidget rootWidget);
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IScreenPartPresenterBuilder<TInputParam, TContents, TData, TState> : 
        IAbstractNavigationTargetPresenterBuilder<TInputParam, TContents, TData, TState>
        where TContents : IScreenPart<TInputParam>
        where TData : class
        where TState : class
    {
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IScreenPresenterBuilder<TInputParam, TContents, TData, TState> : 
        IAbstractNavigationTargetPresenterBuilder<TInputParam, TContents, TData, TState>
        where TContents : IScreen<TInputParam>
        where TData : class
        where TState : class
    {
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IApplicationPresenterBuilder<TInputParam, TContents, TData, TState> : IAbstractPresenterBuilder<TContents, TData, TState>
        where TContents : IApplication<TInputParam>
        where TData : class
        where TState : class
    {
        void SetInitialScreen(IScreen screen);
        void SetInitialScreen<TInputParam2>(
            IScreen<TInputParam2> screen, 
            Expression<Func<TContents, TData, TState, TInputParam, TInputParam2>> screenInputSelector);
    }
}
