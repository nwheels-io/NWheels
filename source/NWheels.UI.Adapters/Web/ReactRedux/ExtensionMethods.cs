using NWheels.Composition.Model.Impl.Metadata;
using NWheels.UI.Adapters.Web.StaticHtml;
using NWheels.UI.Model.Web;

namespace NWheels.UI.Adapters.Web.ReactRedux
{
    public static class ExtensionMethods 
    {
        [TechnologyAdapter(typeof(ReactReduxTechnologyAdapter))]
        public static ReactReduxWebApp AsReactReduxWebApp(this IWebApp webApp)
        {
            return default(ReactReduxWebApp);
        }
    }
}
