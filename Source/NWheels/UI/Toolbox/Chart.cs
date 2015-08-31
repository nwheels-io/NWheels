using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.UI.Uidl;

namespace NWheels.UI.Toolbox
{
    public class Chart : WidgetBase<Chart, Empty.Data, Empty.State>
    {
        public Chart(string idName, ControlledUidlNode parent)
            : base(idName, parent)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void DescribePresenter(PresenterBuilder<Chart, Empty.Data, Empty.State> presenter)
        {
        }
    }
}
