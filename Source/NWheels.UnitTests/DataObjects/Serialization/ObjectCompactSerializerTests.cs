using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using NUnit.Framework;
using NWheels.Testing;
using NWheels.TypeModel.Factories;
using NWheels.TypeModel.Serialization;
using Shouldly;
using Repo = NWheels.UnitTests.DataObjects.Serialization.TestObjectRepository;

namespace NWheels.UnitTests.DataObjects.Serialization
{
    [TestFixture]
    public class ObjectCompactSerializerTests : UnitTestBase
    {
        [Test]
        public void Serialize_SimpliestFlatObject()
        {
            //-- arrange

            var serializer = Framework.Components.Resolve<ObjectCompactSerializer>();

            var obj = new Repo.SimpliestFlat() {
                StringValue = "ABC",
                IntValue = 123,
                DecimalValue = 123.45m
            };

            //-- act

            var serializedBytes = serializer.WriteObject(typeof(Repo.SimpliestFlat), obj, new ObjectCompactSerializerDictionary());

            //-- assert

            serializedBytes.ShouldNotBeNull();
            serializedBytes.Length.ShouldBeGreaterThan(10);
        }
    }
}
