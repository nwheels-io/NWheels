using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.OData.Edm;
using NUnit.Framework;
using Simple.OData.Client;

namespace LinqPadODataV4Driver.Tests
{
    [TestFixture]
    public class CommonUseCaseTests
    {
        [Test]
        public async void CanReceiveMetadataFromServer()
        {
            var client = new ODataClient("http://localhost:9000/odata");
            var metadata = await client.GetMetadataAsync<IEdmModel>();

            var entityTypes = metadata.SchemaElements.OfType<IEdmEntityType>().ToArray();

            foreach (var type in entityTypes)
            {
                Console.WriteLine("{0}{{{1}}}", type.Name, string.Join(",", type.Properties().Select(p => p.Name).ToArray()));
            }
        }
    }
}
