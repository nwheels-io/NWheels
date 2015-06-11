using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.UI
{
    /// <summary>
    /// Represents a Model binding source.
    /// There is no runtime implementation of this interface. It is used solely to let compiler generate binding expressions.
    /// </summary>
    /// <typeparam name="TData">
    /// The type of the Data contract in the model.
    /// </typeparam>
    /// <typeparam name="TState">
    /// The type of the State contract in the model.
    /// </typeparam>
    public interface IViewModel<out TData, out TState>
        where TData : class 
        where TState : class
    {
        /// <summary>
        /// The Data part of the model
        /// </summary>
        TData Data { get; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// The State part of the model
        /// </summary>
        TState State { get; }
    }
}
