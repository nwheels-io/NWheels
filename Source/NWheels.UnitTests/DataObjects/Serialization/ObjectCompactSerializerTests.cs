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

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void Roundtrip_SimpliestFlatObject()
        {
            //-- arrange

            var serializer = Framework.Components.Resolve<ObjectCompactSerializer>();
            var dictionary = new ObjectCompactSerializerDictionary();

            var guid = Guid.NewGuid();
            var original = new Repo.SimpliestFlat() {
                IntValue = 123,
                StringValue = "ABC",
                SystemEnumValue = DayOfWeek.Wednesday,
                AppEnumValue = Repo.AnAppEnum.Second,
                TimeSpanValue = TimeSpan.FromSeconds(123),
                DateTimeValue = new DateTime(2016, 05, 18, 13, 0, 3),
                GuidValue = guid,
                LongValue = Int64.MaxValue - 123,
                FloatValue = 123.45f,
                DecimalValue = 123.45m
            };

            //-- act

            var serializedBytes = serializer.WriteObject(typeof(Repo.SimpliestFlat), original, dictionary);
            var deserialized = (Repo.SimpliestFlat)serializer.ReadObject(typeof(Repo.SimpliestFlat), serializedBytes, dictionary);

            //-- assert

            deserialized.ShouldNotBeNull();
            deserialized.IntValue.ShouldBe(123);
            deserialized.StringValue.ShouldBe("ABC");
            deserialized.SystemEnumValue.ShouldBe(DayOfWeek.Wednesday);
            deserialized.AppEnumValue.ShouldBe(Repo.AnAppEnum.Second);
            deserialized.TimeSpanValue.ShouldBe(TimeSpan.FromSeconds(123));
            deserialized.DateTimeValue.ShouldBe(new DateTime(2016, 05, 18, 13, 0, 3));
            deserialized.GuidValue.ShouldBe(guid);
            deserialized.LongValue.ShouldBe(Int64.MaxValue - 123);
            deserialized.FloatValue.ShouldBe(123.45f);
            deserialized.DecimalValue.ShouldBe(123.45m);
        }
    }
}
