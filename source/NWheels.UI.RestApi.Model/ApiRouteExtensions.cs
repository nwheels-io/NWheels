using System;
using NWheels.RestApi.Model;
using NWheels.UI.Model;

namespace NWheels.UI.RestApi.Model
{
    public static class ApiRouteExtensions
    {
        public static DataSource<TEntity> AsDataSource<TEntity>(this ApiRoute<CrudService<TEntity>> backend)
        {
            return null;
        }
        
        public static DataSource<TEntity> AsDataSource<TEntity>(this ApiRoute<GraphQLService<TEntity>> backend)
        {
            return null;
        }
    }
}
