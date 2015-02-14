using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using NWheels.Core.DataObjects;
using NWheels.DataObjects;

namespace NWheels.Core.UnitTests.DataObjects
{
    [TestFixture]
    public class TypeMetadataCacheTests
    {
        [Test]
        public void CanBuildMetadataOfFlatType()
        {
            var cache = new TypeMetadataCache();

            var metadata = cache.GetTypeMetadata(typeof(TestDataObjects.Repository1.IProduct));

            var metadataString = JsonlikeMetadataStringifier.Stringify(metadata);

            Console.WriteLine(metadataString);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanBuildMetadataOfInterconnectedTypes()
        {
            var cache = new TypeMetadataCache();

            var orderMetadata = cache.GetTypeMetadata(typeof(TestDataObjects.Repository1.IOrder));
            var productMetadata = cache.GetTypeMetadata(typeof(TestDataObjects.Repository1.IProduct));
            var orderLineMetadata = cache.GetTypeMetadata(typeof(TestDataObjects.Repository1.IOrderLine));

            var orderMetadataString = JsonlikeMetadataStringifier.Stringify(orderMetadata);
            var productMetadataString = JsonlikeMetadataStringifier.Stringify(productMetadata);
            var orderLineMetadataString = JsonlikeMetadataStringifier.Stringify(orderLineMetadata);

            Console.WriteLine(orderMetadataString);
            Console.WriteLine(productMetadataString);
            Console.WriteLine(orderLineMetadataString);
        }
    }
}
