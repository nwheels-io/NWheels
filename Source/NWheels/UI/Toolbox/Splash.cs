using System.Collections.Generic;
using NWheels.UI.Uidl;

namespace NWheels.UI.Toolbox
{
    public class Splash : WidgetBase<Splash, Empty.Data, Empty.State>
    {
        public Splash(string idName, ControlledUidlNode parent)
            : base(idName, parent)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override IEnumerable<WidgetUidlNode> GetNestedWidgets()
        {
            yield return InsideContent;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void DescribePresenter(PresenterBuilder<Splash, Empty.Data, Empty.State> presenter)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public WidgetUidlNode InsideContent { get; set; }
    }
}
