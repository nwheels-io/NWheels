using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.UI.Core;

namespace NWheels.UI
{
    /// <summary>
    /// Base class for defining a Screen Part.
    /// </summary>
    /// <typeparam name="TInput">
    /// Type of navigation input parameter for the Screen Part.
    /// </typeparam>
    /// <typeparam name="TScreenPart">
    /// An interface that declares contained elements (widgets, commands, notifications, and alerts).
    /// This is usually a nested type in concrete Screen Part class.
    /// </typeparam>
    /// <typeparam name="TData">
    /// The Data part of the model.
    /// This is usually a nested type in a concrete Screen Part class.
    /// </typeparam>
    /// <typeparam name="TState">
    /// The State part of the model.
    /// This is usually a nested type in a concrete Screen Part class.
    /// </typeparam>
    public abstract class ScreenPartComponent<TScreenPart, TInput, TData, TState> :
        RootContentContainerComponent<TInput>,
        IScreenPart<TScreenPart, TInput, TData, TState>
        where TScreenPart : IScreenPart<TScreenPart, TInput, TData, TState>
        where TData : class
        where TState : class
    {
        /// <summary>
        /// Initializes Scraan Part properties, defines data bindings and behaviors.
        /// This is where the things are wired together.
        /// </summary>
        /// <param name="presenter">
        /// An interface which exposes fluent APIs for wiring the things together.
        /// </param>
        public abstract void DescribePresenter(IScreenPartPresenter<TScreenPart, TInput, TData, TState> presenter);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        ScreenPartDescription IDescriptionProvider<ScreenPartDescription>.GetDescription()
        {
            throw new NotImplementedException();
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public abstract class ScreenPartComponent<TScreenPart, TData, TState> : 
        ScreenPartComponent<TScreenPart, Empty.Input, TData, TState>
        where TScreenPart : IScreenPart<TScreenPart, Empty.Input, TData, TState>
        where TData : class
        where TState : class
    {
    }
}
