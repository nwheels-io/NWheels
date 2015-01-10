using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.UI
{
    public interface IUiScope<out TModel, out TState>
    {
        TModel Model { get; }
        TState UiState { get; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IUiScope<out TModel, out TState, out TInput> : IUiScope<TModel, TState>
    {
        TInput Input { get; }
    }
}
