using System;
using System.Collections.Generic;
using System.Data.Services.Design;
using System.Data.Metadata.Edm;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Microsoft.OData.Client;
using System.Net;
using System.Xml;
using System.IO;
using Microsoft.OData.Edm.Csdl;
using Microsoft.OData.Edm;
using System.Xml.Linq;

namespace LinqPadODataV4Driver.Tests
{
    [TestFixture]
    public class CommonUseCaseTests
    {
        [Test]
        public void CanConnectAndDownloadMetadata()
        {
            var context = new DataServiceContext(new Uri("http://localhost:9000/entity"), ODataProtocolVersion.V4);
            string metadataString;
            IEdmModel model;

            using ( var http = new WebClient() )
            {
                metadataString = http.DownloadString(context.GetMetadataUri());
            }

            using ( var xmlReader = XmlReader.Create(new StringReader(metadataString)) )
            {
                model = EdmxReader.Parse(xmlReader);
            }

            var entityTypes = model.SchemaElements.OfType<IEdmEntityType>().ToArray();

            foreach ( var type in entityTypes )
            {
                Console.WriteLine("{0}{{{1}}}", type.Name, string.Join(",", type.Properties().Select(p => p.Name).ToArray()));
            }
        }


        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanUseDataServiceContextToQueryData()
        {
            var context = new OnlineStoreDataContext(new Uri("http://localhost:9000/entity"));
            context.SendingRequest2 += (sender, e) => Console.WriteLine(e.RequestMessage.Url);

            var allProducts = context.Products.ToArray();

            foreach ( var product in allProducts )
            {
                Console.WriteLine("Product ID={0}, Name={1}, Price={2}", product.Id, product.Name, product.Price);
            }
        }
    }
}
