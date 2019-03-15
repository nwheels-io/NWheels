using NWheels.Domain.Model;
using NWheels.Package.Model;

namespace NWheels.UI.Model.Web
{
    public static class ContributionExtensions
    {
        public static void IncludeWebApp<TApp>(this IContributions packages, WebAppConfig config = null)
            where TApp : UIComponent
        {
        }
    }

    public class WebAppConfig
    {
        [Required]
        public string BaseUrl = null;
    }
}
