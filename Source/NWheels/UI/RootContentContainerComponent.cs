using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.UI
{
    public abstract class RootContentContainerComponent<TInput> : NavigationTargetComponent<TInput>, IRootContentContainer
    {
        public IWidget ContentRoot { get; set; }
    }
}
