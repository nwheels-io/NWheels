using System.Linq;
using System.Web.Http;
using System.Web.OData.Routing;
using NWheels.Stacks.OdataOwinWebapi;

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

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Order Get(int key)
        {
            return _repository.Orders.Where(o => o.Id == key).Single();
        }
    }
}
