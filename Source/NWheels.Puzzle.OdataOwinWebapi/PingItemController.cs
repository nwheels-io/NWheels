using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Builder;
using System.Web.OData.Routing;

namespace NWheels.Puzzle.OdataOwinWebapi
{
    [ODataRoutePrefix("PingItem")]
    public class PingItemController : ODataController
    {
        private static PingItemEntity[] s_PingItems = new[] {
            new PingItemEntity() { Name = "AAA", Value = "111" },
            new PingItemEntity() { Name = "BBB", Value = "222" },
            new PingItemEntity() { Name = "CCC", Value = "333" },
        };

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [EnableQuery()]
        [HttpGet]
        public IQueryable<PingItemEntity> Get()
        {
            return s_PingItems.AsQueryable();
        }
    }
}
