using System;
using System.Collections.Generic;

namespace NWheels.UI.Core
{
    public abstract class NavigationTargetDescription : UIContentElementDescription
    {
        protected NavigationTargetDescription(string idName, NavigationTargetDescription parent)
            : base(idName, parent)
        {
            this.NavigatedHere = new NotificationDescription("NavigatedHere", this);
            base.Notifications.Add(NavigatedHere);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        internal NotificationDescription NavigatedHere { get; set; }
    }
}
