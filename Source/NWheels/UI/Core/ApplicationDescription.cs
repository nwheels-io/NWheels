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
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public List<ScreenDescription> Screens { get; private set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ScreenDescription InitialScreen { get; set; }
    }
}
