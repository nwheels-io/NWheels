using System;
using NWheels.UI.Model;

namespace NWheels.UI.RestApi.Model
{
    public static class DataSourceFactoryExtensions
    {
        public static IDataSource<T> BackendRestApiCrud<T>(this IDataSourceFactory<T> factory) => default;
    }
}
