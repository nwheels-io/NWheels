using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.UI.Core;

namespace NWheels.UI
{
    /// <summary>
    /// Defines UI application.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <typeparam name="TApp"></typeparam>
    /// <typeparam name="TData"></typeparam>
    /// <typeparam name="TState"></typeparam>
    /// <typeparam name="TInput"></typeparam>
    public abstract class ApplicationComponent<TApp, TInput, TData, TState> :
        NavigationTargetComponent<TInput>,
        IApplication<TApp, TInput, TData, TState>
        where TApp : IApplication<TApp, TInput, TData, TState>
        where TData : class
        where TState : class
    {
        /// <summary>
        /// Initializes application properties, defines data bindings and behaviors.
        /// This is where the things are wired together.
        /// </summary>
        /// <param name="presenter">
        /// An interface which exposes fluent APIs for wiring the things together.
        /// </param>
        public abstract void DescribePresenter(IApplicationPresenterBuilder<TApp, TInput, TData, TState> presenter);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        ApplicationDescription IDescriptionProvider<ApplicationDescription>.GetDescription()
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IScreen DefaultInitialScreen { get; set; }
        public INotification NavigationNotAuthorized { get; set; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public abstract class ApplicationComponent<TApp, TInput> : ApplicationComponent<TApp, TInput, Empty.Data, Empty.State>
        where TApp : IApplication<TApp, TInput, Empty.Data, Empty.State>
    {
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public abstract class ApplicationComponent<TApp> : ApplicationComponent<TApp, Empty.Input, Empty.Data, Empty.State>
        where TApp : IApplication<TApp, Empty.Input, Empty.Data, Empty.State>
    {
    }
}
