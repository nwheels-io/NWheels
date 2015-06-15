using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.UI.Core;

namespace NWheels.UI
{
    /// <summary>
    /// Base class for defining a Screen.
    /// </summary>
    /// <typeparam name="TInput">
    /// Type of navigation input parameter for the Screen Part.
    /// </typeparam>
    /// <typeparam name="TScreen">
    /// An interface that declares contained elements (widgets, commands, notifications, and alerts).
    /// This is usually a nested type in concrete Screen class.
    /// </typeparam>
    /// <typeparam name="TData">
    /// The Data part of the model.
    /// This is usually a nested type in a concrete Screen class.
    /// </typeparam>
    /// <typeparam name="TState">
    /// The State part of the model.
    /// This is usually a nested type in a concrete Screen class.
    /// </typeparam>
    public abstract class ScreenComponent<TScreen, TInput, TData, TState> :
        RootContentContainerComponent<TInput>,
        IScreen<TScreen, TInput, TData, TState>
        where TScreen : IScreen<TScreen, TInput, TData, TState>
        where TData : class
        where TState : class
    {
        /// <summary>
        /// Initializes widget properties, defines data bindings and behaviors.
        /// This is where the things are wired together.
        /// </summary>
        /// <param name="presenter">
        /// An interface which exposes fluent APIs for wiring the things together.
        /// </param>
        public abstract void DescribePresenter(IScreenPresenter<TScreen, TInput, TData, TState> presenter);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        ScreenDescription IDescriptionProvider<ScreenDescription>.GetDescription()
        {
            throw new NotImplementedException();
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public abstract class ScreenComponent<TScreen> : ScreenComponent<TScreen, Empty.Input, Empty.Data, Empty.State>
        where TScreen : IScreen<TScreen, Empty.Input, Empty.Data, Empty.State>
    {
    }
}
