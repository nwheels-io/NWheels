using System.Collections.Generic;

namespace NWheels.UI.Core
{
    public class ApplicationDescription : NavigationTargetDescription
    {
        public ApplicationDescription(string idName)
            : base(idName, parent: null)
        {
            base.NodeType = UINodeType.Application;
            this.Screens = new List<ScreenDescription>();
            this.ScreenParts = new List<ScreenPartDescription>();
            this.NavigationNotAuthorized = new NotificationDescription("NavigationNotAuthorized", this);
            base.Notifications.Add(this.NavigationNotAuthorized);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public List<ScreenDescription> Screens { get; private set; }
        public List<ScreenPartDescription> ScreenParts { get; private set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DuplicateReference]
        public ScreenDescription DefaultInitialScreen { get; set; }
        [DuplicateReference]
        public NotificationDescription NavigationNotAuthorized { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string DefaultInitialScreenIdName
        {
            get { return DefaultInitialScreen.IdName; }
        }
    }
}
