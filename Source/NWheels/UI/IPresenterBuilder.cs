using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.UI
{
    /// <summary>
    /// Allows a Presenter describe itself through provided declarations.
    /// </summary>
    /// <typeparam name="TView">
    /// Type of widget contract the Presenter is compatible with.
    /// </typeparam>
    /// <typeparam name="TData">
    /// Type of Data model contract, as required by the Presenter.
    /// </typeparam>
    /// <typeparam name="TState">
    /// Type of State model contract, as required by the Presenter.
    /// </typeparam>
    public interface IPresenterBuilder<TView, TData, TState>
        where TView : IUIElementContainer
        where TData : class
        where TState : class
    {
        PresenterFluentApi.IBindingSourceBuilder<TView, TData, TState, TValue> Bind<TValue>(
            Expression<Func<TView, TValue>> destination);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        PresenterFluentApi.IBehaviorBuilder<TPayload, TData, TState> On<TPayload>(
            INotification<TPayload> notification)
            where TPayload : class;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        TView View { get; }
    }
}
