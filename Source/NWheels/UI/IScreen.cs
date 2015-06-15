using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.UI.Core;

namespace NWheels.UI
{
    /// <summary>
    /// Defines a screen of an applicaiton. 
    /// Screen is the top-level encapsulation of UI elements. 
    /// User can only interact with one Screen at a time.
    /// A Screen can declare contained Widgets and nested Screen Parts.
    /// </summary>
    /// <typeparamref name="TInputParam">
    /// Type of screen input parameter model.
    /// </typeparamref>
    public interface IScreen<out TInputParam> : IUIElement, IUIElementContainer, IRootContentContainer<TInputParam>, IDescriptionProvider<ScreenDescription>
    {
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// Defines a screen of an applicaiton. 
    /// Screen is the top-level encapsulation of UI elements. 
    /// User can only interact with one Screen at a time.
    /// A Screen can declare contained Widgets and nested Screen Parts.
    /// The non-generic version of the interface is for the case when there is no input parameter.
    /// </summary>
    public interface IScreen : IScreen<Empty.InputParam>
    {
    }
}
