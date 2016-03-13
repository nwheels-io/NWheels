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
        public void Roundtrip_ClassWithPrimitiveMembers()
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
        public void Roundtrip_ClassWithStructs()
        {
            //-- arrange

            var serializer = Framework.Components.Resolve<CompactSerializer>();
            var dictionary = new CompactSerializerDictionary();

            var original = new Repo.AClassWithStructs() {
                One = new Repo.PrimitiveStruct(
                    intValue: 123,
                    boolValue: true,
                    anotherBoolValue: false,
                    stringValue: "ABC",
                    anotherStringValue: null, 
                    systemEnumValue: DayOfWeek.Wednesday,
                    timeSpanValue: TimeSpan.FromSeconds(123)),
                Two = new Repo.AnotherPrimitiveStruct("XYZ"),
                Three = new Repo.NonPrimitiveStruct(
                    first: new Repo.Primitive() {
                        StringValue = "1ST"
                    },
                    second: new Repo.PrimitiveStruct(456, false, true, "2ND", "", DayOfWeek.Friday, TimeSpan.FromHours(1)),
                    third: new Repo.Primitive() {
                        StringValue = "3RD"
                    })
            };

            //-- act

            var serializedBytes = serializer.GetBytes(original, dictionary);
            var deserialized = serializer.GetObject<Repo.AClassWithStructs>(serializedBytes, dictionary);

            //-- assert

            deserialized.One.IntValue.ShouldBe(123);
            deserialized.One.BoolValue.ShouldBe(true);
            deserialized.One.AnotherBoolValue.ShouldBe(false);
            deserialized.One.StringValue.ShouldBe("ABC");
            deserialized.One.AnotherStringValue.ShouldBeNull();
            deserialized.One.SystemEnumValue.ShouldBe(DayOfWeek.Wednesday);
            deserialized.One.TimeSpanValue.ShouldBe(TimeSpan.FromSeconds(123));

            deserialized.Two.StringValue.ShouldBe("XYZ");

            deserialized.Three.First.StringValue.ShouldBe("1ST");
            deserialized.Three.Second.StringValue.ShouldBe("2ND");
            deserialized.Three.Third.StringValue.ShouldBe("3RD");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void Roundtrip_ClassWithNestedObjects()
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
        public void Roundtrip_ClassWithCollectionsOfPrimitiveTypes()
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

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void Roundtrip_BaseClass()
        {
            //-- arrange

            var serializer = Framework.Components.Resolve<CompactSerializer>();
            var dictionary = new CompactSerializerDictionary();

            var original = new Repo.BaseClass() {
                StringValue = "ABC"
            };

            //-- act

            var serializedBytes = serializer.GetBytes<Repo.BaseClass>(original, dictionary);
            var deserialized = serializer.GetObject<Repo.BaseClass>(serializedBytes, dictionary);

            //-- assert

            deserialized.ShouldNotBeNull();
            deserialized.StringValue.ShouldBe("ABC");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void Roundtrip_DerivedClassDeclaredAsBase()
        {
            //-- arrange

            var serializer = Framework.Components.Resolve<CompactSerializer>();
            var dictionary = new CompactSerializerDictionary();

            var original = new Repo.DerivedClassTwo() {
                StringValue = "ABC",
                LongValue = Int64.MaxValue - 123,
                DateTimeValue = new DateTime(2016, 10, 10, 12, 45, 50)
            };

            //-- act

            var serializedBytes = serializer.GetBytes<Repo.BaseClass>(original, dictionary);
            var deserialized = serializer.GetObject<Repo.BaseClass>(serializedBytes, dictionary);

            //-- assert

            deserialized.ShouldNotBeNull();
            deserialized.ShouldBeOfType<Repo.DerivedClassTwo>();
            deserialized.StringValue.ShouldBe("ABC");
            ((Repo.DerivedClassTwo)deserialized).LongValue.ShouldBe(Int64.MaxValue - 123);
            ((Repo.DerivedClassTwo)deserialized).DateTimeValue.ShouldBe(new DateTime(2016, 10, 10, 12, 45, 50));
        }
    }
}
