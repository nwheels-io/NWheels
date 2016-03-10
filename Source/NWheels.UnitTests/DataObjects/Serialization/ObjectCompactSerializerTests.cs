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
    public class ObjectCompactSerializerTests : DynamicTypeUnitTestBase
    {
        [Test]
        public void Serialize_PrimitiveTypes()
        {
            //-- arrange

            var serializer = Framework.Components.Resolve<ObjectCompactSerializer>();

            var obj = new Repo.Primitive() {
                StringValue = "ABC",
                IntValue = 123,
                DecimalValue = 123.45m
            };

            //-- act

            var serializedBytes = serializer.GetBytes(obj, new ObjectCompactSerializerDictionary());

            //-- assert

            serializedBytes.ShouldNotBeNull();
            serializedBytes.Length.ShouldBeGreaterThan(10);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void Roundtrip_PrimitiveTypes()
        {
            //-- arrange

            var serializer = Framework.Components.Resolve<ObjectCompactSerializer>();
            var dictionary = new ObjectCompactSerializerDictionary();

            var guid = Guid.NewGuid();
            var original = new Repo.Primitive() {
                IntValue = 123,
                BoolValue = true,
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

            var serializedBytes = serializer.GetBytes(original, dictionary);
            var deserialized = serializer.GetObject<Repo.Primitive>(serializedBytes, dictionary);

            //-- assert

            deserialized.ShouldNotBeNull();
            deserialized.IntValue.ShouldBe(123);
            deserialized.BoolValue.ShouldBe(true);
            deserialized.AnotherBoolValue.ShouldBe(false);
            deserialized.StringValue.ShouldBe("ABC");
            deserialized.AnotherStringValue.ShouldBeNull();
            deserialized.SystemEnumValue.ShouldBe(DayOfWeek.Wednesday);
            deserialized.AppEnumValue.ShouldBe(Repo.AnAppEnum.Second);
            deserialized.TimeSpanValue.ShouldBe(TimeSpan.FromSeconds(123));
            deserialized.DateTimeValue.ShouldBe(new DateTime(2016, 05, 18, 13, 0, 3));
            deserialized.GuidValue.ShouldBe(guid);
            deserialized.LongValue.ShouldBe(Int64.MaxValue - 123);
            deserialized.FloatValue.ShouldBe(123.45f);
            deserialized.DecimalValue.ShouldBe(123.45m);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void Roundtrip_NestedObjects()
        {
            //-- arrange

            var serializer = Framework.Components.Resolve<ObjectCompactSerializer>();
            var dictionary = new ObjectCompactSerializerDictionary();

            var guid = Guid.NewGuid();
            var original = new Repo.WithNestedObjects() {
                First = new Repo.Primitive() {
                    StringValue = "ABC"
                },
                Second = new Repo.AnotherPrimitive() {
                    StringValue = "DEF"
                },
            };

            //-- act

            var serializedBytes = serializer.GetBytes(typeof(Repo.WithNestedObjects), original, dictionary);
            var deserialized = (Repo.WithNestedObjects)serializer.GetObject(typeof(Repo.WithNestedObjects), serializedBytes, dictionary);

            //-- assert

            deserialized.ShouldNotBeNull();
            deserialized.First.ShouldNotBeNull();
            deserialized.First.StringValue.ShouldBe("ABC");
            deserialized.Second.ShouldNotBeNull();
            deserialized.Second.StringValue.ShouldBe("DEF");
            deserialized.Third.ShouldBeNull();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void Roundtrip_CollectionsOfPrimitiveTypes()
        {
            //-- arrange

            var serializer = Framework.Components.Resolve<ObjectCompactSerializer>();
            var dictionary = new ObjectCompactSerializerDictionary();

            var original = new Repo.WithCollectionsOfPrimitiveTypes() {
                EnumArray = new[] { Repo.AnAppEnum.First, Repo.AnAppEnum.Second, Repo.AnAppEnum.Third },
                //StringList = new List<string>() { "AAA", "BBB", "CCC" },
                //DateTimeByIntDictionary = new Dictionary<int, DateTime>() {
                //    { 123, new DateTime(2016, 1, 10) },
                //    { 456, new DateTime(2016, 2, 11) },
                //    { 789, new DateTime(2016, 3, 12) },
                //}
            };

            //-- act

            var serializedBytes = serializer.GetBytes(original, dictionary);
            var deserialized = serializer.GetObject<Repo.WithCollectionsOfPrimitiveTypes>(serializedBytes, dictionary);

            //-- assert

            deserialized.ShouldNotBeNull();

            deserialized.EnumArray.ShouldNotBeNull();
            deserialized.EnumArray.ShouldBe(new[] { Repo.AnAppEnum.First, Repo.AnAppEnum.Second, Repo.AnAppEnum.Third });

            deserialized.StringList.ShouldNotBeNull();
            deserialized.StringList.ShouldBe(new[] { "AAA", "BBB", "CCC" });

            //deserialized.DateTimeByIntDictionary.ShouldNotBeNull();
            //deserialized.DateTimeByIntDictionary.Count.ShouldBe(3);
            //deserialized.DateTimeByIntDictionary.ShouldContainKeyAndValue(123, new DateTime(2016, 1, 10));
            //deserialized.DateTimeByIntDictionary.ShouldContainKeyAndValue(456, new DateTime(2016, 2, 11));
            //deserialized.DateTimeByIntDictionary.ShouldContainKeyAndValue(789, new DateTime(2016, 3, 12));
        }
    }
}
