using System;

namespace NWheels.RestApi.Model
{
    public class GraphQLApiRoute<TEntity> : ApiRoute<GraphQLService<TEntity>>
    {
        public GraphQLApiRoute(Action<ApiRouteConfig<GraphQLService<TEntity>>> config = null) : base(config)
        {
        }
    }
}