using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.UI
{
    public class CommandComponent : UIElementComponent
    {
        public CommandComponent(UIElementContainerComponent container)
        {
            
        }

        public string QualifiedName { get; private set; }
        public INotification<string> Executing { get; private set; }
        public INotification<string> Updating { get; private set; }
    }
}
