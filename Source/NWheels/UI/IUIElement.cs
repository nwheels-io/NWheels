using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.UI
{
    /// <summary>
    /// Encapsulates properties which are common to all UI elements.
    /// </summary>
    public interface IUIElement
    {
        /// <summary>
        /// Get or sets text associated with the element (usually caption).
        /// </summary>
        string Text { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Get or sets informative text explaining the element. Usually used for displaying a tooltip.
        /// </summary>
        string HelpText { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Get or sets an icon that visualizes the element. 
        /// The format and the meaning of the string depends on underlying UI platform.
        /// </summary>
        string Icon { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets or sets whether this UI element is enabled.
        /// </summary>
        bool Enabled { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Indicates whether current user has permissions to access this UI element.
        /// </summary>
        bool Authorized { get; }
    }
}
