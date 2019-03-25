using System;
using NWheels.DB.Model;
using NWheels.RestApi.Model;

namespace NWheels.DB.RestApi.Model
{
    public static class RestApiRouteDefinitionExtensions
    {
        public static RestApiRoute<ICrudService<TEntity>> CrudOverDB<TDB, TEntity>(
            this IRestApiRouteImplementation definition,
            Func<TDB, DBCollection<TEntity>> collection
        ) => default;

        public static RestApiRoute<ICrudService<TEntity>> GraphQLOverDB<TDB, TEntity>(
            this IRestApiRouteImplementation definition,
            Func<TDB, DBCollection<TEntity>> collection
        ) => default;
    }
}
