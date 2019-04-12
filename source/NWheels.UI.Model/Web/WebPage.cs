using NWheels.Composition.Model;
using NWheels.Composition.Model.Impl.Metadata;
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
        
        public abstract string Title { get; }

        public virtual ILayoutComponent PageLayout => null;

        public Event<Empty.Data> PageReady { get; set; } = null;
    }

    public abstract class WebPage : WebPage<Empty.Props, Empty.State>
    {
        public WebPage() : base(new Empty.Props())
        {
        }
    }
}
