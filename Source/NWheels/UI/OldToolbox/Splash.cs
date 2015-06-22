using NWheels.UI.Core;

namespace NWheels.UI.OldToolbox
{
    public class Splash : WidgetComponent<Splash, Empty.Data, Empty.State>
    {
        public override void DescribePresenter(IWidgetPresenter<Splash, Empty.Data, Empty.State> presenter)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IWidget Content { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class GeneratedDescription : WidgetDescription
        {
            public GeneratedDescription(string idName, UIContentElementDescription parent)
                : base(idName, parent)
            {
                base.ElementType = "Splash";
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public WidgetDescription Content { get; set; }
        }
    }
}
