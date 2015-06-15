using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.UI.Toolbox
{
    public class Splash : WidgetComponent<Splash, Empty.Data, Empty.State>
    {
        public override void DescribePresenter(IWidgetPresenter<Splash, Empty.Data, Empty.State> presenter)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IWidget Content { get; set; }
    }
}
