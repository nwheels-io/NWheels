using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.UI.Core;

namespace NWheels.UI
{
    /// <summary>
    /// Base class for defining a widget.
    /// </summary>
    /// <typeparam name="TWidget">
    /// An interface that declares widget properties and contained elements (widgets, commands, notifications, and alerts).
    /// This is usually a nested type in concrete widget class.
    /// </typeparam>
    /// <typeparam name="TData">
    /// The Data part of the model.
    /// This is usually a nested type in concrete widget class.
    /// </typeparam>
    /// <typeparam name="TState">
    /// The State part of the model.
    /// This is usually a nested type in concrete widget class.
    /// </typeparam>
    public abstract class WidgetComponent<TWidget, TData, TState> :
        UIElementContainerComponent,
        IWidget<TWidget, TData, TState>
        where TWidget : WidgetComponent<TWidget, TData, TState>
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
        public abstract void DescribePresenter(IWidgetPresenter<TWidget, TData, TState> presenter);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        WidgetDescription IDescriptionProvider<WidgetDescription>.GetDescription()
        {
            throw new NotImplementedException();
        }
    }
}
