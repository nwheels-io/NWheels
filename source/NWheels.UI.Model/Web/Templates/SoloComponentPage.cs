namespace NWheels.UI.Model.Web.Templates
{
    public abstract class SoloComponentPage<TProps, TState> : WebPage<TProps, TState>
    {
        public abstract UIComponent SoloComponent { get; }        
    }

    public abstract class SoloComponentPage : SoloComponentPage<Empty.Props, Empty.State>
    {
    }
}
