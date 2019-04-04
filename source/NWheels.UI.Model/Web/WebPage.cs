using NWheels.Composition.Model.Metadata;
using NWheels.UI.Model.Impl.Parsers.Web;

namespace NWheels.UI.Model.Web
{
    public interface IWebPage
    {
    }
    
    [ModelParser(typeof(WebPageParser))]
    public abstract class WebPage<TProps, TState> : UIComponent<TProps, TState>, IWebPage
    {
        public WebPage(TProps props)
        {
        }
    }

    public abstract class WebPage : WebPage<Empty.Props, Empty.State>
    {
        public WebPage() : base(new Empty.Props())
        {
        }
    }
}
