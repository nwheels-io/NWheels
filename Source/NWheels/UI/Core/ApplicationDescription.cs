using System.Collections.Generic;

namespace NWheels.UI.Core
{
    public class ApplicationDescription : UIElementContainerDescription
    {
        public ApplicationDescription()
        {
            this.Screens = new List<ScreenDescription>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public List<ScreenDescription> Screens { get; private set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ScreenDescription InitialScreen { get; set; }
    }
}
