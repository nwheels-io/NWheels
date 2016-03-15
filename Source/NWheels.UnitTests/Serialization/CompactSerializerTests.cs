using System;
using System.Collections.Generic;
using System.IO;
using Autofac;
using NUnit.Framework;
using NWheels.DataObjects;
using NWheels.Serialization;
using NWheels.Serialization.Factories;
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
            var dictionary = new StaticCompactSerializerDictionary();

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
            var dictionary = new StaticCompactSerializerDictionary();

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
            var dictionary = new StaticCompactSerializerDictionary();

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
            var dictionary = new StaticCompactSerializerDictionary();

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
            var dictionary = new StaticCompactSerializerDictionary();

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
            var dictionary = new StaticCompactSerializerDictionary();

            dictionary.RegisterType(typeof(Repo.DerivedClassOne));
            dictionary.RegisterType(typeof(Repo.DerivedClassTwo));

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

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void Roundtrip_CollectionsOfPolymorphicObjects()
        {
            //-- arrange

            var serializer = Framework.Components.Resolve<CompactSerializer>();
            var dictionary = new StaticCompactSerializerDictionary();

            dictionary.RegisterType(typeof(Repo.DerivedClassOne));
            dictionary.RegisterType(typeof(Repo.DerivedClassTwo));
            dictionary.MakeImmutable();

            var original = new Repo.WithCollectionsOfPolymorphicObjects() {
                FirstArray = new Repo.BaseClass[] {
                    new Repo.DerivedClassOne() { StringValue = "One:First[0]", TimeSpanValue = TimeSpan.FromSeconds(123) },
                    new Repo.DerivedClassTwo() { StringValue = "Two:First[1]", DateTimeValue = new DateTime(2016, 1, 2)  },
                    new Repo.BaseClass()  { StringValue = "Base:First[2]" },
                },
                SecondList = new List<Repo.BaseClass>() {
                    new Repo.DerivedClassTwo() { StringValue = "Two:Second[0]", DateTimeValue = new DateTime(2016, 3, 4) },
                    new Repo.DerivedClassOne() { StringValue = "One:Second[1]", TimeSpanValue = TimeSpan.FromSeconds(456) },
                    new Repo.BaseClass() { StringValue = "Base:Second[2]" },
                },
                ThirdDictionary = new Dictionary<string, Repo.BaseClass>() {
                    { "AAA", new Repo.DerivedClassOne() { StringValue = "One:Third[AAA]", TimeSpanValue = TimeSpan.FromSeconds(789) } },
                    { "BBB", new Repo.DerivedClassTwo() { StringValue = "Two:Third[BBB]", DateTimeValue = new DateTime(2016, 5, 6)  } },
                    { "CCC", new Repo.BaseClass() { StringValue = "Base:Third[CCC]" } },
                }
            };

            //-- act

            var serializedBytes = serializer.GetBytes(original, dictionary);
            File.WriteAllBytes(@"C:\Temp\serialized1.bin", serializedBytes);
            var deserialized = serializer.GetObject<Repo.WithCollectionsOfPolymorphicObjects>(serializedBytes, dictionary);

            //-- assert

            deserialized.ShouldNotBeNull();

            deserialized.FirstArray.ShouldNotBeNull();
            deserialized.FirstArray.Length.ShouldBe(3);
            deserialized.FirstArray[0].ShouldBeOfType<Repo.DerivedClassOne>();
            deserialized.FirstArray[0].StringValue.ShouldBe("One:First[0]");
            deserialized.FirstArray[1].ShouldBeOfType<Repo.DerivedClassTwo>();
            deserialized.FirstArray[1].StringValue.ShouldBe("Two:First[1]");
            deserialized.FirstArray[2].ShouldBeOfType<Repo.BaseClass>();
            deserialized.FirstArray[2].StringValue.ShouldBe("Base:First[2]");

            deserialized.SecondList.ShouldNotBeNull();
            deserialized.SecondList.Count.ShouldBe(3);
            deserialized.SecondList[0].ShouldBeOfType<Repo.DerivedClassTwo>();
            deserialized.SecondList[0].StringValue.ShouldBe("Two:Second[0]");
            deserialized.SecondList[1].ShouldBeOfType<Repo.DerivedClassOne>();
            deserialized.SecondList[1].StringValue.ShouldBe("One:Second[1]");
            deserialized.SecondList[2].ShouldBeOfType<Repo.BaseClass>();
            deserialized.SecondList[2].StringValue.ShouldBe("Base:Second[2]");

            deserialized.ThirdDictionary.ShouldNotBeNull();
            deserialized.ThirdDictionary.Count.ShouldBe(3);
            deserialized.ThirdDictionary["AAA"].ShouldBeOfType<Repo.DerivedClassOne>();
            deserialized.ThirdDictionary["AAA"].StringValue.ShouldBe("One:Third[AAA]");
            deserialized.ThirdDictionary["BBB"].ShouldBeOfType<Repo.DerivedClassTwo>();
            deserialized.ThirdDictionary["BBB"].StringValue.ShouldBe("Two:Third[BBB]");
            deserialized.ThirdDictionary["CCC"].ShouldBeOfType<Repo.BaseClass>();
            deserialized.ThirdDictionary["CCC"].StringValue.ShouldBe("Base:Third[CCC]");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

#if false
        [Test]
        public void Roundtrip_PolymorphicObjectsWithTypeResolution()
        {
            //-- arrange

            var serverSerializer = new CompactSerializer(
                Resolve<IComponentContext>(), 
                Resolve<ITypeMetadataCache>(),
                Resolve<CompactSerializerFactory>(),
                new ICompactSerializerExtension[] {
                    
                });
            var serverDictionary = new StaticCompactSerializerDictionary();
            serverDictionary.RegisterType(typeof(Repo.DerivedClassOne));
            serverDictionary.RegisterType(typeof(Repo.DerivedClassTwo));
            serverDictionary.MakeImmutable();

            var original = new Repo.WithCollectionsOfPolymorphicObjects() {
                FirstArray = new Repo.BaseClass[] {
                    new Repo.DerivedClassOne() { StringValue = "One:First[0]", TimeSpanValue = TimeSpan.FromSeconds(123) },
                    new Repo.DerivedClassTwo() { StringValue = "Two:First[1]", DateTimeValue = new DateTime(2016, 1, 2)  },
                },
                SecondList = new List<Repo.BaseClass>() {
                    new Repo.DerivedClassTwo() { StringValue = "Two:Second[0]", DateTimeValue = new DateTime(2016, 3, 4) },
                    new Repo.DerivedClassOne() { StringValue = "One:Second[1]", TimeSpanValue = TimeSpan.FromSeconds(456) },
                },
                ThirdDictionary = new Dictionary<string, Repo.BaseClass>() {
                    { "AAA", new Repo.DerivedClassOne() { StringValue = "One:Third[AAA]", TimeSpanValue = TimeSpan.FromSeconds(789) } },
                    { "BBB", new Repo.DerivedClassTwo() { StringValue = "Two:Third[BBB]", DateTimeValue = new DateTime(2016, 5, 6)  } },
                }
            };

            //-- act

            var serializedBytes = serializer.GetBytes(original, dictionary);
            File.WriteAllBytes(@"C:\Temp\serialized1.bin", serializedBytes);
            var deserialized = serializer.GetObject<Repo.WithCollectionsOfPolymorphicObjects>(serializedBytes, dictionary);

            //-- assert

            deserialized.ShouldNotBeNull();

            deserialized.FirstArray.ShouldNotBeNull();
            deserialized.FirstArray.Length.ShouldBe(3);
            deserialized.FirstArray[0].ShouldBeOfType<Repo.DerivedClassOne>();
            deserialized.FirstArray[0].StringValue.ShouldBe("One:First[0]");
            deserialized.FirstArray[1].ShouldBeOfType<Repo.DerivedClassTwo>();
            deserialized.FirstArray[1].StringValue.ShouldBe("Two:First[1]");
            deserialized.FirstArray[2].ShouldBeOfType<Repo.BaseClass>();
            deserialized.FirstArray[2].StringValue.ShouldBe("Base:First[2]");

            deserialized.SecondList.ShouldNotBeNull();
            deserialized.SecondList.Count.ShouldBe(3);
            deserialized.SecondList[0].ShouldBeOfType<Repo.DerivedClassTwo>();
            deserialized.SecondList[0].StringValue.ShouldBe("Two:Second[0]");
            deserialized.SecondList[1].ShouldBeOfType<Repo.DerivedClassOne>();
            deserialized.SecondList[1].StringValue.ShouldBe("One:Second[1]");
            deserialized.SecondList[2].ShouldBeOfType<Repo.BaseClass>();
            deserialized.SecondList[2].StringValue.ShouldBe("Base:Second[2]");

            deserialized.ThirdDictionary.ShouldNotBeNull();
            deserialized.ThirdDictionary.Count.ShouldBe(3);
            deserialized.ThirdDictionary["AAA"].ShouldBeOfType<Repo.DerivedClassOne>();
            deserialized.ThirdDictionary["AAA"].StringValue.ShouldBe("One:Third[AAA]");
            deserialized.ThirdDictionary["BBB"].ShouldBeOfType<Repo.DerivedClassTwo>();
            deserialized.ThirdDictionary["BBB"].StringValue.ShouldBe("Two:Third[BBB]");
            deserialized.ThirdDictionary["CCC"].ShouldBeOfType<Repo.BaseClass>();
            deserialized.ThirdDictionary["CCC"].StringValue.ShouldBe("Base:Third[CCC]");
        }
#endif
    }
}
