using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.UI.Core;

namespace NWheels.UI
{
    public interface IWidget : 
        IUIElement, 
        IUIElementContainer, 
        IDescriptionProvider<WidgetDescription>
    {
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// Defines a widget, which is a basic element of user interaction.
    /// A Widget can declare contained widgets, notifications, and commands.
    /// </summary>
    public interface IWidget<TWidget, TData, TState> : 
        IUIElement, 
        IUIElementContainer, 
        IWidget
        where TWidget : IWidget<TWidget, TData, TState>
        where TData : class 
        where TState : class
    {
        /// <summary>
        /// Allows presenter describe itself through provided fluent API.
        /// </summary>
        /// <param name="builder">
        /// A builder object that exposes declarations to presenter.
        /// </param>
        void DescribePresenter(IWidgetPresenter<TWidget, TData, TState> builder);
    }
}
