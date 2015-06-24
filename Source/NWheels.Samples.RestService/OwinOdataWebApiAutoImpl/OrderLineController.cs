using System.Linq;
using System.Web.Http;
using System.Web.OData.Routing;
using NWheels.Stacks.OdataOwinWebapi;

namespace NWheels.Samples.RestService.OwinOdataWebApiAutoImpl
{
    [ODataRoutePrefix("OrderLine")]
    public class OrderLineController : EntityControllerBase<OrderLine>
    {
        private readonly IMyRestServiceEntityRepository _repository;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public OrderLineController(IMyRestServiceEntityRepository repository)
        {
            _repository = repository;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        //[Queryable(MaxExpansionDepth = 5)]
        public override IQueryable<OrderLine> Get()
        {
            return _repository.OrderLines;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        //[Queryable(MaxExpansionDepth = 5)]
        public OrderLine Get(int key)
        {
            return _repository.OrderLines.Single(o => o.Id == key);
        }
    }
}
