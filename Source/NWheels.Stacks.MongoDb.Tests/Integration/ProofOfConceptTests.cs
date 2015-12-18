using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using NUnit.Framework;
using NWheels.DataObjects.Core;
using NWheels.Entities;
using NWheels.Entities.Core;
using NWheels.Extensions;
using NWheels.Stacks.MongoDb.Factories;
using NWheels.Testing;
using NWheels.UI.Toolbox;
using Shouldly;

namespace NWheels.Stacks.MongoDb.Tests.Integration
{
    using PocFramework;
    using PocMongoStack;

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    [TestFixture]
    public class ProofOfConceptTests : IntegrationTestWithoutNodeHosts
    {
        public const string ConnectionString = "server=localhost;database=ProofOfConceptTests";

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [SetUp]
        public void SetUp()
        {
            var initializer = new MongoDatabaseInitializer(new IDomainContextPopulator[0]);
            initializer.DropStorageSchema(ConnectionString);

            PocQueryLog.Clear();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanInsertData()
        {
            PocDomain.IMyPocContext context = new DomainImpl.PocMongoDomainContext(Framework.Components, ConnectionString);

            var one1 = context.Ones.New();
            var one2 = context.Ones.New();

            var two1 = context.Twos.New();
            var two2 = context.Twos.New();
            var two3 = context.Twos.New();

            var three1 = context.Threes.New();
            var three2 = context.Threes.New();
            var three3 = context.Threes.New();
            var three4 = context.Threes.New();

            var four1 = context.NewEntityFour(id: "F1");
            var four2 = context.NewEntityFour(id: "F2");
            var four3 = context.NewEntityFour(id: "F3");
            var four4 = context.NewEntityFour(id: "F4");
            var four5 = context.NewEntityFour(id: "F5");

            one1.IntScalar = 12345;
            one1.StringScalar = "ABCDEF";
            one1.ValueA.DateTimeScalar = new DateTime(2015, 10, 20, 17, 30, 50, 123);
            one1.ValueA.TimeSpanScalar = TimeSpan.FromSeconds(47);

            one1.ValueBs.Add(context.NewValueObjectB(
                new Guid("410E12AA-9754-4562-A205-E08A8F1667D2"), 
                DayOfWeek.Thursday, 
                context.NewValueObjectC(
                    PocDomain.AFlagsEnum.None,
                    12345.67m,
                    three1,
                    four1, four2
                ),
                context.NewValueObjectC(
                    PocDomain.AFlagsEnum.Second | PocDomain.AFlagsEnum.Third,
                    67890.12m,
                    three2,
                    four3, four4
                )
            ));
            one1.ValueBs.Add(context.NewValueObjectB(
                new Guid("B547C485-342B-4EAA-A5E7-7CCACBC05F24"), 
                DayOfWeek.Wednesday, 
                context.NewValueObjectC(
                    PocDomain.AFlagsEnum.None,
                    12345.67m,
                    three3,
                    four3, four4
                ),
                context.NewValueObjectC(
                    PocDomain.AFlagsEnum.Second | PocDomain.AFlagsEnum.Third,
                    67890.12m,
                    three: null
                )
            ));

            one1.Threes.Add(three1);
            one1.Threes.Add(three2);
            one1.Threes.Add(three3);

            two1.StringValue = "TWO1";
            two1.One = one1;

            two2.StringValue = "TWO2";
            two2.One = null;

            three1.StringValue = "THREE1";
            three2.StringValue = "THREE2";
            three3.StringValue = "THREE3";
            three4.StringValue = "THREE4";

            four1.StringValue = "FOUR1";
            four1.Three = three1;

            four2.StringValue = "FOUR2";
            four2.Three = three3;

            four3.StringValue = "FOUR3";
            four3.Three = three3;

            four4.StringValue = "FOUR4";
            four4.Three = three3;

            context.Ones.Insert(one1);
            context.Ones.Insert(one2);
            context.Twos.Insert(two1);
            context.Twos.Insert(two2);
            context.Twos.Insert(two3);
            context.Threes.Insert(three1);
            context.Threes.Insert(three2);
            context.Threes.Insert(three3);
            context.Threes.Insert(three4);
            context.Fours.Insert(four1);
            context.Fours.Insert(four2);
            context.Fours.Insert(four3);
            context.Fours.Insert(four4);
            context.Fours.Insert(four5);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanLoadData()
        {
            CanInsertData();

            PocDomain.IMyPocContext context = new DomainImpl.PocMongoDomainContext(Framework.Components, ConnectionString);
            ((IPocMongoDomainContext)context).Database.SetProfilingLevel(ProfilingLevel.All);

            var one1 = context.Ones.GetById(1);

            one1.ShouldNotBeNull();
            one1.IntScalar.ShouldBe(12345);
            one1.StringScalar.ShouldBe("ABCDEF");
            one1.ValueA.ShouldNotBeNull();
            one1.ValueA.DateTimeScalar.ShouldBe(new DateTime(2015, 10, 20, 17, 30, 50, 123, DateTimeKind.Utc));
            one1.ValueA.TimeSpanScalar.ShouldBe(TimeSpan.FromSeconds(47));
            one1.ValueBs.ShouldNotBeNull();
            one1.ValueBs.Count.ShouldBe(2);

            var one1Bs = one1.ValueBs.ToArray();
            one1Bs[0].ShouldNotBeNull();
            one1Bs[0].EnumScalar.ShouldBe(DayOfWeek.Thursday);
            one1Bs[0].GuidScalar.ShouldBe(new Guid("410E12AA-9754-4562-A205-E08A8F1667D2"));
            one1Bs[0].ValueCs.ShouldNotBeNull();
            one1Bs[0].ValueCs.Count.ShouldBe(2);

            var one1Bs0Cs = one1Bs[0].ValueCs.ToArray();
            one1Bs0Cs[0].ShouldNotBeNull();
            one1Bs0Cs[0].FlagsEnumScalar.ShouldBe(PocDomain.AFlagsEnum.None);
            one1Bs0Cs[0].DecimalValue.ShouldBe(12345.67m);
            
            one1Bs0Cs[1].ShouldNotBeNull();
            one1Bs0Cs[1].FlagsEnumScalar.ShouldBe(PocDomain.AFlagsEnum.Second | PocDomain.AFlagsEnum.Third);
            one1Bs0Cs[1].DecimalValue.ShouldBe(67890.12m);

            one1Bs[1].ShouldNotBeNull();
            one1Bs[1].EnumScalar.ShouldBe(DayOfWeek.Wednesday);
            one1Bs[1].GuidScalar.ShouldBe(new Guid("B547C485-342B-4EAA-A5E7-7CCACBC05F24"));
            one1Bs[1].ValueCs.ShouldNotBeNull();
            one1Bs[1].ValueCs.Count.ShouldBe(2);

            var one1Bs1Cs = one1Bs[1].ValueCs.ToArray();
            one1Bs1Cs[0].ShouldNotBeNull();
            one1Bs1Cs[0].FlagsEnumScalar.ShouldBe(PocDomain.AFlagsEnum.None);
            one1Bs1Cs[0].DecimalValue.ShouldBe(12345.67m);

            one1Bs1Cs[1].ShouldNotBeNull();
            one1Bs1Cs[1].FlagsEnumScalar.ShouldBe(PocDomain.AFlagsEnum.Second | PocDomain.AFlagsEnum.Third);
            one1Bs1Cs[1].DecimalValue.ShouldBe(67890.12m);

            PocQueryLog.TakeLog().ShouldBe(new[] { "IEntityOne[1]" });

            one1.Two.ShouldNotBeNull();

            PocQueryLog.Count.ShouldBe(0);

            one1.Two.Id.ShouldBeOfType<ObjectId>();

            PocQueryLog.TakeLog().ShouldBe(new[] { "LLFK[EntityTwo]" });

            one1.Two.StringValue.ShouldBe("TWO1");
            one1.Two.One.ShouldNotBeNull();
            one1.Two.One.Id.ShouldBe(1);

            PocQueryLog.Count.ShouldBe(0);

            one1Bs0Cs[0].Fours.ShouldNotBeNull();
            
            PocQueryLog.TakeLog().ShouldBe(new[] { "CLLIDS[EntityFour]" });

            one1Bs0Cs[0].Fours.Count.ShouldBe(2);
            one1Bs0Cs[0].Fours.Count(f => f.Id == "F1").ShouldBe(1);
            one1Bs0Cs[0].Fours.Count(f => f.Id == "F2").ShouldBe(1);

            PocQueryLog.Count.ShouldBe(0);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static class PocDomain
        {
            public interface IMyPocContext
            {
                IPocEntityRepo<IEntityOne> Ones { get; }
                IPocEntityRepo<IEntityTwo> Twos { get; }
                IPocEntityRepo<IEntityThree> Threes { get; }
                IPocEntityRepo<IEntityFour> Fours { get; }

                IEntityFour NewEntityFour(string id);
                IValueObjectB NewValueObjectB(Guid guidScalar, DayOfWeek enumScalar, params IValueObjectC[] valueCs);
                IValueObjectC NewValueObjectC(AFlagsEnum flagsEnumScalar, decimal decimalValue, IEntityThree three, params IEntityFour[] fours);
            }

            public interface IEntityOne
            {
                int Id { get; }
                int IntScalar { get; set; }
                string StringScalar { get; set; }
                IValueObjectA ValueA { get; }
                ICollection<IValueObjectB> ValueBs { get; }
                IEntityTwo Two { get; set; }
                ICollection<IEntityThree> Threes { get; }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public interface IEntityTwo
            {
                object Id { get; }
                string StringValue { get; set; }
                IEntityOne One { get; set; }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public interface IEntityThree
            {
                object Id { get; }
                string StringValue { get; set; }
                ICollection<IEntityFour> Fours { get; }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public interface IEntityFour
            {
                string Id { get; set; }
                string StringValue { get; set; }
                IEntityThree Three { get; set; }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public interface IValueObjectA
            {
                TimeSpan TimeSpanScalar { get; set; }
                DateTime DateTimeScalar { get; set; }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public interface IValueObjectB
            {
                Guid GuidScalar { get; set; }
                DayOfWeek EnumScalar { get; set; }
                IList<IValueObjectC> ValueCs { get; }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public interface IValueObjectC
            {
                AFlagsEnum FlagsEnumScalar { get; set; }
                decimal DecimalValue { get; set; }
                IEntityThree Three { get; set; }
                ICollection<IEntityFour> Fours { get; }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            [Flags]
            public enum AFlagsEnum
            {
                None = 0,
                Frist = 0x01,
                Second = 0x02,
                Third = 0x04
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static class DomainImpl
        {
            public class DomainObject_EntityOne : PocDomain.IEntityOne, IPocDomainObject
            {
                private readonly IComponentContext _components;
                private IPersistableObjectLazyLoader _thisLazyLoader;

                //---------------------------------------------------------------------------------------------------------------------------------------------

                public DomainObject_EntityOne(IComponentContext components)
                {
                    _components = components;
                    _thisLazyLoader = null;
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                void IPocDomainObject.InitializeValues(bool idManuallyAssigned)
                {
                    m_Id = IntIdGenerator.TakeNextValue();
                    m_IntScalar = 123;
                    m_StringScalar = "ABC";

                    m_ValueA = new DomainObject_ValueObjectA(_components);
                    ((IPocDomainObject)m_ValueA).InitializeValues(false);

                    m_ValueBs = new ConcreteToAbstractListAdapter<DomainObject_ValueObjectB, PocDomain.IValueObjectB>(new List<DomainObject_ValueObjectB>());
                    m_Two = null;
                    m_Threes = new ConcreteToAbstractListAdapter<DomainObject_EntityThree, PocDomain.IEntityThree>(new List<DomainObject_EntityThree>());
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                void IPocImportExportValues.ImportValues(IPocEntityRepo entityRepo, object[] values)
                {
                    m_Id = (int)values[0];
                    m_IntScalar = (int)values[1];
                    m_StringScalar = (string)values[2];
                    m_ValueA = PocDomainRuntimeHelpers.ImportEmbeddedDomainObject<PocDomain.IValueObjectA, DomainObject_ValueObjectA>(
                        entityRepo,
                        values[3],
                        () => new DomainObject_ValueObjectA(_components));
                    m_ValueBs = PocDomainRuntimeHelpers.ImportEmbeddedDomainCollection<PocDomain.IValueObjectB, DomainObject_ValueObjectB>(
                        entityRepo,
                        values[4],
                        () => new DomainObject_ValueObjectB(_components));
                    m_Two = PocDomainRuntimeHelpers.ImportDomainLazyLoadObject<PocDomain.IEntityTwo, DomainObject_EntityTwo>(
                        entityRepo,
                        values[5],
                        () => new DomainObject_EntityTwo(_components));
                    m_Threes = PocDomainRuntimeHelpers.ImportDomainLazyLoadObjectCollection<PocDomain.IEntityThree, DomainObject_EntityThree>(
                        entityRepo,
                        values[6],
                        () => new DomainObject_EntityThree(_components),
                        out m_Threes_LazyLoader);
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                object[] IPocImportExportValues.ExportValues(IPocEntityRepo entityRepo)
                {
                    return new object[] { m_Id, m_IntScalar, m_StringScalar, m_ValueA, m_ValueBs, m_Two, m_Threes };
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                void IPocDomainObject.SetLazyLoader(IPersistableObjectLazyLoader lazyLoader)
                {
                    _thisLazyLoader = lazyLoader;
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                object IPocDomainObject.EntityId
                {
                    get { return Id; }
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                #region Implementation of IEntityOne

                private int m_Id;
                public int Id
                {
                    get { return PocDomainRuntimeHelpers.EntityIdPropertyGetter(this, ref _thisLazyLoader, ref m_Id); }
                }

                private int m_IntScalar;
                public int IntScalar 
                {
                    get { return PocDomainRuntimeHelpers.PropertyGetter(this, ref _thisLazyLoader, ref m_IntScalar); }
                    set { PocDomainRuntimeHelpers.PropertySetter(this, ref _thisLazyLoader, out m_IntScalar, ref value); }
                }

                private string m_StringScalar;
                public string StringScalar
                {
                    get { return PocDomainRuntimeHelpers.PropertyGetter(this, ref _thisLazyLoader, ref m_StringScalar); }
                    set { PocDomainRuntimeHelpers.PropertySetter(this, ref _thisLazyLoader, out m_StringScalar, ref value); }
                }

                private PocDomain.IValueObjectA m_ValueA;
                public PocDomain.IValueObjectA ValueA
                {
                    get { return PocDomainRuntimeHelpers.PropertyGetter(this, ref _thisLazyLoader, ref m_ValueA); }
                }

                private ICollection<PocDomain.IValueObjectB> m_ValueBs;
                public ICollection<PocDomain.IValueObjectB> ValueBs
                {
                    get { return PocDomainRuntimeHelpers.PropertyGetter(this, ref _thisLazyLoader, ref m_ValueBs); }
                }

                private PocDomain.IEntityTwo m_Two;
                private IPersistableObjectLazyLoader m_Two_LazyLoader;
                public PocDomain.IEntityTwo Two
                {
                    get { return PocDomainRuntimeHelpers.LazyLoadObjectPropertyGetter(this, ref _thisLazyLoader, ref m_Two_LazyLoader, ref m_Two); }
                    set { PocDomainRuntimeHelpers.LazyLoadObjectPropertySetter(this, ref _thisLazyLoader, out m_Two_LazyLoader, out m_Two, ref value); }
                }


                private IList<PocDomain.IEntityThree> m_Threes;
                private IPersistableObjectCollectionLazyLoader m_Threes_LazyLoader;
                public ICollection<PocDomain.IEntityThree> Threes
                {
                    get { return PocDomainRuntimeHelpers.LazyLoadObjectCollectionPropertyGetter<PocDomain.IEntityThree, DomainObject_EntityThree>(this, ref _thisLazyLoader, ref m_Threes_LazyLoader, ref m_Threes); }
                }

                #endregion
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public class DomainObject_EntityTwo : PocDomain.IEntityTwo, IPocDomainObject
            {
                private readonly IComponentContext _components;
                private IPersistableObjectLazyLoader _thisLazyLoader;

                //---------------------------------------------------------------------------------------------------------------------------------------------

                public DomainObject_EntityTwo(IComponentContext components)
                {
                    _components = components;
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                void IPocDomainObject.InitializeValues(bool idManuallyAssigned)
                {
                    m_Id = ObjectId.GenerateNewId();
                    m_StringValue = "ABC2";
                    m_One = null;
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                void IPocDomainObject.SetLazyLoader(IPersistableObjectLazyLoader lazyLoader)
                {
                    _thisLazyLoader = lazyLoader;
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                void IPocImportExportValues.ImportValues(IPocEntityRepo entityRepo, object[] values)
                {
                    m_Id = (ObjectId)values[0];
                    m_StringValue = (string)values[1];
                    m_One = PocDomainRuntimeHelpers.ImportDomainLazyLoadObject<PocDomain.IEntityOne, DomainObject_EntityOne>(
                        entityRepo,
                        values[2],
                        () => new DomainObject_EntityOne(_components));
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                object[] IPocImportExportValues.ExportValues(IPocEntityRepo entityRepo)
                {
                    return new object[] { m_Id, m_StringValue, m_One };
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                object IPocDomainObject.EntityId
                {
                    get { return this.Id; }
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                #region Implementation of IEntityTwo

                private ObjectId m_Id;
                public object Id
                {
                    get { return PocDomainRuntimeHelpers.EntityIdPropertyGetter(this, ref _thisLazyLoader, ref m_Id); }
                }

                private string m_StringValue;
                public string StringValue
                {
                    get { return PocDomainRuntimeHelpers.PropertyGetter(this, ref _thisLazyLoader, ref m_StringValue); }
                    set { PocDomainRuntimeHelpers.PropertySetter(this, ref _thisLazyLoader, out m_StringValue, ref value); }
                }

                private PocDomain.IEntityOne m_One;
                private IPersistableObjectLazyLoader m_One_LazyLoader;
                public PocDomain.IEntityOne One
                {
                    get { return PocDomainRuntimeHelpers.LazyLoadObjectPropertyGetter(this, ref _thisLazyLoader, ref m_One_LazyLoader, ref m_One); }
                    set { PocDomainRuntimeHelpers.LazyLoadObjectPropertySetter(this, ref _thisLazyLoader, out m_One_LazyLoader, out m_One, ref value); }
                }

                #endregion
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public class DomainObject_EntityThree : PocDomain.IEntityThree, IPocDomainObject
            {
                private readonly IComponentContext _components;
                private IPersistableObjectLazyLoader _thisLazyLoader;
                private IPersistableObjectCollectionLazyLoader _foursLazyLoader;

                //---------------------------------------------------------------------------------------------------------------------------------------------

                public DomainObject_EntityThree(IComponentContext components)
                {
                    _components = components;
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                void IPocDomainObject.InitializeValues(bool idManuallyAssigned)
                {
                    m_Id = ObjectId.GenerateNewId();
                    m_StringValue = "ABC3";
                    m_Fours = new ConcreteToAbstractListAdapter<DomainObject_EntityFour, PocDomain.IEntityFour>(new List<DomainObject_EntityFour>());
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                void IPocImportExportValues.ImportValues(IPocEntityRepo entityRepo, object[] values)
                {
                    m_Id = (ObjectId)values[0];
                    m_StringValue = (string)values[1];
                    m_Fours = PocDomainRuntimeHelpers.ImportDomainLazyLoadObjectCollection<PocDomain.IEntityFour, DomainObject_EntityFour>(
                        entityRepo,
                        values[2],
                        () => new DomainObject_EntityFour(_components),
                        out _foursLazyLoader);
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                object[] IPocImportExportValues.ExportValues(IPocEntityRepo entityRepo)
                {
                    return new object[] {
                        m_Id, 
                        m_StringValue, 
                        m_Fours
                    };
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                object IPocDomainObject.EntityId
                {
                    get { return Id; }
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                void IPocDomainObject.SetLazyLoader(IPersistableObjectLazyLoader lazyLoader)
                {
                    _thisLazyLoader = lazyLoader;
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                #region Implementation of IEntityThree

                private ObjectId m_Id;
                public object Id
                {
                    get { return PocDomainRuntimeHelpers.EntityIdPropertyGetter(this, ref _thisLazyLoader, ref m_Id); }
                }

                private string m_StringValue;
                public string StringValue
                {
                    get { return PocDomainRuntimeHelpers.PropertyGetter(this, ref _thisLazyLoader, ref m_StringValue); }
                    set { PocDomainRuntimeHelpers.PropertySetter(this, ref _thisLazyLoader, out m_StringValue, ref value); }
                }


                private IList<PocDomain.IEntityFour> m_Fours;
                private IPersistableObjectCollectionLazyLoader m_Fours_LazyLoader;
                public ICollection<PocDomain.IEntityFour> Fours
                {
                    get { return PocDomainRuntimeHelpers.LazyLoadObjectCollectionPropertyGetter<PocDomain.IEntityFour, DomainObject_EntityFour>(this, ref _thisLazyLoader, ref m_Fours_LazyLoader, ref m_Fours); }
                }

                #endregion
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public class DomainObject_EntityFour : PocDomain.IEntityFour, IPocDomainObject
            {
                private readonly IComponentContext _components;
                private IPersistableObjectLazyLoader _thisLazyLoader;

                //---------------------------------------------------------------------------------------------------------------------------------------------

                public DomainObject_EntityFour(IComponentContext components)
                {
                    _components = components;
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                #region Implementation of IEntityFour

                private string m_Id;
                public string Id
                {
                    get { return PocDomainRuntimeHelpers.EntityIdPropertyGetter(this, ref _thisLazyLoader, ref m_Id); }
                    set { PocDomainRuntimeHelpers.PropertySetter(this, ref _thisLazyLoader, out m_Id, ref value); }
                }

                private string m_StringValue;
                public string StringValue
                {
                    get { return PocDomainRuntimeHelpers.PropertyGetter(this, ref _thisLazyLoader, ref m_StringValue); }
                    set { PocDomainRuntimeHelpers.PropertySetter(this, ref _thisLazyLoader, out m_StringValue, ref value); }
                }

                private PocDomain.IEntityThree m_Three;
                private IPersistableObjectLazyLoader m_Three_LazyLoader;
                public PocDomain.IEntityThree Three
                {
                    get { return PocDomainRuntimeHelpers.LazyLoadObjectPropertyGetter(this, ref _thisLazyLoader, ref m_Three_LazyLoader, ref m_Three); }
                    set { PocDomainRuntimeHelpers.LazyLoadObjectPropertySetter(this, ref _thisLazyLoader, out m_Three_LazyLoader, out m_Three, ref value); }
                }

                #endregion

                //---------------------------------------------------------------------------------------------------------------------------------------------

                void IPocDomainObject.InitializeValues(bool idManuallyAssigned)
                {
                    m_Id = null;
                    m_StringValue = "ABC4";
                    m_Three = null;
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                void IPocImportExportValues.ImportValues(IPocEntityRepo entityRepo, object[] values)
                {
                    m_Id = (string)values[0];
                    m_StringValue = (string)values[1];
                    m_Three = PocDomainRuntimeHelpers.ImportDomainLazyLoadObject<PocDomain.IEntityThree, DomainObject_EntityThree>(
                        entityRepo,
                        values[2],
                        () => new DomainObject_EntityThree(_components));
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                object[] IPocImportExportValues.ExportValues(IPocEntityRepo entityRepo)
                {
                    return new object[] {
                        m_Id, 
                        m_StringValue, 
                        m_Three
                    };
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                void IPocDomainObject.SetLazyLoader(IPersistableObjectLazyLoader lazyLoader)
                {
                    _thisLazyLoader = lazyLoader;
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                object IPocDomainObject.EntityId
                {
                    get { return this.Id; }
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public class DomainObject_ValueObjectA : PocDomain.IValueObjectA, IPocDomainObject
            {
                private readonly IComponentContext _components;
                private IPersistableObjectLazyLoader _thisLazyLoader;

                //---------------------------------------------------------------------------------------------------------------------------------------------

                public DomainObject_ValueObjectA(IComponentContext components)
                {
                    _components = components;
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                void IPocDomainObject.InitializeValues(bool idManuallyAssigned)
                {
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                void IPocImportExportValues.ImportValues(IPocEntityRepo entityRepo, object[] values)
                {
                    m_TimeSpanScalar = (TimeSpan)values[0];
                    m_DateTimeScalar = (DateTime)values[1];
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                void IPocDomainObject.SetLazyLoader(IPersistableObjectLazyLoader lazyLoader)
                {
                    _thisLazyLoader = lazyLoader;
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                object[] IPocImportExportValues.ExportValues(IPocEntityRepo entityRepo)
                {
                    return new object[] { m_TimeSpanScalar, m_DateTimeScalar };
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                object IPocDomainObject.EntityId
                {
                    get { return null; }
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                #region Implementation of IValueObjectA

                private TimeSpan m_TimeSpanScalar;
                public TimeSpan TimeSpanScalar
                {
                    get { return PocDomainRuntimeHelpers.PropertyGetter(this, ref _thisLazyLoader, ref m_TimeSpanScalar); }
                    set { PocDomainRuntimeHelpers.PropertySetter(this, ref _thisLazyLoader, out m_TimeSpanScalar, ref value); }
                }

                private DateTime m_DateTimeScalar;
                public DateTime DateTimeScalar
                {
                    get { return PocDomainRuntimeHelpers.PropertyGetter(this, ref _thisLazyLoader, ref m_DateTimeScalar); }
                    set { PocDomainRuntimeHelpers.PropertySetter(this, ref _thisLazyLoader, out m_DateTimeScalar, ref value); }
                }

                #endregion
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public class DomainObject_ValueObjectB : PocDomain.IValueObjectB, IPocDomainObject
            {
                private readonly IComponentContext _components;
                private IPersistableObjectLazyLoader _thisLazyLoader;

                //---------------------------------------------------------------------------------------------------------------------------------------------

                public DomainObject_ValueObjectB(IComponentContext components)
                {
                    _components = components;
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                void IPocDomainObject.InitializeValues(bool idManuallyAssigned)
                {
                    m_ValueCs = new ConcreteToAbstractListAdapter<DomainObject_ValueObjectC, PocDomain.IValueObjectC>(new List<DomainObject_ValueObjectC>());
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                void IPocImportExportValues.ImportValues(IPocEntityRepo entityRepo, object[] values)
                {
                    m_GuidScalar = (Guid)values[0];
                    m_EnumScalar = (DayOfWeek)values[1];
                    m_ValueCs = PocDomainRuntimeHelpers.ImportEmbeddedDomainCollection<PocDomain.IValueObjectC, DomainObject_ValueObjectC>(
                        entityRepo,
                        values[2],
                        () => new DomainObject_ValueObjectC(_components));
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                object[] IPocImportExportValues.ExportValues(IPocEntityRepo entityRepo)
                {
                    return new object[] { m_GuidScalar, m_EnumScalar, m_ValueCs };
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                void IPocDomainObject.SetLazyLoader(IPersistableObjectLazyLoader lazyLoader)
                {
                    _thisLazyLoader = lazyLoader;
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                object IPocDomainObject.EntityId
                {
                    get { return null; }
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                #region Implementation of IValueObjectB

                private Guid m_GuidScalar;
                public Guid GuidScalar
                {
                    get { return PocDomainRuntimeHelpers.PropertyGetter(this, ref _thisLazyLoader, ref m_GuidScalar); }
                    set { PocDomainRuntimeHelpers.PropertySetter(this, ref _thisLazyLoader, out m_GuidScalar, ref value); }
                }

                private DayOfWeek m_EnumScalar;
                public DayOfWeek EnumScalar
                {
                    get { return PocDomainRuntimeHelpers.PropertyGetter(this, ref _thisLazyLoader, ref m_EnumScalar); }
                    set { PocDomainRuntimeHelpers.PropertySetter(this, ref _thisLazyLoader, out m_EnumScalar, ref value); }
                }

                private IList<PocDomain.IValueObjectC> m_ValueCs;
                public IList<PocDomain.IValueObjectC> ValueCs
                {
                    get { return PocDomainRuntimeHelpers.PropertyGetter(this, ref _thisLazyLoader, ref m_ValueCs); }
                }

                #endregion
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public class DomainObject_ValueObjectC : PocDomain.IValueObjectC, IPocDomainObject
            {
                private readonly IComponentContext _components;
                private IPersistableObjectLazyLoader _thisLazyLoader;

                //---------------------------------------------------------------------------------------------------------------------------------------------

                public DomainObject_ValueObjectC(IComponentContext components)
                {
                    _components = components;
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                void IPocDomainObject.InitializeValues(bool idManuallyAssigned)
                {
                    m_Fours = new List<PocDomain.IEntityFour>();
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                void IPocImportExportValues.ImportValues(IPocEntityRepo entityRepo, object[] values)
                {
                    m_FlagsEnumScalar = (PocDomain.AFlagsEnum)values[0];
                    m_DecimalValue = (decimal)values[1];
                    m_Three = PocDomainRuntimeHelpers.ImportDomainLazyLoadObject<PocDomain.IEntityThree, DomainObject_EntityThree>(
                        entityRepo,
                        values[2],
                        () => new DomainObject_EntityThree(_components));
                    m_Fours = PocDomainRuntimeHelpers.ImportDomainLazyLoadObjectCollection<PocDomain.IEntityFour, DomainObject_EntityFour>(
                        entityRepo,
                        values[3],
                        () => new DomainObject_EntityFour(_components),
                        out m_Fours_LazyLoader);
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                object[] IPocImportExportValues.ExportValues(IPocEntityRepo entityRepo)
                {
                    return new object[] {
                        m_FlagsEnumScalar, 
                        m_DecimalValue, 
                        m_Three, 
                        m_Fours
                    };
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                void IPocDomainObject.SetLazyLoader(IPersistableObjectLazyLoader lazyLoader)
                {
                    _thisLazyLoader = lazyLoader;
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                object IPocDomainObject.EntityId
                {
                    get { return null; }
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                #region Implementation of IValueObjectC

                private PocDomain.AFlagsEnum m_FlagsEnumScalar;
                public PocDomain.AFlagsEnum FlagsEnumScalar
                {
                    get { return PocDomainRuntimeHelpers.PropertyGetter(this, ref _thisLazyLoader, ref m_FlagsEnumScalar); }
                    set { PocDomainRuntimeHelpers.PropertySetter(this, ref _thisLazyLoader, out m_FlagsEnumScalar, ref value); }
                }

                private decimal m_DecimalValue;
                public decimal DecimalValue
                {
                    get { return PocDomainRuntimeHelpers.PropertyGetter(this, ref _thisLazyLoader, ref m_DecimalValue); }
                    set { PocDomainRuntimeHelpers.PropertySetter(this, ref _thisLazyLoader, out m_DecimalValue, ref value); }
                }

                private PocDomain.IEntityThree m_Three;
                private IPersistableObjectLazyLoader m_Three_LazyLoader;
                public PocDomain.IEntityThree Three
                {
                    get { return PocDomainRuntimeHelpers.LazyLoadObjectPropertyGetter(this, ref _thisLazyLoader, ref m_Three_LazyLoader, ref m_Three); }
                    set { PocDomainRuntimeHelpers.LazyLoadObjectPropertySetter(this, ref _thisLazyLoader, out m_Three_LazyLoader, out m_Three, ref value); }
                }

                private IList<PocDomain.IEntityFour> m_Fours;
                private IPersistableObjectCollectionLazyLoader m_Fours_LazyLoader;
                public ICollection<PocDomain.IEntityFour> Fours
                {
                    get { return PocDomainRuntimeHelpers.LazyLoadObjectCollectionPropertyGetter<PocDomain.IEntityFour, DomainObject_EntityFour>(this, ref _thisLazyLoader, ref m_Fours_LazyLoader, ref m_Fours); }
                }

                #endregion
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public class PocMongoDomainContext : IPocDomainContext, IPocMongoDomainContext, PocDomain.IMyPocContext
            {
                private readonly IComponentContext _components;
                private readonly string _connectionString;
                private readonly MongoDatabase _database;
                private readonly Dictionary<Type, IPocEntityRepo> _repoByContract;

                //---------------------------------------------------------------------------------------------------------------------------------------------

                public PocMongoDomainContext(IComponentContext components, string connectionString)
                {
                    _connectionString = connectionString;
                    _components = components;

                    var connectionStringBuilder = new MongoConnectionStringBuilder(connectionString);
                    var client = new MongoClient(connectionStringBuilder.ConnectionString);
                    var server = client.GetServer();

                    _database = server.GetDatabase(connectionStringBuilder.DatabaseName);
                    _repoByContract = new Dictionary<Type, IPocEntityRepo>();

                    this.Ones = new PocMongoEntityRepo<PocDomain.IEntityOne, DomainImpl.DomainObject_EntityOne, MongoPersistable_EntityOne>(
                        this,
                        "EntityOne",
                        () => new DomainImpl.DomainObject_EntityOne(_components),
                        () => new MongoPersistable_EntityOne());

                    this.Twos = new PocMongoEntityRepo<PocDomain.IEntityTwo, DomainImpl.DomainObject_EntityTwo, MongoPersistable_EntityTwo>(
                        this,
                        "EntityTwo",
                        () => new DomainImpl.DomainObject_EntityTwo(_components),
                        () => new MongoPersistable_EntityTwo());

                    this.Threes = new PocMongoEntityRepo<PocDomain.IEntityThree, DomainImpl.DomainObject_EntityThree, MongoPersistable_EntityThree>(
                        this,
                        "EntityThree",
                        () => new DomainImpl.DomainObject_EntityThree(_components),
                        () => new MongoPersistable_EntityThree());

                    this.Fours = new PocMongoEntityRepo<PocDomain.IEntityFour, DomainImpl.DomainObject_EntityFour, MongoPersistable_EntityFour>(
                        this,
                        "EntityFour",
                        () => new DomainImpl.DomainObject_EntityFour(_components),
                        () => new MongoPersistable_EntityFour());


                    _repoByContract[typeof(PocDomain.IEntityOne)] = (IPocEntityRepo)this.Ones;
                    _repoByContract[typeof(PocDomain.IEntityTwo)] = (IPocEntityRepo)this.Twos;
                    _repoByContract[typeof(PocDomain.IEntityThree)] = (IPocEntityRepo)this.Threes;
                    _repoByContract[typeof(PocDomain.IEntityFour)] = (IPocEntityRepo)this.Fours;

                    PocThreadStaticRepoStack.Push(this);
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                public void Dispose()
                {
                    PocThreadStaticRepoStack.Pop(this);
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                bool IPocDomainContext.TryGetEntityFromCache(Type contractType, object id, out IPocDomainObject entity)
                {
                    var repo = _repoByContract[contractType];
                    return repo.TryGetEntityFromCache(id, out entity);
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                public IPocEntityRepo GetEntityRepo(Type contractType)
                {
                    return _repoByContract[contractType];
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                MongoDatabase IPocMongoDomainContext.Database
                {
                    get { return _database; }
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                IComponentContext IPocDomainContext.Components
                {
                    get { return _components; }
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                public IPocEntityRepo<PocDomain.IEntityOne> Ones { get; private set; }
                public IPocEntityRepo<PocDomain.IEntityTwo> Twos { get; private set; }
                public IPocEntityRepo<PocDomain.IEntityThree> Threes { get; private set; }
                public IPocEntityRepo<PocDomain.IEntityFour> Fours { get; private set; }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                PocDomain.IEntityFour PocDomain.IMyPocContext.NewEntityFour(string id)
                {
                    var obj = new DomainObject_EntityFour(_components);
                    ((IPocDomainObject)obj).InitializeValues(idManuallyAssigned: true);

                    obj.Id = id;
                    return obj;
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                PocDomain.IValueObjectB PocDomain.IMyPocContext.NewValueObjectB(Guid guidScalar, DayOfWeek enumScalar, params PocDomain.IValueObjectC[] valueCs)
                {
                    var obj = new DomainObject_ValueObjectB(_components);
                    ((IPocDomainObject)obj).InitializeValues(idManuallyAssigned: false);

                    obj.GuidScalar = guidScalar;
                    obj.EnumScalar = enumScalar;

                    foreach ( var c in valueCs )
                    {
                        obj.ValueCs.Add(c);
                    }

                    return obj;
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                PocDomain.IValueObjectC PocDomain.IMyPocContext.NewValueObjectC(PocDomain.AFlagsEnum flagsEnumScalar, decimal decimalValue, PocDomain.IEntityThree three, params PocDomain.IEntityFour[] fours)
                {
                    var obj = new DomainObject_ValueObjectC(_components);
                    ((IPocDomainObject)obj).InitializeValues(idManuallyAssigned: false);

                    obj.FlagsEnumScalar = flagsEnumScalar;
                    obj.DecimalValue = decimalValue;
                    obj.Three = three;

                    foreach ( var four in fours )
                    {
                        obj.Fours.Add(four);
                    }

                    return obj;
                }


            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            [BsonDiscriminator("EntityOne", Required = true)]
            public class MongoPersistable_EntityOne : IPocPersistableObject
            {
                object IPocPersistableObject.EntityId
                {
                    get
                    {
                        return Id;
                    }
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                Type IPocPersistableObject.PocHintDomainImplType
                {
                    //get { return typeof(PocDomain.IEntityOne); }
                    get { return typeof(DomainObject_EntityOne); }
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                void IPocImportExportValues.ImportValues(IPocEntityRepo entityRepo, object[] values)
                {
                    Id = (int)values[0];
                    IntScalar = (int)values[1];
                    StringScalar = (string)values[2];
                    ValueA = PocDomainRuntimeHelpers.ImportEmbeddedPersistableObject<MongoPersistable_ValueObjectA>(entityRepo, values[3]);
                    ValueBs = PocDomainRuntimeHelpers.ImportEmbeddedPersistableCollection<MongoPersistable_ValueObjectB>(entityRepo, values[4]);
                    // [5] is ignored, connected through inverse foreign key
                    Threes = PocMongoRuntimeHelpers.ImportPersistableLazyLoadObjectCollection<ObjectId>(values[6]);
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                object[] IPocImportExportValues.ExportValues(IPocEntityRepo entityRepo)
                {
                    return new object[] {
                        Id, 
                        IntScalar,
                        StringScalar,
                        ValueA,
                        ValueBs,
                        new ObjectLazyLoaderByForeignKey(entityRepo.GetOwnerContext().GetEntityRepo(typeof(PocDomain.IEntityTwo)), "One", Id),
                        new ObjectCollectionLazyLoaderById<ObjectId>(entityRepo.GetOwnerContext().GetEntityRepo(typeof(PocDomain.IEntityThree)), Threes), 
                    };
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                #region Implementation of IEntityOne

                [BsonId]
                public int Id { get; set; }
                
                public int IntScalar { get; set; }
                public string StringScalar { get; set; }
                public MongoPersistable_ValueObjectA ValueA { get; set; }
                public MongoPersistable_ValueObjectB[] ValueBs { get; set; }
                public ObjectId[] Threes { get; set; }

                #endregion
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            [BsonDiscriminator("EntityTwo", Required = true)]
            public class MongoPersistable_EntityTwo : IPocPersistableObject
            {
                object IPocPersistableObject.EntityId
                {
                    get
                    {
                        return Id;
                    }
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                Type IPocPersistableObject.PocHintDomainImplType
                {
                    //get { return typeof(PocDomain.IEntityTwo); }
                    get { return typeof(DomainObject_EntityTwo); }
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                void IPocImportExportValues.ImportValues(IPocEntityRepo entityRepo, object[] values)
                {
                    Id = (ObjectId)values[0];
                    StringValue = (string)values[1];
                    One = PocMongoRuntimeHelpers.ImportPersistableLazyLoadObjectNullable<int>(values[2]);
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                object[] IPocImportExportValues.ExportValues(IPocEntityRepo entityRepo)
                {
                    return new object[] {
                        Id,
                        StringValue,
                        One.HasValue ? new ObjectLazyLoaderById(entityRepo, One.Value) : null, 
                    };
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                #region Implementation of IEntityTwo

                [BsonId]
                public ObjectId Id { get; set; }
                
                public string StringValue { get; set; }
                public int? One { get; set; }

                #endregion
            }
            
            //-------------------------------------------------------------------------------------------------------------------------------------------------

            [BsonDiscriminator("EntityThree", Required = true)]
            public class MongoPersistable_EntityThree : IPocPersistableObject
            {
                object IPocPersistableObject.EntityId
                {
                    get { return Id; }
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                Type IPocPersistableObject.PocHintDomainImplType
                {
                    //get { return typeof(PocDomain.IEntityThree); }
                    get { return typeof(DomainObject_EntityThree); }
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                void IPocImportExportValues.ImportValues(IPocEntityRepo entityRepo, object[] values)
                {
                    Id = (ObjectId)values[0];
                    StringValue = (string)values[1];
                    // [2] is ignored: referenced by inverse foreign key
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                object[] IPocImportExportValues.ExportValues(IPocEntityRepo entityRepo)
                {
                    return new object[] {
                        Id,
                        StringValue,
                        new ObjectCollectionLazyLoaderByForeignKey(entityRepo.GetOwnerContext().GetEntityRepo(typeof(PocDomain.IEntityFour)), "Three", Id), 
                    };
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                #region Implementation of IEntityThree

                [BsonId]
                public ObjectId Id { get; set; }

                public string StringValue { get; set; }

                #endregion
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            [BsonDiscriminator("EntityFour", Required = true)]
            public class MongoPersistable_EntityFour : IPocPersistableObject
            {
                object IPocPersistableObject.EntityId
                {
                    get { return Id; }
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                Type IPocPersistableObject.PocHintDomainImplType
                {
                    //get { return typeof(PocDomain.IEntityFour); }
                    get { return typeof(DomainObject_EntityFour); }
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                void IPocImportExportValues.ImportValues(IPocEntityRepo entityRepo, object[] values)
                {
                    Id = (string)values[0];
                    StringValue = (string)values[1];
                    Three = PocMongoRuntimeHelpers.ImportPersistableLazyLoadObjectNullable<ObjectId>(values[2]);
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                object[] IPocImportExportValues.ExportValues(IPocEntityRepo entityRepo)
                {
                    return new object[] {
                        Id,
                        StringValue,
                        Three.HasValue ? new ObjectLazyLoaderById(entityRepo.GetOwnerContext().GetEntityRepo(typeof(PocDomain.IEntityThree)), Three.Value) : null, 
                    };
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                #region Implementation of IEntityFour

                [BsonId]
                public string Id { get; set; }
                
                public string StringValue { get; set; }
                public ObjectId? Three { get; set; }

                #endregion
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            [BsonDiscriminator("ValueObjectA", Required = true)]
            public class MongoPersistable_ValueObjectA : IPocPersistableObject
            {
                object IPocPersistableObject.EntityId
                {
                    get { return null; }
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                Type IPocPersistableObject.PocHintDomainImplType
                {
                    //get { return typeof(PocDomain.IValueObjectA); }
                    get { return typeof(DomainObject_ValueObjectA); }
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                void IPocImportExportValues.ImportValues(IPocEntityRepo entityRepo, object[] values)
                {
                    TimeSpanScalar = (TimeSpan)values[0];

                    var v1 = (DateTime)values[1];
                    DateTimeScalar = new DateTime(v1.Year, v1.Month, v1.Day, v1.Hour, v1.Minute, v1.Second, v1.Millisecond, DateTimeKind.Utc);
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                object[] IPocImportExportValues.ExportValues(IPocEntityRepo entityRepo)
                {
                    return new object[] {
                        TimeSpanScalar,
                        DateTimeScalar
                    };
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                #region Implementation of IValueObjectA

                public TimeSpan TimeSpanScalar { get; set; }

                [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
                public DateTime DateTimeScalar { get; set; }

                #endregion
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            [BsonDiscriminator("ValueObjectB", Required = true)]
            public class MongoPersistable_ValueObjectB : IPocPersistableObject
            {
                object IPocPersistableObject.EntityId
                {
                    get { return null; }
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                Type IPocPersistableObject.PocHintDomainImplType
                {
                    //get { return typeof(PocDomain.IValueObjectB); }
                    get { return typeof(DomainObject_ValueObjectB); }
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                void IPocImportExportValues.ImportValues(IPocEntityRepo entityRepo, object[] values)
                {
                    GuidScalar = (Guid)values[0];
                    EnumScalar = (DayOfWeek)values[1];
                    ValueCs = PocDomainRuntimeHelpers.ImportEmbeddedPersistableCollection<MongoPersistable_ValueObjectC>(entityRepo, values[2]);
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                object[] IPocImportExportValues.ExportValues(IPocEntityRepo entityRepo)
                {
                    return new object[] {
                        GuidScalar,
                        EnumScalar,
                        ValueCs
                    };
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                #region Implementation of IValueObjectB

                public Guid GuidScalar { get; set; }
                
                [BsonRepresentation(BsonType.String)]
                public DayOfWeek EnumScalar { get; set; }
                
                public MongoPersistable_ValueObjectC[] ValueCs { get; set; }

                #endregion
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            [BsonDiscriminator("ValueObjectC", Required = true)]
            public class MongoPersistable_ValueObjectC : IPocPersistableObject
            {
                object IPocPersistableObject.EntityId
                {
                    get { return null; }
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                Type IPocPersistableObject.PocHintDomainImplType
                {
                    //get { return typeof(PocDomain.IValueObjectC); }
                    get { return typeof(DomainObject_ValueObjectC); }
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                void IPocImportExportValues.ImportValues(IPocEntityRepo entityRepo, object[] values)
                {
                    FlagsEnumScalar = (PocDomain.AFlagsEnum)values[0];
                    DecimalValue = (decimal)values[1];
                    Three = PocMongoRuntimeHelpers.ImportPersistableLazyLoadObjectNullable<ObjectId>(values[2]);
                    Fours = PocMongoRuntimeHelpers.ImportPersistableLazyLoadObjectCollection<string>(values[3]);
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                object[] IPocImportExportValues.ExportValues(IPocEntityRepo entityRepo)
                {
                    return new object[] {
                        FlagsEnumScalar,
                        DecimalValue,
                        Three.HasValue ? new ObjectLazyLoaderById(entityRepo.GetOwnerContext().GetEntityRepo(typeof(PocDomain.IEntityThree)), Three) : null, 
                        new ObjectCollectionLazyLoaderById<string>(entityRepo.GetOwnerContext().GetEntityRepo(typeof(PocDomain.IEntityFour)), Fours), 
                    };
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                #region Implementation of IValueObjectC

                [BsonRepresentation(BsonType.String)]
                public PocDomain.AFlagsEnum FlagsEnumScalar { get; set; }

                public decimal DecimalValue { get; set; }
                public ObjectId? Three { get; set; }
                public string[] Fours { get; set; }

                #endregion
            }
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    namespace PocFramework
    {
        public interface IPocDomainObject : IPocImportExportValues
        {
            void InitializeValues(bool idManuallyAssigned);
            void SetLazyLoader(IPersistableObjectLazyLoader lazyLoader);
            object EntityId { get; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface IPocPersistableObject : IPocImportExportValues
        {
            object EntityId { get; }
            Type PocHintDomainImplType { get; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public  interface IPersistableObjectLazyLoader
        {
            IPocDomainObject Load(IPocDomainObject target);
            Type EntityContractType { get; }
            Type DomainContextType { get; }
            object EntityId { get; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public  interface IPersistableObjectCollectionLazyLoader
        {
            IEnumerable<IPocDomainObject> Load();
            Type EntityContractType { get; }
            Type DomainContextType { get; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface IPocImportExportValues
        {
            void ImportValues(IPocEntityRepo entityRepo, object[] values);
            object[] ExportValues(IPocEntityRepo entityRepo);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public  interface IPocEntityRepo
        {
            IPocDomainContext GetOwnerContext();
            IPocDomainObject NewDomainObject(Type pocHintDerivedDomainImplType = null);
            bool TryGetEntityFromCache(object id, out IPocDomainObject entity);
            Type ContractType { get; }
            Type PersistableType { get; }
            Type DomainType { get; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public  interface IPocEntityRepo<TEntity>
        {
            TEntity New();
            T New<T>(Type pocHintDomainImplType) where T : TEntity;
            void Insert(TEntity entity);
            TEntity GetById(object id);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public  interface IPocDomainContext : IDisposable
        {
            bool TryGetEntityFromCache(Type contractType, object id, out IPocDomainObject entity);
            IPocEntityRepo GetEntityRepo(Type contractType);
            //MongoDatabase Database { get; }
            IComponentContext Components { get; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static class PocThreadStaticRepoStack
        {
            [ThreadStatic]
            private static Dictionary<Type, Stack<IPocDomainContext>> _s_stackByImplType;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public static void Push(IPocDomainContext instance)
            {
                if ( _s_stackByImplType == null )
                {
                    _s_stackByImplType = new Dictionary<Type, Stack<IPocDomainContext>>();
                }

                var stack = GetOrAddStack(instance.GetType());
                stack.Push(instance);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public static IPocDomainContext Peek(Type implType)
            {
                if ( _s_stackByImplType == null )
                {
                    throw new InvalidOperationException("ThreadStaticRepositoryStack is empty.");
                }

                var stack = GetOrAddStack(implType);
                return (stack.Count > 0 ? stack.Peek() : null);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public static IPocDomainContext PeekOrCreate(Type implType)
            {
                if ( _s_stackByImplType == null )
                {
                    throw new InvalidOperationException("ThreadStaticRepositoryStack is empty.");
                }

                var stack = GetOrAddStack(implType);
                return (stack.Count > 0 ? stack.Peek() : null);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public static void Pop(IPocDomainContext instance)
            {
                var stack = GetOrAddStack(instance.GetType());
                var popped = stack.Pop();

                if ( !_s_stackByImplType.Values.Any(s => s.Count > 0) )
                {
                    _s_stackByImplType = null;
                }

                if ( popped != instance )
                {
                    throw new InvalidOperationException("ThreadStaticRepositoryStack Push/Pop mismatch.");
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private static Stack<IPocDomainContext> GetOrAddStack(Type implType)
            {
                return _s_stackByImplType.GetOrAdd(implType, t => new Stack<IPocDomainContext>());
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static class PocDomainRuntimeHelpers
        {
            public static TContract ImportEmbeddedDomainObject<TContract, TImpl>(IPocEntityRepo entityRepo, object importValue, Func<TImpl> implFactory) 
                where TImpl : TContract
            {
                var impl = implFactory();
                var persisted = ((IPocImportExportValues)importValue);
                if ( persisted != null )
                {
                    ((IPocDomainObject)impl).ImportValues(entityRepo, persisted.ExportValues(entityRepo));
                }
                else
                {
                    ((IPocDomainObject)impl).InitializeValues(idManuallyAssigned: false);
                }
                return impl;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public static IList<TContract> ImportEmbeddedDomainCollection<TContract, TImpl>(IPocEntityRepo entityRepo, object importValue, Func<TImpl> itemImplFactory)
                where TImpl : TContract
            {
                var persistedCollection = (IEnumerable<IPocPersistableObject>)importValue;
                var concrete = new List<TImpl>();

                if ( persistedCollection != null )
                {
                    concrete.AddRange(persistedCollection.Select(persistedItem => {
                        var impl = itemImplFactory();
                        if ( persistedItem != null )
                        {
                            ((IPocDomainObject)impl).ImportValues(entityRepo, persistedItem.ExportValues(entityRepo));
                        }
                        else
                        {
                            ((IPocDomainObject)impl).InitializeValues(idManuallyAssigned: false);
                        }
                        return impl;
                    }));
                }
                return new ConcreteToAbstractListAdapter<TImpl, TContract>(concrete);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public static TContract ImportDomainLazyLoadObject<TContract, TImpl>(IPocEntityRepo entityRepo, object importValue, Func<TImpl> implFactory)
                where TContract : class
                where TImpl : TContract
            {
                var persisted = (importValue as IPocPersistableObject);
                var lazyLoader = (importValue as IPersistableObjectLazyLoader);

                if ( persisted != null )
                {
                    var impl = implFactory();
                    ((IPocDomainObject)impl).ImportValues(entityRepo, persisted.ExportValues(entityRepo));
                    return impl;
                }
                else if ( lazyLoader != null )
                {
                    var impl = implFactory();
                    ((IPocDomainObject)impl).SetLazyLoader(lazyLoader);
                    return impl;
                }
                else
                {
                    return null;
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public static IList<TContract> ImportDomainLazyLoadObjectCollection<TContract, TImpl>(
                IPocEntityRepo entityRepo, 
                object importValue, 
                Func<TImpl> itemImplFactory, 
                out IPersistableObjectCollectionLazyLoader lazyLoader)
                where TImpl : TContract
            {
                var persistedCollection = (importValue as IEnumerable<IPocPersistableObject>);
                lazyLoader = (importValue as IPersistableObjectCollectionLazyLoader);

                if ( persistedCollection != null )
                {
                    var concrete = new List<TImpl>();

                    concrete.AddRange(persistedCollection.Select(persistedItem => {
                        var itemImpl = itemImplFactory();
                        if ( persistedItem != null )
                        {
                            ((IPocDomainObject)itemImpl).ImportValues(entityRepo, persistedItem.ExportValues(entityRepo));
                        }
                        else
                        {
                            ((IPocDomainObject)itemImpl).InitializeValues(idManuallyAssigned: false);
                        }
                        return itemImpl;
                    }));

                    return new ConcreteToAbstractListAdapter<TImpl, TContract>(concrete);
                }
                else
                {
                    return null;
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public static TPersistable ImportEmbeddedPersistableObject<TPersistable>(IPocEntityRepo entityRepo, object importValue)
                where TPersistable : class, IPocPersistableObject, new()
            {
                var domain = ((IPocImportExportValues)importValue);
                if ( domain != null )
                {
                    var persistable = new TPersistable();
                    persistable.ImportValues(entityRepo, domain.ExportValues(entityRepo));
                    return persistable;
                }
                else
                {
                    return null;
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public static TPersistable[] ImportEmbeddedPersistableCollection<TPersistable>(IPocEntityRepo entityRepo, object importValue)
                where TPersistable : class, IPocPersistableObject, new()
            {
                var domainCollection = (System.Collections.IEnumerable)importValue;

                if ( domainCollection == null )
                {
                    return null;
                }
                
                var persistableCollection = domainCollection.Cast<IPocDomainObject>().Select(domainItem => {
                    if ( domainItem != null )
                    {
                        var persistable = new TPersistable();
                        persistable.ImportValues(entityRepo, domainItem.ExportValues(entityRepo));
                        return persistable;
                    }
                    else
                    {
                        return null;
                    }
                }).ToArray();

                return persistableCollection;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public static T EntityIdPropertyGetter<T>(
                IPocDomainObject target,
                ref IPersistableObjectLazyLoader targetLazyLoader,
                ref T backingField)
            {
                if ( targetLazyLoader != null )
                {
                    var entityIdValue = targetLazyLoader.EntityId;

                    if ( entityIdValue != null )
                    {
                        return (T)entityIdValue;
                    }

                    targetLazyLoader.Load(target);
                    targetLazyLoader = null;
                }

                return backingField;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public static T PropertyGetter<T>(
                IPocDomainObject target, 
                ref IPersistableObjectLazyLoader targetLazyLoader, 
                ref T backingField)
            {
                if ( targetLazyLoader != null )
                {
                    targetLazyLoader.Load(target);
                    targetLazyLoader = null;
                }

                return backingField;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public static void PropertySetter<T>(
                IPocDomainObject target,
                ref IPersistableObjectLazyLoader targetLazyLoader,
                out T backingField,
                ref T value)
            {
                if ( targetLazyLoader != null )
                {
                    targetLazyLoader.Load(target);
                    targetLazyLoader = null;
                }

                backingField = value;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public static T LazyLoadObjectPropertyGetter<T>(
                IPocDomainObject target,
                ref IPersistableObjectLazyLoader targetLazyLoader,
                ref IPersistableObjectLazyLoader propertyLazyLoader,
                ref T propertyBackingField)
            {
                if ( targetLazyLoader != null )
                {
                    targetLazyLoader.Load(target);
                    targetLazyLoader = null;
                }

                if ( propertyLazyLoader != null )
                {
                    propertyBackingField = (T)propertyLazyLoader.Load((IPocDomainObject)propertyBackingField);
                    propertyLazyLoader = null;
                }

                return propertyBackingField;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public static void LazyLoadObjectPropertySetter<T>(
                IPocDomainObject target,
                ref IPersistableObjectLazyLoader targetLazyLoader,
                out IPersistableObjectLazyLoader propertyLazyLoader,
                out T propertyBackingField,
                ref T value)
            {
                if ( targetLazyLoader != null )
                {
                    targetLazyLoader.Load(target);
                    targetLazyLoader = null;
                }

                propertyLazyLoader = null;
                propertyBackingField = value;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public static IList<TContract> LazyLoadObjectCollectionPropertyGetter<TContract, TDomainImpl>(
                IPocDomainObject target,
                ref IPersistableObjectLazyLoader targetLazyLoader,
                ref IPersistableObjectCollectionLazyLoader propertyLazyLoader,
                ref IList<TContract> propertyBackingField)
                where TDomainImpl : TContract
            {
                if ( targetLazyLoader != null )
                {
                    targetLazyLoader.Load(target);
                    targetLazyLoader = null;
                }

                if ( propertyLazyLoader != null )
                {
                    propertyBackingField = new ConcreteToAbstractListAdapter<TDomainImpl, TContract>(propertyLazyLoader.Load().Cast<TDomainImpl>().ToList());
                    propertyLazyLoader = null;
                }

                return propertyBackingField;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public static IList<TContract> LazyLoadObjectCollection<TContract, TImpl>(
                IPocEntityRepo entityRepo, 
                Func<TImpl> itemImplFactory,
                ref IPersistableObjectCollectionLazyLoader lazyLoader)
                where TImpl : TContract
            {
                var persistedCollection = lazyLoader.Load();
                var concrete = new List<TImpl>();

                if ( persistedCollection != null )
                {
                    concrete.AddRange(persistedCollection.Select(persistedItem => {
                        var itemImpl = itemImplFactory();
                        ((IPocDomainObject)itemImpl).ImportValues(entityRepo, persistedItem.ExportValues(entityRepo));
                        return itemImpl;
                    }));
                }

                lazyLoader = null;
                return new ConcreteToAbstractListAdapter<TImpl, TContract>(concrete);
            }
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    namespace PocMongoStack
    {
        using PocFramework;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface IPocMongoDomainContext
        {
            MongoDatabase Database { get; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface IPocMongoEntityRepo
        {
            MongoCollection GetMongoCollection();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static class IntIdGenerator
        {
            private static int _s_lastValue = 0;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public static int TakeNextValue()
            {
                return Interlocked.Increment(ref _s_lastValue);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static class PocMongoRuntimeHelpers
        {
            public static TId ImportPersistableLazyLoadObject<TId>(object importValue)
            {
                var domainObject = importValue as IPocDomainObject;
                var lazyLoaderById = importValue as ObjectLazyLoaderById;

                if ( domainObject != null )
                {
                    return (TId)domainObject.EntityId;
                }

                if ( lazyLoaderById != null )
                {
                    return (TId)lazyLoaderById.EntityId;
                }

                return default(TId);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public static TId? ImportPersistableLazyLoadObjectNullable<TId>(object importValue)
                where TId : struct
            {
                var domainObject = importValue as IPocDomainObject;
                var lazyLoaderById = importValue as ObjectLazyLoaderById;

                if ( domainObject != null )
                {
                    return (TId)domainObject.EntityId;
                }

                if ( lazyLoaderById != null )
                {
                    return (TId)lazyLoaderById.EntityId;
                }

                return null;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public static TId[] ImportPersistableLazyLoadObjectCollection<TId>(object importValue)
            {
                var domainCollection = importValue as System.Collections.IEnumerable;
                var lazyLoaderByIdList = importValue as ObjectCollectionLazyLoaderById<TId>;

                if ( domainCollection != null )
                {
                    return domainCollection.Cast<IPocDomainObject>().Select(obj => (TId)obj.EntityId).ToArray();
                }

                if ( lazyLoaderByIdList != null )
                {
                    return lazyLoaderByIdList.DocumentIds;
                }

                return null;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static class PocQueryLog
        {
            private static List<string> _s_log = new List<string>();

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public static void LogQuery<TEntity>(object id)
            {
                _s_log.Add(string.Format("{0}[{1}]", typeof(TEntity).Name, id));
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public static void LogQuery(string format, params object[] args)
            {
                _s_log.Add(string.Format(format, args));
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public static string[] GetLog()
            {
                return _s_log.ToArray();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public static string[] TakeLog()
            {
                var log = _s_log.ToArray();
                _s_log.Clear();
                return log;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public static void Clear()
            {
                _s_log.Clear();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public static int Count
            {
                get { return _s_log.Count; }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class ObjectLazyLoaderById : IPersistableObjectLazyLoader
        {
            private readonly Type _domainContextType;
            private readonly Type _entityContractType;
            private readonly MongoDatabase _database;
            private readonly string _collectionName;
            private readonly Type _documentType;
            private readonly object _documentId;

            //---------------------------------------------------------------------------------------------------------------------------------------------

            public ObjectLazyLoaderById(IPocEntityRepo entityRepo, object documentId)
            {
                var collection = ((IPocMongoEntityRepo)entityRepo).GetMongoCollection();

                _domainContextType = entityRepo.GetOwnerContext().GetType();
                _entityContractType = entityRepo.ContractType;
                _database = collection.Database;
                _collectionName = collection.Name;
                _documentType = entityRepo.PersistableType;
                _documentId = documentId;
            }

            //---------------------------------------------------------------------------------------------------------------------------------------------

            public IPocDomainObject Load(IPocDomainObject target)
            {
                PocQueryLog.LogQuery("LLID[{0}[{1}]]", _collectionName, _documentId);

                var collection = _database.GetCollection(_collectionName);
                var entityRepo = PocThreadStaticRepoStack.Peek(_domainContextType).GetEntityRepo(_entityContractType);
                
                var persistableObject = (IPocPersistableObject)collection.FindOneByIdAs(_documentType, BsonValue.Create(_documentId));
                var domainObject = target ?? entityRepo.NewDomainObject(persistableObject.PocHintDomainImplType);
                var values = persistableObject.ExportValues(entityRepo);
                domainObject.ImportValues(entityRepo, values);

                return domainObject;
            }

            //---------------------------------------------------------------------------------------------------------------------------------------------

            public object EntityId
            {
                get { return _documentId; }
            }

            //---------------------------------------------------------------------------------------------------------------------------------------------

            public Type EntityContractType
            {
                get { return _entityContractType; }
            }

            //---------------------------------------------------------------------------------------------------------------------------------------------

            public Type DomainContextType
            {
                get { return _domainContextType; }
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public class ObjectLazyLoaderByForeignKey : IPersistableObjectLazyLoader
        {
            private readonly Type _domainContextType;
            private readonly Type _entityContractType;
            private readonly MongoDatabase _database;
            private readonly string _collectionName;
            private readonly Type _documentType;
            private readonly string _keyPropertyName;
            private readonly object _keyPropertyValue;

            //---------------------------------------------------------------------------------------------------------------------------------------------

            public ObjectLazyLoaderByForeignKey(IPocEntityRepo entityRepo, string keyPropertyName, object keyPropertyValue)
            {
                _keyPropertyValue = keyPropertyValue;
                _keyPropertyName = keyPropertyName;
                var collection = ((IPocMongoEntityRepo)entityRepo).GetMongoCollection();

                _domainContextType = entityRepo.GetOwnerContext().GetType();
                _entityContractType = entityRepo.ContractType;
                _database = collection.Database;
                _collectionName = collection.Name;
                _documentType = entityRepo.PersistableType;
                _keyPropertyName = keyPropertyName;
                _keyPropertyValue = keyPropertyValue;
            }

            //---------------------------------------------------------------------------------------------------------------------------------------------

            public IPocDomainObject Load(IPocDomainObject target)
            {
                PocQueryLog.LogQuery("LLFK[{0}]", _collectionName);

                var collection = _database.GetCollection(_documentType, _collectionName);
                var entityRepo = PocThreadStaticRepoStack.Peek(_domainContextType).GetEntityRepo(_entityContractType);
                
                var query = Query.EQ(_keyPropertyName, BsonValue.Create(_keyPropertyValue));
                var persistableObject = (IPocPersistableObject)collection.FindOneAs(_documentType, query);
                
                var domainObject = target ?? entityRepo.NewDomainObject(persistableObject.PocHintDomainImplType);
                var values = persistableObject.ExportValues(entityRepo);
                domainObject.ImportValues(entityRepo, values);

                return domainObject;
            }

            //---------------------------------------------------------------------------------------------------------------------------------------------

            public object EntityId
            {
                get { return null; }
            }

            //---------------------------------------------------------------------------------------------------------------------------------------------

            public Type EntityContractType
            {
                get { return _entityContractType; }
            }

            //---------------------------------------------------------------------------------------------------------------------------------------------

            public Type DomainContextType
            {
                get { return _domainContextType; }
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public class ObjectCollectionLazyLoaderById<TId> : IPersistableObjectCollectionLazyLoader
        {
            private readonly Type _domainContextType;
            private readonly Type _entityContractType;
            private readonly MongoDatabase _database;
            private readonly string _collectionName;
            private readonly Type _documentType;
            private readonly TId[] _documentIds;

            //---------------------------------------------------------------------------------------------------------------------------------------------

            public ObjectCollectionLazyLoaderById(IPocEntityRepo entityRepo, TId[] documentIds)
            {
                var collection = ((IPocMongoEntityRepo)entityRepo).GetMongoCollection();

                _domainContextType = entityRepo.GetOwnerContext().GetType();
                _entityContractType = entityRepo.ContractType;
                _database = collection.Database;
                _collectionName = collection.Name;
                _documentType = entityRepo.PersistableType;
                _documentIds = documentIds;
            }

            //---------------------------------------------------------------------------------------------------------------------------------------------

            public IEnumerable<IPocDomainObject> Load()
            {
                PocQueryLog.LogQuery("CLLIDS[{0}]", _collectionName);

                var collection = _database.GetCollection(_documentType, _collectionName);
                var entityRepo = PocThreadStaticRepoStack.Peek(_domainContextType).GetEntityRepo(_entityContractType);
                
                var query = Query.In("_id", new BsonArray(_documentIds));
                var cursor = collection.FindAs(_documentType, query);

                return cursor.Cast<IPocPersistableObject>().Select(persistableObject => {
                    var domainObject = entityRepo.NewDomainObject(persistableObject.PocHintDomainImplType);
                    var values = persistableObject.ExportValues(entityRepo);
                    domainObject.ImportValues(entityRepo, values);
                    return domainObject;
                });
            }

            //---------------------------------------------------------------------------------------------------------------------------------------------

            public Type EntityContractType
            {
                get { return _entityContractType; }
            }

            //---------------------------------------------------------------------------------------------------------------------------------------------

            public Type DomainContextType
            {
                get { return _domainContextType; }
            }

            //---------------------------------------------------------------------------------------------------------------------------------------------

            public TId[] DocumentIds
            {
                get { return _documentIds; }
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public class ObjectCollectionLazyLoaderByForeignKey : IPersistableObjectCollectionLazyLoader
        {
            private readonly Type _domainContextType;
            private readonly Type _entityContractType;
            private readonly MongoDatabase _database;
            private readonly string _collectionName;
            private readonly Type _documentType;
            private readonly string _foreignKeyPropertyName;
            private readonly object _foreignKeyValue;

            //---------------------------------------------------------------------------------------------------------------------------------------------

            public ObjectCollectionLazyLoaderByForeignKey(IPocEntityRepo entityRepo, string foreignKeyPropertyName, object foreignKeyValue)
            {
                var collection = ((IPocMongoEntityRepo)entityRepo).GetMongoCollection();

                _domainContextType = entityRepo.GetOwnerContext().GetType();
                _entityContractType = entityRepo.ContractType;
                _database = collection.Database;
                _collectionName = collection.Name;
                _documentType = entityRepo.PersistableType;

                _foreignKeyValue = foreignKeyValue;
                _foreignKeyPropertyName = foreignKeyPropertyName;
            }

            //---------------------------------------------------------------------------------------------------------------------------------------------

            public IEnumerable<IPocDomainObject> Load()
            {
                PocQueryLog.LogQuery("CLLFK[{0}]", _collectionName);

                var collection = _database.GetCollection(_documentType, _collectionName);
                var entityRepo = PocThreadStaticRepoStack.Peek(_domainContextType).GetEntityRepo(_entityContractType);

                var query = Query.EQ(_foreignKeyPropertyName, BsonValue.Create(_foreignKeyValue));
                var cursor = collection.FindAs(_documentType, query);

                return cursor.Cast<IPocPersistableObject>().Select(persistableObject => {
                    var domainObject = entityRepo.NewDomainObject(persistableObject.PocHintDomainImplType);
                    var values = persistableObject.ExportValues(entityRepo);
                    domainObject.ImportValues(entityRepo, values);
                    return domainObject;
                });
            }

            //---------------------------------------------------------------------------------------------------------------------------------------------

            public Type EntityContractType
            {
                get { return _entityContractType; }
            }

            //---------------------------------------------------------------------------------------------------------------------------------------------

            public Type DomainContextType
            {
                get { return _domainContextType; }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class PocMongoEntityRepo<TContract, TDomain, TPersistable> : IPocEntityRepo<TContract>, IPocEntityRepo, IPocMongoEntityRepo
            where TDomain : TContract
        {
            private readonly IPocDomainContext _ownerContext;
            private readonly MongoCollection<TPersistable> _collection;
            private readonly Func<TDomain> _domainFactory;
            private readonly Func<TPersistable> _persistableFactory;
            private readonly Dictionary<object, IPocDomainObject> _entityCacheByKey;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public PocMongoEntityRepo(IPocDomainContext ownerContext, string collectionName, Func<TDomain> domainFactory, Func<TPersistable> persistableFactory)
            {
                _persistableFactory = persistableFactory;
                _domainFactory = domainFactory;
                _ownerContext = ownerContext;
                _collection = ((IPocMongoDomainContext)_ownerContext).Database.GetCollection<TPersistable>(collectionName);
                _entityCacheByKey = new Dictionary<object, IPocDomainObject>();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            #region Implementation of IPocEntityRepo<TContract>

            public TContract New()
            {
                var entity = (TContract)Activator.CreateInstance(typeof(TDomain), _ownerContext.Components);
                ((IPocDomainObject)entity).InitializeValues(idManuallyAssigned: false);
                return entity;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public T New<T>(Type pocHintDomainImplType) where T : TContract
            {
                var entity = (T)Activator.CreateInstance(pocHintDomainImplType, _ownerContext.Components);
                ((IPocDomainObject)entity).InitializeValues(idManuallyAssigned: false);
                return entity;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void Insert(TContract entity)
            {
                var persistable = _persistableFactory();
                var values = ((IPocDomainObject)entity).ExportValues(this);
                ((IPocPersistableObject)persistable).ImportValues(this, values);
                _collection.Insert(persistable);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public TContract GetById(object id)
            {
                PocQueryLog.LogQuery<TContract>(id);

                var idBsonValue = BsonValue.Create(id);
                var persistable = _collection.FindOneById(idBsonValue);
                var entity = _domainFactory();
                var values = ((IPocPersistableObject)persistable).ExportValues(this);
                ((IPocDomainObject)entity).ImportValues(this, values);
                return entity;
            }

            #endregion

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            MongoCollection IPocMongoEntityRepo.GetMongoCollection()
            {
                return _collection;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            IPocDomainContext IPocEntityRepo.GetOwnerContext()
            {
                return _ownerContext;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            bool IPocEntityRepo.TryGetEntityFromCache(object id, out IPocDomainObject entity)
            {
                return _entityCacheByKey.TryGetValue(id, out entity);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            IPocDomainObject IPocEntityRepo.NewDomainObject(Type pocHintDerivedDomainImplType)
            {
                var domainImplType = pocHintDerivedDomainImplType ?? typeof(TDomain);

                var entity = (IPocDomainObject)Activator.CreateInstance(domainImplType, _ownerContext.Components);
                ((IPocDomainObject)entity).InitializeValues(idManuallyAssigned: false);
                
                return entity;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            Type IPocEntityRepo.ContractType
            {
                get { return typeof(TContract); }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            Type IPocEntityRepo.PersistableType
            {
                get { return typeof(TPersistable); }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            Type IPocEntityRepo.DomainType
            {
                get { return typeof(TDomain); }
            }
        }
    }
}
