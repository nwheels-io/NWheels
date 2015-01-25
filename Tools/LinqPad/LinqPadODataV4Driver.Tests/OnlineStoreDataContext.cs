using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using LinqPadODataV4Driver.Tests.Entities;
using Microsoft.OData.Client;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Csdl;

namespace LinqPadODataV4Driver.Tests
{
    public abstract class DynamicDataServiceContextBase : DataServiceContext
    {
        private readonly string _localEntityTypesNamespace;
        private string _remoteEntityTypesNamespace;
        private IEdmModel _edmModel;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public DynamicDataServiceContextBase(Uri serviceRoot, string localEntityTypesNamespace) : 
            base(serviceRoot, ODataProtocolVersion.V4)
        {
            _localEntityTypesNamespace = localEntityTypesNamespace;

            this.ResolveName = ResolveNameFromType;
            this.ResolveType = ResolveTypeFromName;

            this.Format.LoadServiceModel = LoadServiceModel;
            this.Format.UseJson();
        }



        private Type ResolveTypeFromName(string typeName)
        {
            Type resolvedType = this.DefaultResolveType(typeName, _remoteEntityTypesNamespace, _localEntityTypesNamespace);

            if ( (resolvedType != null) )
            {
                return resolvedType;
            }

            return null;
        }

        private string ResolveNameFromType(Type clientType)
        {
            var originalNameAttribute = clientType.GetCustomAttributes(typeof(OriginalNameAttribute), true).OfType<OriginalNameAttribute>().SingleOrDefault();

            if ( clientType.Namespace == _localEntityTypesNamespace )
            {
                if ( originalNameAttribute != null )
                {
                    return string.Concat(_remoteEntityTypesNamespace + ".", originalNameAttribute.OriginalName);
                }
                return string.Concat(_remoteEntityTypesNamespace + ".", clientType.Name);
            }

            return null;
        }

        private IEdmModel LoadServiceModel()
        {
            using ( var http = new WebClient() )
            {
                var metadataString = http.DownloadString(GetMetadataUri());

                using ( var xmlReader = XmlReader.Create(new StringReader(metadataString)) )
                {
                    _edmModel = EdmxReader.Parse(xmlReader);
                }
            }

            _remoteEntityTypesNamespace = _edmModel.DeclaredNamespaces.FirstOrDefault(ns => ns != "Default"); //"NWheels.Samples.RestService"

            return _edmModel;
        }
    }

    public class OnlineStoreDataContext : DynamicDataServiceContextBase
    {
        private DataServiceQuery<Product> _products;
        private DataServiceQuery<Order> _orders;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public OnlineStoreDataContext(Uri serviceRoot)
            : base(serviceRoot, "LinqPadODataV4Driver.Tests.Entities")
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public DataServiceQuery<Product> Products
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

        public DataServiceQuery<Order> Orders
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
