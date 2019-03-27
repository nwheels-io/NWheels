using System;

namespace NWheels.RestApi.Model
{

    public static class RestApiRoute
    {
        public static IRestApiRouteImplementation Implementation => default;
    }

    public class RestApiRoute<TApi>
    {
        public RestApiRoute(Action<RestApiRouteConfig<TApi>> config)
        {
        }
    }

    public class RestApiRouteConfig<TApi>
    {
        public RestApiRouteConfig<TApi> WithBackend(IRestApiRouteBackend<TApi> backend) => default;
    }
    

    public interface IRestApiRouteImplementation
    {
        RestApiRoute<TApi> Route<TApi>(Action<RestApiRouteConfig<TApi>> config);
    }
}
