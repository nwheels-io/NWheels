using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.UI
{
    public interface IRootContentContainer : IUIElementContainer
    {
        IWidget ContentRoot { get; set; }
    }
}
