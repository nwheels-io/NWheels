using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.UI.Toolbox
{
    public class ScreenPartContainer : WidgetComponent<ScreenPartContainer, Empty.Data, Empty.State>
    {
        public override void DescribePresenter(IWidgetPresenter<ScreenPartContainer, Empty.Data, Empty.State> presenter)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IScreenPart ContainedScreenPart { get; set; }
        public INotification ScreenPartLoaded { get; set; }
    }
}
