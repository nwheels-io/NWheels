using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using NWheels.UI.Uidl;

namespace NWheels.UI.Toolbox
{
    public class WidgetScreenPart<TWidget> : ScreenPartBase<WidgetScreenPart<TWidget>, Empty.Input, Empty.Data, Empty.State>
        where TWidget : WidgetUidlNode
    {
        public WidgetScreenPart(string idName, UidlApplication parent)
            : base(idName, parent)
        {
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void DescribePresenter(PresenterBuilder<WidgetScreenPart<TWidget>, Empty.Data, Empty.State> presenter)
        {
            ContentRoot = Widget;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public TWidget Widget { get; set; }
    }
}
