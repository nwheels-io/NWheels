using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.UI.Core;

namespace NWheels.UI
{
    public interface IScreenPart : IDescriptionProvider<ScreenPartDescription>
    {
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IScreenPartWithInput<TInput> : IScreenPart, IRootContentContainer, INavigationTarget<TInput>
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
    public interface IScreenPart<TScreenPart, TInput, TData, TState> : 
        IScreenPartWithInput<TInput>
        where TScreenPart : IScreenPart<TScreenPart, TInput, TData, TState>
        where TData : class
        where TState : class
    {
        /// <summary>
        /// Allows presenter describe itself through provided fluent API.
        /// </summary>
        /// <param name="builder">
        /// A builder object that exposes declarations to presenter.
        /// </param>
        void DescribePresenter(IScreenPartPresenter<TScreenPart, TInput, TData, TState> builder);
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// Encapsulates a set of widgets, which together form logically complete unit of interaction. 
    /// A Screen Part can declare contained Widgets and nested Screen Parts.
    /// The non-generic version of the interface is for the case when there is no input parameter.
    /// </summary>
    public interface IScreenPart<TScreenPart, TData, TState> :
        IScreenPart<TScreenPart, Empty.Input, TData, TState>
        where TScreenPart : IScreenPart<TScreenPart, Empty.Input, TData, TState>
        where TData : class
        where TState : class
    {
    }
}
