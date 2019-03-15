using System;
using NWheels.DB.Model;
using NWheels.RestApi.Model;

namespace NWheels.DB.RestApi.Model
{
    public static class RestApiRouteConfigExtensions
    {
        public static RestApiRouteConfig<ICrudService<T>> DBCrudService<TDB, T>(
            this RestApiRouteConfig<ICrudService<T>> config,
            Func<TDB, DBCollection<T>> collection) 
            => default;
    }
}
