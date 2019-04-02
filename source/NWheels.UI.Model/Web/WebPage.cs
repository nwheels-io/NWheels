using NWheels.Composition.Model.Metadata;

namespace NWheels.UI.Model.Web
{

    [ModelElement]
    public interface IWebPage
    {
    }
    
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
