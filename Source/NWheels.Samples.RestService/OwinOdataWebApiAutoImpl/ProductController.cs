using System.Linq;
using System.Web.OData.Routing;
using NWheels.Puzzle.OdataOwinWebapi;

namespace NWheels.Samples.RestService.OwinOdataWebApiAutoImpl
{
    [ODataRoutePrefix("Product")]
    public class ProductController : EntityControllerBase<Product>
    {
        private readonly IMyRestServiceEntityRepository _repository;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ProductController(IMyRestServiceEntityRepository repository)
        {
            _repository = repository;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override IQueryable<Product> Get()
        {
            return _repository.Products;
        }
    }
}
