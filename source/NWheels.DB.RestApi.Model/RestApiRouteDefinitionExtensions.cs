using System;
using NWheels.DB.Model;
using NWheels.RestApi.Model;

namespace NWheels.DB.RestApi.Model
{
    public static class RestApiRouteDefinitionExtensions
    {
        public static CrudService<TEntity> CrudOverDB<TDB, TEntity>(
            this IApiRouteImplementation definition,
            Func<TDB, DBCollection<TEntity>> collection
        ) => default;

        public static CrudService<TEntity> GraphQLOverDB<TDB, TEntity>(
            this IApiRouteImplementation definition,
            Func<TDB, DBCollection<TEntity>> collection
        ) => default;
    }
}
