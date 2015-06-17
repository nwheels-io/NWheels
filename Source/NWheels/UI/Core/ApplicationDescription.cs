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
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public List<ScreenDescription> Screens { get; private set; }
        public List<ScreenPartDescription> ScreenParts { get; private set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DuplicateReference]
        public ScreenDescription InitialScreen { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string InitialScreenIdName
        {
            get { return InitialScreen.IdName; }
        }
    }
}
