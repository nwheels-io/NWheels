using NWheels.Composition.Model.Impl.Metadata;
using NWheels.UI.Model.Web;

namespace NWheels.UI.Adapters.Web.StaticHtml
{
    public static class ExtensionMethods
    {
        [TechnologyAdapter(typeof(StaticHtmlTechnologyAdapter))]
        public static StaticHtmlWebSite AsStaticHtmlWebSite(this IWebApp webApp)
        {
            return default(StaticHtmlWebSite);
        }
    }
}
