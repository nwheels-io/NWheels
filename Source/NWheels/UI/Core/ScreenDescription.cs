namespace NWheels.UI.Core
{
    public class ScreenDescription : RootContentContainerDescription
    {
        protected ScreenDescription(string idName, ApplicationDescription parent)
            : base(idName, parent)
        {
            base.NodeType = UINodeType.Screen;
        }
   }
}
