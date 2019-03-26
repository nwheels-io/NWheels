using System;
using NWheels.RestApi.Model;
using NWheels.UI.Model;

namespace NWheels.UI.RestApi.Model
{
    public static class DataSourceFactoryExtensions
    {
        public static IBackendRestApiSyntax<TModel> BackendRestApi<TModel>(this IDataSourceFactory factory) => default;

        public interface IBackendRestApiSyntax<TModel>
        {
            IDataSource<TEntity> Route<TEntity>(Func<TModel, ICrudService<TEntity>> routeSelector);
        }
    }
}
