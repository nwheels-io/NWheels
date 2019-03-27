namespace NWheels.UI.Model.Web
{
    public interface IWebApp
    {
    }
    
    public abstract class WebApp<TProps, TState> : UIComponent<TProps, TState>, IWebApp
    {
    }
}
