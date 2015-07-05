using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.DataObjects.Core;
using NWheels.Testing.Entities.Stacks;
using IR1 = NWheels.Testing.Entities.Stacks.Interfaces.Repository1;

namespace NWheels.Stacks.MongoDb.ComponentTests
{
    public static class HardCodedImplementations
    {
        public class DataRepositoryObject_OnlineStoreRepository : IR1.IOnlineStoreRepository
        {

            public IR1.IOrderLine NewOrderLine(IR1.IOrder order, IR1.IProduct product, int quantity)
            {
                throw new NotImplementedException();
            }

            public Entities.IEntityRepository<IR1.IProduct> Products
            {
                get { throw new NotImplementedException(); }
            }

            public Entities.IEntityRepository<IR1.IOrder> Orders
            {
                get { throw new NotImplementedException(); }
            }

            public Type[] GetEntityTypesInRepository()
            {
                throw new NotImplementedException();
            }

            public void CommitChanges()
            {
                throw new NotImplementedException();
            }

            public void RollbackChanges()
            {
                throw new NotImplementedException();
            }

            public bool IsAutoCommitMode
            {
                get { throw new NotImplementedException(); }
            }

            public Entities.UnitOfWorkState UnitOfWorkState
            {
                get { throw new NotImplementedException(); }
            }

            public void Dispose()
            {
                throw new NotImplementedException();
            }

            public void InvokeGenericOperation(Type contractType, Entities.Core.IDataRepositoryCallback callback)
            {
                throw new NotImplementedException();
            }

            public Type[] GetEntityContractsInRepository()
            {
                throw new NotImplementedException();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class EntityObject_Order : Interfaces.Repository1.IOrder
        {
            private int m_Id;
            private ICollection<EntityObject_OrderLine> m_OrderLines = new HashSet<EntityObject_OrderLine>();
            private ConcreteToAbstractCollectionAdapter<EntityObject_OrderLine, Interfaces.Repository1.IOrderLine> m_OrderLines_Adapter;
            private DateTime m_PlacedAt;
            private Interfaces.Repository1.OrderStatus m_Status;

            public EntityObject_Order()
            {
                this.m_OrderLines_Adapter =
                    new ConcreteToAbstractCollectionAdapter<EntityObject_OrderLine, Interfaces.Repository1.IOrderLine>(this.m_OrderLines);
                this.m_Status = Interfaces.Repository1.OrderStatus.New;
            }

            public int Id
            {
                get { return this.m_Id; }
                set { this.m_Id = value; }
            }

            ICollection<Interfaces.Repository1.IOrderLine> Interfaces.Repository1.IOrder.OrderLines
            {
                get { return this.m_OrderLines_Adapter; }
            }

            public virtual ICollection<EntityObject_OrderLine> OrderLines
            {
                get { return this.m_OrderLines; }
                set
                {
                    this.m_OrderLines_Adapter = new ConcreteToAbstractCollectionAdapter<EntityObject_OrderLine, Interfaces.Repository1.IOrderLine>(value);
                    this.m_OrderLines = value;
                }
            }

            public DateTime PlacedAt
            {
                get { return this.m_PlacedAt; }
                set { this.m_PlacedAt = value; }
            }

            public Interfaces.Repository1.OrderStatus Status
            {
                get { return m_Status; }
                set { m_Status = value; }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class EntityObject_OrderLine : Interfaces.Repository1.IOrderLine
        {
            private int m_Id;
            private EntityObject_Order m_Order;
            private EntityObject_Product m_Product;
            private int m_Quantity;

            public int Id
            {
                get { return this.m_Id; }
                set { this.m_Id = value; }
            }

            Interfaces.Repository1.IOrder Interfaces.Repository1.IOrderLine.Order
            {
                get { return this.m_Order; }
                set { this.m_Order = (EntityObject_Order)value; }
            }

            Interfaces.Repository1.IProduct Interfaces.Repository1.IOrderLine.Product
            {
                get { return this.m_Product; }
                set
                {
                    this.m_Product = (EntityObject_Product)value;
                }
            }

            public virtual EntityObject_Order Order
            {
                get { return this.m_Order; }
                set { this.m_Order = value; }
            }

            public virtual EntityObject_Product Product
            {
                get { return this.m_Product; }
                set { this.m_Product = value; }
            }

            public int Quantity
            {
                get { return this.m_Quantity; }
                set { this.m_Quantity = value; }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class EntityObject_Product : Interfaces.Repository1.IProduct
        {
            private int m_Id;
            private string m_Name;
            private decimal m_Price;

            public int Id
            {
                get { return this.m_Id; }
                set { this.m_Id = value; }
            }

            public string Name
            {
                get { return this.m_Name; }
                set { this.m_Name = value; }
            }

            public decimal Price
            {
                get { return this.m_Price; }
                set { this.m_Price = value; }
            }
        }
    }
}
