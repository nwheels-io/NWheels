using System.Linq;
using System.Web.OData.Routing;
using NWheels.Puzzle.OdataOwinWebapi;

namespace NWheels.Samples.RestService.OwinOdataWebApiAutoImpl
{
    [ODataRoutePrefix("Order")]
    public class OrderController : EntityControllerBase<Order>
    {
        private readonly IMyRestServiceEntityRepository _repository;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public OrderController(IMyRestServiceEntityRepository repository)
        {
            _repository = repository;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override IQueryable<Order> Get()
        {
            return _repository.Orders;
        }
    }
}
