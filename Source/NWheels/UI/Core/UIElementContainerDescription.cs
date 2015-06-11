using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.UI.Core
{
    public abstract class UIElementContainerDescription : UIElementDescription
    {
        protected UIElementContainerDescription()
        {
            this.Commands = new List<CommandDescription>();
            this.Notifications = new List<NotificationDescription>();
            this.Alerts = new List<AlertDescription>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public List<CommandDescription> Commands { get; private set; }
        public List<NotificationDescription> Notifications { get; private set; }
        public List<AlertDescription> Alerts { get; private set; }
    }
}
