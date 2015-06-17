using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.UI.Core
{
    public class BehaviorDescription : UINodeDescription
    {
        public BehaviorDescription(string idName, NotificationDescription parent)
            : base(idName, parent)
        {
            base.NodeType = UINodeType.Behavior;
        }
    }
}
