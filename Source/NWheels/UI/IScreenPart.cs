using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.UI
{
    /// <summary>
    /// Encapsulates a set of widgets, which together form logically complete unit of interaction. 
    /// A Screen Part can declare contained Widgets and nested Screen Parts.
    /// </summary>
    /// <typeparamref name="TInputParam">
    /// Type of input parameter model. Use IEmptyInputParam if there is no input parameter.
    /// </typeparamref>
    public interface IScreenPart<out TInputParam> : IUIElement, IUIElementContainer, INavigationTarget<TInputParam>
        where TInputParam : class
    {
    }
}
