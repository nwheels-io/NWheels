using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.UI.Core;

namespace NWheels.UI
{

    public interface IApplication : IDescriptionProvider<ApplicationDescription>
    {
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// Defines a UI application. This is UIDL root object.
    /// An Application declares its Screens and Screen Parts.
    /// </summary>
    public interface IApplication<TApp, TInput, TData, TState> : 
        IUIElement, 
        IUIElementContainer, 
        INavigationTarget<TInput>,
        IApplication
        where TApp : IApplication<TApp, TInput, TData, TState>
        where TData : class
        where TState : class
    {
        /// <summary>
        /// Allows presenter describe itself through provided fluent API.
        /// </summary>
        /// <param name="builder">
        /// A builder object that exposes declarations to presenter.
        /// </param>
        void DescribePresenter(IApplicationPresenterBuilder<TApp, TInput, TData, TState> builder);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// 
        /// </summary>
        IScreen DefaultInitialScreen { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// 
        /// </summary>
        INotification NavigationNotAuthorized { get; set; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IApplication<TApp> : IApplication<TApp, Empty.Input, Empty.Data, Empty.State>
        where TApp : IApplication<TApp, Empty.Input, Empty.Data, Empty.State>
    {
    }
}
