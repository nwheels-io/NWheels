using System;
using System.Collections.Generic;
using System.IO;
using Autofac;
using NUnit.Framework;
using NWheels.Serialization;
using NWheels.Testing;
using Shouldly;
using Repo = NWheels.UnitTests.Serialization.TestObjectRepository;

namespace NWheels.UnitTests.Serialization
{
    [TestFixture]
    public class CompactSerializerTests : DynamicTypeUnitTestBase
    {
        [Test]
        public void Roundtrip_PrimitiveTypes()
        {
            //-- arrange

            var serializer = Framework.Components.Resolve<CompactSerializer>();
            var dictionary = new CompactSerializerDictionary();

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

            var serializer = Framework.Components.Resolve<CompactSerializer>();
            var dictionary = new CompactSerializerDictionary();

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

            var serializer = Framework.Components.Resolve<CompactSerializer>();
            var dictionary = new CompactSerializerDictionary();

            var original = new Repo.WithCollectionsOfPrimitiveTypes() {
                EnumArray = new[] { Repo.AnAppEnum.First, Repo.AnAppEnum.Second, Repo.AnAppEnum.Third },
                StringList = new List<string>() { "AAA", "BBB", "CCC" },
                IntStringDictionary = new Dictionary<int, string>() {
                    { 123, "ABCD" },
                    { 456, "EFGH" },
                    { 789, "IJKL" },
                }
            };

            //-- act

            var serializedBytes = serializer.GetBytes(original, dictionary);
            File.WriteAllBytes(@"C:\Temp\serialized1.bin", serializedBytes);
            var deserialized = serializer.GetObject<Repo.WithCollectionsOfPrimitiveTypes>(serializedBytes, dictionary);

            //-- assert

            deserialized.ShouldNotBeNull();

            deserialized.EnumArray.ShouldNotBeNull();
            deserialized.EnumArray.ShouldBe(new[] { Repo.AnAppEnum.First, Repo.AnAppEnum.Second, Repo.AnAppEnum.Third });

            deserialized.StringList.ShouldNotBeNull();
            deserialized.StringList.ShouldBe(new[] { "AAA", "BBB", "CCC" });

            deserialized.IntStringDictionary.ShouldNotBeNull();
            deserialized.IntStringDictionary.Count.ShouldBe(3);
            deserialized.IntStringDictionary.ShouldContainKeyAndValue(123, "ABCD");
            deserialized.IntStringDictionary.ShouldContainKeyAndValue(456, "EFGH");
            deserialized.IntStringDictionary.ShouldContainKeyAndValue(789, "IJKL");
        }
    }
}
