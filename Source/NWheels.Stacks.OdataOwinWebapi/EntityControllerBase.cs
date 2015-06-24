using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;

namespace NWheels.Stacks.OdataOwinWebapi
{
    public abstract class EntityControllerBase<TEntity> : ODataController
    {
        [EnableQuery]
        [HttpGet]
        public abstract IQueryable<TEntity> Get();
    }
}
