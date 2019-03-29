namespace NWheels.UI.Model.Web
{
    public interface IWebApp
    {
    }
    
    public abstract class WebApp<TProps, TState> : UIComponent<TProps, TState>, IWebApp
    {
    }

    public class SinglePageWebApp<TPage> : WebApp<Empty.Props, Empty.State>
        where TPage : IWebPage
    {
        public TPage Index;
    }
}
