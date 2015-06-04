using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.UI.Core
{
    public abstract class CommandBase : UIElementBase, ICommand
    {
        #region Implementation of ICommand

        public string Icon { get; set; }
        public string Text { get; set; }

        #endregion
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class HardCoded_ICommand : CommandBase
    {
    }
}
