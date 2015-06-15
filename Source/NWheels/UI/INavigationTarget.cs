using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.UI
{
    /// <summary>
    /// Distinguishes Screen and Screen Part, which can be navigated to.
    /// </summary>
    /// <typeparam name="TInputParam">
    /// Type of input parameter model. Use IEmptyInputParam if there is no input parameter. 
    /// </typeparam>
    public interface INavigationTarget<out TInputParam>
    {
        INotification<TInputParam> NavigatedHere { get; }
    }
}
