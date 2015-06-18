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
            private ScreenPartDescription _dashboard;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public GeneratedDescription(string idName, UIContentElementDescription parent)
                : base(idName, parent)
            {
                base.ElementType = "ManagementConsole";
                this.MainContent = new ScreenPartContainer.GeneratedDescription("MainContent", this);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ScreenPartContainer.GeneratedDescription MainContent { get; private set; }
            public Menu.GeneratedDescription NavigationStructure { get; set; }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            [DuplicateReference]
            public ScreenPartDescription Dashboard
            {
                get
                {
                    return _dashboard;
                }
                set
                {
                    _dashboard = value;
                    MainContent.InitialScreenPartIdName = _dashboard.IdName;
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public string DashboardIdName
            {
                get { return Dashboard.IdName; }
            }
        }
    }
}
