using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.UI.Core
{
    public abstract class UIElementDescription : IUIElement
    {
        public string IdName { get; protected set; }
        public string Text { get; set; }
        public string HelpText { get; set; }
        public string Icon { get; set; }
        public bool Enabled { get; set; }
        public bool Authorized { get; protected set; }
    }
}
