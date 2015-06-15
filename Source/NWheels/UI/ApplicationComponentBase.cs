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
    /// <typeparam name="TContents"></typeparam>
    /// <typeparam name="TData"></typeparam>
    /// <typeparam name="TState"></typeparam>
    /// <typeparam name="TInputParam"></typeparam>
    public abstract class ApplicationComponentBase<TInputParam, TContents, TData, TState> :
        IApplicationPresenter<TInputParam, TContents, TData, TState>,
        IDescriptionProvider<ApplicationDescription>
        where TContents : IApplication<TInputParam>
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
        public abstract void DescribePresenter(IApplicationPresenterBuilder<TInputParam, TContents, TData, TState> presenter);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        ApplicationDescription IDescriptionProvider<ApplicationDescription>.GetDescription()
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TContents Contents { get; set; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public abstract class ApplicationComponentBase<TInputParam, TContents> : ApplicationComponentBase<TInputParam, TContents, Empty.Data, Empty.State>
        where TContents : IApplication<TInputParam>
    {
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public abstract class ApplicationComponentBase<TContents> : ApplicationComponentBase<Empty.InputParam, TContents, Empty.Data, Empty.State>
        where TContents : IApplication
    {
    }
}
