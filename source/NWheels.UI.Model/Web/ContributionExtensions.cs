using NWheels.Domain.Model;
using NWheels.Composition.Model;

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
        [ValueContract.Required]
        public string BaseUrl = null;
    }
}
