using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using LinqPadODataV4Driver.Entities;
using Microsoft.OData.Client;

namespace LinqPadODataV4Driver
{
    public class OnlineStoreDataContext : ODataClientContextBase
    {
        private DataServiceQuery<Product> _products;
        private DataServiceQuery<Order> _orders;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public OnlineStoreDataContext(Uri serviceRoot, string localEntityTypesNamespace)
            : base(serviceRoot, "LinqPadODataV4Driver.Entities")
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public DataServiceQuery<Product> Product
        {
            get
            {
                if ( _products == null )
                {
                    _products = base.CreateQuery<Product>("Product");
                }

                return _products;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public DataServiceQuery<Order> Order
        {
            get
            {
                if ( _orders == null )
                {
                    _orders = base.CreateQuery<Order>("Order");
                }

                return _orders;
            }
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    namespace Entities
    {
        [Microsoft.OData.Client.Key("Id")]
        [Microsoft.OData.Client.OriginalNameAttribute("Product")]
        public class Product
        {
            private DataServiceCollection<OrderLine> _orderLines = new DataServiceCollection<OrderLine>(null, TrackingMode.None);

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public int Id { get; set; }
            public string Name { get; set; }
            public decimal Price { get; set; }
            public virtual DataServiceCollection<OrderLine> OrderLines
            {
                get { return _orderLines; }
                set { _orderLines = value; }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Microsoft.OData.Client.Key("Id")]
        [Microsoft.OData.Client.OriginalNameAttribute("Order")]
        public partial class Order
        {
            private DataServiceCollection<OrderLine> _orderLines = new DataServiceCollection<OrderLine>(null, TrackingMode.None);

            //-------------------------------------------------------------------------------------------------------------------------------------------------
            
            public int Id { get; set; }
            public System.DateTimeOffset DateTime { get; set; }
            public string CustomerEmail { get; set; }
            public virtual DataServiceCollection<OrderLine> OrderLines
            {
                get { return _orderLines; }
                set { _orderLines = value; }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Microsoft.OData.Client.Key("Id")]
        [Microsoft.OData.Client.OriginalNameAttribute("OrderLine")]
        public partial class OrderLine
        {
            public int Id { get; set; }
            public int Quantity { get; set; }
            public virtual Product Product { get; set; }
            public virtual Order Order { get; set; }
        }
    }
}
