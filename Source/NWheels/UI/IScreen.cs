using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.UI
{
    /// <summary>
    /// Defines a screen of an applicaiton. 
    /// Screen is the top-level encapsulation of UI elements. 
    /// User can only interact with one Screen at a time.
    /// A Screen can declare contained Widgets and nested Screen Parts.
    /// </summary>
    /// <typeparamref name="TInputParam">
    /// Type of screen input parameter model. Use IEmptyInputParam if there is no input parameter.
    /// </typeparamref>
    public interface IScreen<out TInputParam> : IUIElement, IUIElementContainer, INavigationTarget<TInputParam>
        where TInputParam : class
    {
    }
}
