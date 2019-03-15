using NWheels.Package.Model;

namespace NWheels.RestApi.Model
{
    public static class ContributionExtensions
    {
        public static void IncludeApiRoutes<TModel>(this IContributions contributions, ApiRoutesConfig config = null)
            where TModel : RestApiModel
        {
        }
    }

    public class ApiRoutesConfig
    {
        public string BaseUrl = null;
    }
}
