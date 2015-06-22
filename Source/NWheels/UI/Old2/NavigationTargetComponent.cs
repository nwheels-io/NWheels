using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.UI
{
    public abstract class NavigationTargetComponent<TInput> : 
        UIElementContainerComponent,
        INavigationTarget<TInput>
    {
        public INotification<TInput> NavigatedHere { get; set; }
    }
}
