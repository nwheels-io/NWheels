using NWheels.Composition.Model.Impl.Metadata;
using NWheels.UI.Adapters.Web.StaticHtml;
using NWheels.UI.Model.Web;

namespace NWheels.UI.Adapters.Web.Wix
{
    public static class ExtensionMethods
    {
        [TechnologyAdapter(typeof(WixSiteTechnologyAdapter))]
        public static WixWebSite AsWixWebSite(this IWebApp webApp, string backendUrl)
        {
            return default(WixWebSite);
        }
    }
}
