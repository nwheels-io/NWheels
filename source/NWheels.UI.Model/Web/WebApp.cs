using NWheels.Composition.Model;
using NWheels.Composition.Model.Metadata;
using NWheels.UI.Model.Impl.Parsers.Web;

namespace NWheels.UI.Model.Web
{
    [ModelParser(typeof(WebAppParser))]    
    public interface IWebApp : ICanInclude<IWebPage>
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

        [Include]
        public TPage Index => new TPage();
    }
}
