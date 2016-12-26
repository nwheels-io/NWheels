using System;
using System.Collections.Generic;
using System.IO;
using Autofac;
using NUnit.Framework;
using NWheels.DataObjects;
using NWheels.Extensions;
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
        public void Roundtrip_ClassWithNullables_AllNotNulls()
        {
            //-- arrange

            var serializer = Framework.Components.Resolve<CompactSerializer>();
            var dictionary = new StaticCompactSerializerDictionary();

            var original = new Repo.AClassWithNullables() {
                IntValue = 987,
                EnumValue = TestObjectRepository.AnAppEnum.Second,
                AnotherValue = new Repo.AnotherPrimitiveStruct("ABC")
            };

            //-- act

            var serializedBytes = serializer.GetBytes(original, dictionary);
            var deserialized = serializer.GetObject<Repo.AClassWithNullables>(serializedBytes, dictionary);

            //-- assert

            deserialized.IntValue.HasValue.ShouldBe(true);
            deserialized.IntValue.Value.ShouldBe(987);

            deserialized.EnumValue.HasValue.ShouldBe(true);
            deserialized.EnumValue.Value.ShouldBe(TestObjectRepository.AnAppEnum.Second);

            deserialized.AnotherValue.HasValue.ShouldBe(true);
            deserialized.AnotherValue.Value.StringValue.ShouldBe("ABC");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void Roundtrip_ClassWithNullables_AllNulls()
        {
            //-- arrange

            var serializer = Framework.Components.Resolve<CompactSerializer>();
            var dictionary = new StaticCompactSerializerDictionary();

            var original = new Repo.AClassWithNullables() {
                IntValue = null,
                EnumValue = null,
                AnotherValue = null
            };

            //-- act

            var serializedBytes = serializer.GetBytes(original, dictionary);
            var deserialized = serializer.GetObject<Repo.AClassWithNullables>(serializedBytes, dictionary);

            //-- assert

            deserialized.IntValue.HasValue.ShouldBe(false);
            deserialized.EnumValue.HasValue.ShouldBe(false);
            deserialized.AnotherValue.HasValue.ShouldBe(false);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void Roundtrip_ClassWithNullables_SomeNulls()
        {
            //-- arrange

            var serializer = Framework.Components.Resolve<CompactSerializer>();
            var dictionary = new StaticCompactSerializerDictionary();

            var original1 = new Repo.AClassWithNullables() {
                IntValue = null,
                EnumValue = TestObjectRepository.AnAppEnum.Second,
                AnotherValue = null
            };

            var original2 = new Repo.AClassWithNullables() {
                IntValue = 987,
                EnumValue = null,
                AnotherValue = new Repo.AnotherPrimitiveStruct("ABC")
            };

            //-- act

            var serializedBytes1 = serializer.GetBytes(original1, dictionary);
            var serializedBytes2 = serializer.GetBytes(original2, dictionary);
            var deserialized1 = serializer.GetObject<Repo.AClassWithNullables>(serializedBytes1, dictionary);
            var deserialized2 = serializer.GetObject<Repo.AClassWithNullables>(serializedBytes2, dictionary);

            //-- assert

            deserialized1.IntValue.HasValue.ShouldBe(false);
            
            deserialized1.EnumValue.HasValue.ShouldBe(true);
            deserialized1.EnumValue.Value.ShouldBe(TestObjectRepository.AnAppEnum.Second);
            
            deserialized1.AnotherValue.HasValue.ShouldBe(false);

            deserialized2.IntValue.HasValue.ShouldBe(true);
            deserialized2.IntValue.Value.ShouldBe(987);
            
            deserialized2.EnumValue.HasValue.ShouldBe(false);
            
            deserialized2.AnotherValue.HasValue.ShouldBe(true);
            deserialized2.AnotherValue.Value.StringValue.ShouldBe("ABC");
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
            //File.WriteAllBytes(@"C:\Temp\serialized1.bin", serializedBytes);
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

        [Test]
        public void Roundtrip_PolymorphicObjectsWithClientServerTypeResolution()
        {
            //-- arrange

            var serverSerializer = new CompactSerializer(
                Resolve<IComponentContext>(), 
                Resolve<ITypeMetadataCache>(),
                Resolve<CompactSerializerFactory>(),
                new ICompactSerializerExtension[] {
                    new ServerEntityTypeResolver()
                });
            var serverDictionary = new StaticCompactSerializerDictionary();
            serverDictionary.RegisterType(typeof(Repo.IEntityAOne));
            serverDictionary.RegisterType(typeof(Repo.IEntityATwo));
            serverDictionary.RegisterType(typeof(Repo.IEntityB));
            serverDictionary.MakeImmutable();

            var clientSerializer = new CompactSerializer(
                Resolve<IComponentContext>(),
                Resolve<ITypeMetadataCache>(),
                Resolve<CompactSerializerFactory>(),
                new ICompactSerializerExtension[] {
                    new ClientEntityTypeResolver()
                });
            var clientDictionary = new StaticCompactSerializerDictionary();
            clientDictionary.RegisterType(typeof(Repo.IEntityAOne));
            clientDictionary.RegisterType(typeof(Repo.IEntityATwo));
            clientDictionary.RegisterType(typeof(Repo.IEntityB));
            clientDictionary.MakeImmutable();

            var originalOnServer = new Repo.WithEntityObjects() {
                TheA = new List<Repo.IEntityA> {
                    new Repo.ServerEntityAOne() {
                        Id = 123,
                        Name = "ABC",
                        IntValue = 112233,
                        TimeValue = TimeSpan.FromDays(123)
                    },
                    new Repo.ServerEntityATwo() {
                        Id = 456,
                        Name = "DEF",
                        DateValue = new DateTime(2456, 1, 1)
                    }
                },
                TheB = new Repo.ServerEntityB() {
                    Id = "XYZ",
                    TheA = new Repo.ServerEntityATwo() {
                        Id = 789,
                        Name = "GHI",
                        DateValue = new DateTime(2789, 1, 1)
                    }
                }
            };

            //-- act

            var serializedBytesOnServer = serverSerializer.GetBytes(originalOnServer, serverDictionary);
            //File.WriteAllBytes(@"C:\Temp\serialized1.bin", serializedBytesOnServer);
            var deserializedOnClient = clientSerializer.GetObject<Repo.WithEntityObjects>(serializedBytesOnServer, clientDictionary);
            var serializedBytesOnClient = clientSerializer.GetBytes(deserializedOnClient, clientDictionary);
            //File.WriteAllBytes(@"C:\Temp\serialized2.bin", serializedBytesOnClient);
            var deserializedOnServer = serverSerializer.GetObject<Repo.WithEntityObjects>(serializedBytesOnClient, serverDictionary);

            //-- assert

            deserializedOnClient.ShouldNotBeNull();

            deserializedOnClient.TheA.ShouldNotBeNull();
            deserializedOnClient.TheA.Count.ShouldBe(2);
            deserializedOnClient.TheA[0].ShouldBeOfType<Repo.ClientEntityAOne>();
            deserializedOnClient.TheA[0].As<Repo.ClientEntityAOne>().Id.ShouldBe(123);
            deserializedOnClient.TheA[0].As<Repo.ClientEntityAOne>().Name.ShouldBe("ABC");
            deserializedOnClient.TheA[0].As<Repo.ClientEntityAOne>().IntValue.ShouldBe(112233);
            deserializedOnClient.TheA[0].As<Repo.ClientEntityAOne>().TimeValue.ShouldBe(TimeSpan.FromDays(123));
            deserializedOnClient.TheA[1].ShouldBeOfType<Repo.ClientEntityATwo>();
            deserializedOnClient.TheA[1].As<Repo.ClientEntityATwo>().Id.ShouldBe(456);
            deserializedOnClient.TheA[1].As<Repo.ClientEntityATwo>().Name.ShouldBe("DEF");
            deserializedOnClient.TheA[1].As<Repo.ClientEntityATwo>().DateValue.ShouldBe(new DateTime(2456, 1, 1));

            deserializedOnClient.TheB.ShouldBeOfType<Repo.ClientEntityB>();
            deserializedOnClient.TheB.Id.ShouldBe("XYZ");
            deserializedOnClient.TheB.TheA.ShouldNotBeNull();
            deserializedOnClient.TheB.TheA.ShouldBeOfType<Repo.ClientEntityATwo>();
            deserializedOnClient.TheB.TheA.As<Repo.ClientEntityATwo>().Id.ShouldBe(789);
            deserializedOnClient.TheB.TheA.As<Repo.ClientEntityATwo>().Name.ShouldBe("GHI");
            deserializedOnClient.TheB.TheA.As<Repo.ClientEntityATwo>().DateValue.ShouldBe(new DateTime(2789, 1, 1));

            deserializedOnServer.ShouldNotBeNull();

            deserializedOnServer.TheA.ShouldNotBeNull();
            deserializedOnServer.TheA.Count.ShouldBe(2);
            deserializedOnServer.TheA[0].ShouldBeOfType<Repo.ServerEntityAOne>();
            deserializedOnServer.TheA[0].As<Repo.ServerEntityAOne>().Id.ShouldBe(123);
            deserializedOnServer.TheA[0].As<Repo.ServerEntityAOne>().Name.ShouldBe("ABC");
            deserializedOnServer.TheA[0].As<Repo.ServerEntityAOne>().IntValue.ShouldBe(112233);
            deserializedOnServer.TheA[0].As<Repo.ServerEntityAOne>().TimeValue.ShouldBe(TimeSpan.FromDays(123));
            deserializedOnServer.TheA[1].ShouldBeOfType<Repo.ServerEntityATwo>();
            deserializedOnServer.TheA[1].As<Repo.ServerEntityATwo>().Id.ShouldBe(456);
            deserializedOnServer.TheA[1].As<Repo.ServerEntityATwo>().Name.ShouldBe("DEF");
            deserializedOnServer.TheA[1].As<Repo.ServerEntityATwo>().DateValue.ShouldBe(new DateTime(2456, 1, 1));

            deserializedOnServer.TheB.ShouldBeOfType<Repo.ServerEntityB>();
            deserializedOnServer.TheB.Id.ShouldBe("XYZ");
            deserializedOnServer.TheB.TheA.ShouldNotBeNull();
            deserializedOnServer.TheB.TheA.ShouldBeOfType<Repo.ServerEntityATwo>();
            deserializedOnServer.TheB.TheA.As<Repo.ServerEntityATwo>().Id.ShouldBe(789);
            deserializedOnServer.TheB.TheA.As<Repo.ServerEntityATwo>().Name.ShouldBe("GHI");
            deserializedOnServer.TheB.TheA.As<Repo.ServerEntityATwo>().DateValue.ShouldBe(new DateTime(2789, 1, 1));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class ServerEntityTypeResolver : CompactSerializerExtensionBase
        {
            #region Overrides of CompactSerializerExtensionBase

            public override Type GetSerializationType(Type declaredType, object obj)
            {
                if (obj is Repo.IEntityAOne)
                {
                    return typeof(Repo.IEntityAOne);
                }
                if (obj is Repo.IEntityATwo)
                {
                    return typeof(Repo.IEntityATwo);
                }
                if (obj is Repo.IEntityB)
                {
                    return typeof(Repo.IEntityB);
                }

                return obj.GetType();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override Type GetMaterializationType(Type declaredType, Type serializedType)
            {
                if (serializedType == typeof(Repo.IEntityAOne))
                {
                    return typeof(Repo.ServerEntityAOne);
                }
                if (serializedType == typeof(Repo.IEntityATwo))
                {
                    return typeof(Repo.ServerEntityATwo);
                }
                if (serializedType == typeof(Repo.IEntityB))
                {
                    return typeof(Repo.ServerEntityB);
                }
                return serializedType;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override bool CanMaterialize(Type declaredType, Type serializedType)
            {
                return (
                    serializedType == typeof(Repo.IEntityAOne) ||
                    serializedType == typeof(Repo.IEntityATwo) ||
                    serializedType == typeof(Repo.IEntityB));
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override object Materialize(Type declaredType, Type serializedType)
            {
                if (serializedType == typeof(Repo.IEntityAOne))
                {
                    return new Repo.ServerEntityAOne();
                }
                if (serializedType == typeof(Repo.IEntityATwo))
                {
                    return new Repo.ServerEntityATwo();
                }
                if (serializedType == typeof(Repo.IEntityB))
                {
                    return new Repo.ServerEntityB();
                }
                throw new ArgumentException("Test ServerEntityTypeResolver cannot materialized serialized type: " + serializedType.Name);
            }


            #endregion
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class ClientEntityTypeResolver : CompactSerializerExtensionBase
        {
            #region Overrides of CompactSerializerExtensionBase

            public override Type GetSerializationType(Type declaredType, object obj)
            {
                if (obj is Repo.IEntityAOne)
                {
                    return typeof(Repo.IEntityAOne);
                }
                if (obj is Repo.IEntityATwo)
                {
                    return typeof(Repo.IEntityATwo);
                }
                if (obj is Repo.IEntityB)
                {
                    return typeof(Repo.IEntityB);
                }

                return obj.GetType();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override Type GetMaterializationType(Type declaredType, Type serializedType)
            {
                if (serializedType == typeof(Repo.IEntityAOne))
                {
                    return typeof(Repo.ClientEntityAOne);
                }
                if (serializedType == typeof(Repo.IEntityATwo))
                {
                    return typeof(Repo.ClientEntityATwo);
                }
                if (serializedType == typeof(Repo.IEntityB))
                {
                    return typeof(Repo.ClientEntityB);
                }
                return serializedType;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override bool CanMaterialize(Type declaredType, Type serializedType)
            {
                return (
                    serializedType == typeof(Repo.IEntityAOne) || 
                    serializedType == typeof(Repo.IEntityATwo) || 
                    serializedType == typeof(Repo.IEntityB));
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override object Materialize(Type declaredType, Type serializedType)
            {
                if (serializedType == typeof(Repo.IEntityAOne))
                {
                    return new Repo.ClientEntityAOne();
                }
                if (serializedType == typeof(Repo.IEntityATwo))
                {
                    return new Repo.ClientEntityATwo();
                }
                if (serializedType == typeof(Repo.IEntityB))
                {
                    return new Repo.ClientEntityB();
                }
                throw new ArgumentException("Test ClientEntityTypeResolver cannot materialized serialized type: " + serializedType.Name);
            }

            #endregion
        }
    }
}
