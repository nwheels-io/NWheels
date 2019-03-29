using NWheels.RestApi.Model;

namespace NWheels.RestApi.Adapters.AspNetCore
{
    public static class AspNetCoreRestApiTechnologyAdapter
    {
        public static AspNetCoreRestApiServer AsAspNetCoreServer(this RestApiModel restApi)
        {
            return default(AspNetCoreRestApiServer);
        }
    }
}