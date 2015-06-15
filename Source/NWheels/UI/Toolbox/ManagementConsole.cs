using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IScreenPart Dashboard { get; set; }
    }
}
