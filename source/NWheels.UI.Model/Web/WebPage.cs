namespace NWheels.UI.Model.Web
{

    public interface IWebPage
    {
    }
    
    public abstract class WebPage<TProps, TState> : UIComponent<TProps, TState>, IWebPage
    {
    }

    public abstract class WebPage : WebPage<Empty.Props, Empty.State>
    {
    }
}
