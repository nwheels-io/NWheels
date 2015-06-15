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
    /// <typeparam name="TInputParam">
    /// Type of navigation input parameter for the Screen Part.
    /// </typeparam>
    /// <typeparam name="TContents">
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
    public abstract class ScreenPartComponentBase<TInputParam, TContents, TData, TState> :
        IScreenPartPresenter<TInputParam, TContents, TData, TState>,
        IDescriptionProvider<ScreenPartDescription>
        where TContents : IScreenPart<TInputParam>
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
        public abstract void DescribePresenter(IScreenPartPresenterBuilder<TInputParam, TContents, TData, TState> presenter);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        ScreenPartDescription IDescriptionProvider<ScreenPartDescription>.GetDescription()
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TContents Contents { get; set; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public abstract class ScreenPartComponentBase<TContents, TData, TState> : ScreenPartComponentBase<Empty.InputParam, TContents, TData, TState>
        where TContents : IScreenPart
        where TData : class
        where TState : class
    {
    }
}
