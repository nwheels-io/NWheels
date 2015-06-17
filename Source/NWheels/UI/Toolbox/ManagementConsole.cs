using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.UI.Core;

namespace NWheels.UI.Toolbox
{
    public class ManagementConsole : WidgetComponent<ManagementConsole, Empty.Data, Empty.State>
    {
        public override void DescribePresenter(IWidgetPresenter<ManagementConsole, Empty.Data, Empty.State> presenter)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void DefineNavigation(object anonymous)
        {
            this.NavigationStructure = anonymous;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IScreenPart Dashboard { get; set; }
        public object NavigationStructure { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class GeneratedDescription : WidgetDescription
        {
            public GeneratedDescription(string idName, UIContentElementDescription parent)
                : base(idName, parent)
            {
                base.ElementType = "ManagementConsole";
                this.MainContent = new ScreenPartContainer.GeneratedDescription("MainContent", this);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ScreenPartContainer.GeneratedDescription MainContent { get; private set; }
            public ScreenPartDescription Dashboard { get; set; }
            public Menu.GeneratedDescription NavigationStructure { get; set; }
        }
    }
}
