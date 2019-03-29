using System;

namespace NWheels.RestApi.Model
{
    public abstract class AnyApiRoute
    {
    }

    public class ApiRoute<TApi>
    {
        public ApiRoute(Action<ApiRouteConfig<TApi>> config)
        {
        }
    }

    public class ApiRouteConfig<TApi>
    {
        public ApiRouteConfig<TApi> WithBackend(IRestApiRouteBackend<TApi> backend) => default;
    }
    

    public interface IApiRouteImplementation
    {
        ApiRoute<TApi> Route<TApi>(Action<ApiRouteConfig<TApi>> config);
    }
}
