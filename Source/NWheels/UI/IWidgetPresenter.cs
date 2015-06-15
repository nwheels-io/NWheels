using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.UI
{
    /// <summary>
    /// Defines a Presenter, which is responsible for configuring UI infrastructure with 
    /// declarations of data binding, data-driven styling, and handling of otherwise complex UI logic. 
    /// </summary>
    /// <typeparam name="TContents">
    /// Type of contents this Presenter is compatible with.
    /// </typeparam>
    /// <typeparam name="TData">
    /// Type of Data model contract, as required by this Presenter.
    /// </typeparam>
    /// <typeparam name="TState">
    /// Type of State model contract, as required by this Presenter.
    /// </typeparam>
    public interface IWidgetPresenter<TContents, TData, TState>
        where TContents : IWidget
        where TData : class
        where TState : class
    {
        /// <summary>
        /// Allows presenter describe itself through provided fluent API.
        /// </summary>
        /// <param name="builder">
        /// A builder object that exposes declarations to presenter.
        /// </param>
        void DescribePresenter(IWidgetPresenterBuilder<TContents, TData, TState> builder);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// The declared contents of the widget.
        /// </summary>
        TContents Contents { get; set; }
    }
}
