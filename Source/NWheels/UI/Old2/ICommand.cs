using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.UI
{
    /// <summary>
    /// Defines a Command, which encapsulates information required to execute an action upon a request by user. 
    /// </summary>
    public interface ICommand : IUIElement
    {
        /// <summary>
        /// Qualified name of the command, which uniquely identifies it.
        /// </summary>
        string QualifiedName { get; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Broadcast when the command is triggered to execute.
        /// QualifiedName is passed as notification payload.
        /// </summary>
        INotification<string> Executing { get; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Broadcast when the Enabled/Disabled state of the command needs to be updated.
        /// QualifiedName is passed as notification payload.
        /// </summary>
        INotification<string> Updating { get; }
    }
}
