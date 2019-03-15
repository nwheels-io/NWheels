using System;
using NWheels.DB.Model;
using NWheels.RestApi.Model;

namespace NWheels.DB.RestApi.Model
{
    public static class RestApiRouteDefinitionExtensions
    {
        public static IDbCrudRestApiRouteDefinition<TDB> CrudOverDB<TDB>(this IRestApiRouteImplementation definition) => default;
    }

    public interface IDbCrudRestApiRouteDefinition<TDB>
    {
        RestApiRoute<ICrudService<T>> Collection<T>(Func<TDB, DBCollection<T>> collection);
    }
}