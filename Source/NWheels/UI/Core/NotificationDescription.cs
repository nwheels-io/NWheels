using System.Collections.Generic;

namespace NWheels.UI.Core
{
    public class NotificationDescription : UINodeDescription
    {
        public NotificationDescription(string idName, UIElementDescription parent)
            : base(idName, parent)
        {
            base.NodeType = UINodeType.Notification;
            this.Subscribers = new List<BehaviorDescription>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public List<BehaviorDescription> Subscribers { get; private set; }
    }
}
