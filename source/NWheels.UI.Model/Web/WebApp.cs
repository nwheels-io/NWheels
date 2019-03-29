namespace NWheels.UI.Model.Web
{
    public interface IWebApp
    {
    }
    
    public abstract class WebApp<TProps, TState> : UIComponent<TProps, TState>, IWebApp
    {
        protected WebApp(TProps props)
        {
        }
    }

    public class SinglePageWebApp<TPage> : WebApp<Empty.Props, Empty.State>
        where TPage : class, IWebPage, new()
    {
        public SinglePageWebApp() 
            : base(new Empty.Props())
        {
        }

        public TPage Index;
    }
}
