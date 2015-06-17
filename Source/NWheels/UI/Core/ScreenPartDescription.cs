namespace NWheels.UI.Core
{
    public class ScreenPartDescription : RootContentContainerDescription
    {
        protected ScreenPartDescription(string idName, NavigationTargetDescription parent)
            : base(idName, parent)
        {
            base.NodeType = UINodeType.ScreenPart;
        }
    }
}
