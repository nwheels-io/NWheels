using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.UI.Core
{
    public abstract class UIElementBase : IUIElement
    {
        #region Implementation of IUIElement

        public string IdName { get; set; }
        public bool Enabled { get; set; }
        public bool HiddenWhenDisabled { get; set; }

        #endregion
    }
}
