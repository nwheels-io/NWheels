using System.Collections.Generic;

namespace NWheels.UI.Core
{
    public class CommandDescription : UIElementDescription
    {
        public CommandDescription(string idName, UIContentElementDescription parent)
            : base(idName, parent)
        {
            base.NodeType = UINodeType.Command;

            this.Executing = new NotificationDescription("Executing", this);
            this.Updating = new NotificationDescription("Updating", this);
            
            base.Notifications.Add(Executing);
            base.Notifications.Add(Updating);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public NotificationDescription Executing { get; set; }
        public NotificationDescription Updating { get; set; }
    }
}
