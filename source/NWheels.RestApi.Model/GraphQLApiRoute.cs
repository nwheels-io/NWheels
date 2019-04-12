using System;
using System.Collections.Generic;

namespace NWheels.RestApi.Model
{
    public class GraphQLApiRoute<TEntity> : ApiRoute<GraphQLService<TEntity>>
    {
        public GraphQLApiRoute(Action<ApiRouteConfig<GraphQLService<TEntity>>> config = null) : base(config)
        {
        }

        public IEnumerable<TEntity> Data => null;
    }
}
