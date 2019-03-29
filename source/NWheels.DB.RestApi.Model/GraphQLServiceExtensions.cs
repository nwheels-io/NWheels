using System;
using NWheels.DB.Model;
using NWheels.RestApi.Model;

namespace NWheels.DB.RestApi.Model
{
    public static class GraphQLServiceExtensions
    {
        public static GraphQLApiRoute<TEntity> AsGraphQLApiRoute<TEntity>(this DBCollection<TEntity> collection)
        {
            return null;
        }
    }
}