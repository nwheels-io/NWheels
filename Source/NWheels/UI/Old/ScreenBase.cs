using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.UI.Core
{
    public abstract class ScreenBase : UIElementBase, IScreen
    {
        #region Implementation of IScreen

        public string Icon { get; set; }
        public string Title { get; set; }
        public string SubTitle { get; set; }

        #endregion

        public string ViewName { get; protected set; }
    }
}
