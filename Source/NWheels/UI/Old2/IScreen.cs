using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.UI.Core;

namespace NWheels.UI
{
    public interface IScreen : IDescriptionProvider<ScreenDescription>
    {
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IScreenWithInput<TInput> : IScreen, IRootContentContainer, INavigationTarget<TInput>
    {
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// Encapsulates a set of widgets, which together form logically complete unit of interaction. 
    /// A Screen Part can declare contained Widgets and nested Screen Parts.
    /// </summary>
    /// <typeparamref name="TInput">
    /// Type of input parameter model. Use IEmptyInputParam if there is no input parameter.
    /// </typeparamref>
    public interface IScreen<TScreen, TInput, TData, TState> :
        IScreenWithInput<TInput>
        where TScreen : IScreen<TScreen, TInput, TData, TState>
        where TData : class
        where TState : class
    {
        /// <summary>
        /// Allows presenter describe itself through provided fluent API.
        /// </summary>
        /// <param name="builder">
        /// A builder object that exposes declarations to presenter.
        /// </param>
        void DescribePresenter(IScreenPresenter<TScreen, TInput, TData, TState> builder);
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// Encapsulates a set of widgets, which together form logically complete unit of interaction. 
    /// A Screen Part can declare contained Widgets and nested Screen Parts.
    /// The non-generic version of the interface is for the case when there is no input parameter.
    /// </summary>
    public interface IScreen<TScreen> :
        IScreen<TScreen, Empty.Input, Empty.Data, Empty.State>
        where TScreen : IScreen<TScreen, Empty.Input, Empty.Data, Empty.State>
    {
    }
}
