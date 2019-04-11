using NWheels.Composition.Model;
using NWheels.Composition.Model.Impl;
using NWheels.Composition.Model.Impl.Metadata;
using NWheels.UI.Model.Impl.Parsers.Web;

namespace NWheels.UI.Model.Web
{
    public interface IWebApp : ICanInclude<IWebPage>
    {
    }
    
    [ModelParser(typeof(WebAppParser))]    
    public abstract class WebApp<TProps, TState> : UIComponent<TProps, TState>, IWebApp
    {
        protected WebApp(TProps props)
        {
        }
    }

    [ModelParser(typeof(SinglePageWebAppParser))]    
    public sealed class SinglePageWebApp<TPage> : WebApp<Empty.Props, Empty.State>
        where TPage : class, IWebPage, new()
    {
        public SinglePageWebApp() 
            : base(new Empty.Props())
        {
        }

        public TPage Index => new TPage();
    }
}
