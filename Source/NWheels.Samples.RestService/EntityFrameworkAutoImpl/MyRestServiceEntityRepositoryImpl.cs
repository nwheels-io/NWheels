using System;
using System.Linq;
using System.Web.OData.Builder;
using NWheels.Stacks.OdataOwinWebapi;

namespace NWheels.Samples.RestService.EntityFrameworkAutoImpl
{
    public class MyRestServiceEntityRepositoryImpl : IMyRestServiceEntityRepository
    {
        private readonly OnlineStoreModelContainer _dbContext;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MyRestServiceEntityRepositoryImpl(OnlineStoreModelContainer dbContext)
        {
            _dbContext = dbContext;
        }

        ////-----------------------------------------------------------------------------------------------------------------------------------------------------

        //public override Microsoft.OData.Edm.IEdmModel GetEdmModel()
        //{
        //    var model = new ODataConventionModelBuilder();
        //    model.EntitySet<Product>("Product");
        //    model.EntitySet<Order>("Order");
        //    return model.GetEdmModel();
        //}

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public System.Type[] GetEntityTypesInRepository()
        {
            return new Type[] {
                typeof(Product), 
                typeof(Order),
                typeof(OrderLine)
            };
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IQueryable<Product> Products
        {
            get { return _dbContext.Products; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IQueryable<Order> Orders
        {
            get { return _dbContext.Orders; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IQueryable<OrderLine> OrderLines
        {
            get { return _dbContext.OrderLines; }
        }


        void Entities.IUnitOfWork.CommitChanges()
        {
            throw new NotImplementedException();
        }

        void Entities.IUnitOfWork.RollbackChanges()
        {
            throw new NotImplementedException();
        }

        bool Entities.IUnitOfWork.IsAutoCommitMode
        {
            get { throw new NotImplementedException(); }
        }

        Entities.UnitOfWorkState Entities.IUnitOfWork.UnitOfWorkState
        {
            get { throw new NotImplementedException(); }
        }

        void IDisposable.Dispose()
        {
        }

        public void InvokeGenericOperation(Type contractType, Entities.Core.IDataRepositoryCallback callback)
        {
            throw new NotImplementedException();
        }

        public Type[] GetEntityContractsInRepository()
        {
            throw new NotImplementedException();
        }


        public Entities.IEntityRepository[] GetEntityRepositories()
        {
            throw new NotImplementedException();
        }
    }
}
