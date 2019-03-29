using NWheels.Composition.Model;

namespace NWheels.UI.Model.Web.Templates
{
    public abstract class SoloComponentPage<TProps, TState> : WebPage<TProps, TState>
    {
        protected SoloComponentPage(TProps props) : base(props)
        {
        }

        [Include]
        public abstract UIComponent SoloComponent { get; }        
    }

    public abstract class SoloComponentPage : SoloComponentPage<Empty.Props, Empty.State>
    {
        protected SoloComponentPage(Empty.Props props) : base(props)
        {
        }
    }
}
