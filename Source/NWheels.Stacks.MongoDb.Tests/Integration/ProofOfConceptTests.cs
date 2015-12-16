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

            var one = context.Ones.GetById(1);

            one.ShouldNotBeNull();
            one.IntScalar.ShouldBe(12345);
            one.StringScalar.ShouldBe("ABCDEF");
            one.ValueA.ShouldNotBeNull();
            one.ValueA.DateTimeScalar.ShouldBe(new DateTime(2015, 10, 20, 17, 30, 50, 123, DateTimeKind.Utc));
            one.ValueA.TimeSpanScalar.ShouldBe(TimeSpan.FromSeconds(47));
            one.ValueBs.ShouldNotBeNull();
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
                private IPersistableObjectLazyLoader _lazyLoader;
                private IPersistableObjectCollectionLazyLoader m_ThreesLazyLoader;

                //---------------------------------------------------------------------------------------------------------------------------------------------

                public DomainObject_EntityOne(IComponentContext components)
                {
                    _components = components;
                    _lazyLoader = null;
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                void IPocDomainObject.InitializeValues()
                {
                    this.Id = IntIdGenerator.TakeNextValue();
                    this.IntScalar = 123;
                    this.StringScalar = "ABC";

                    this.ValueA = new DomainObject_ValueObjectA(_components);
                    ((IPocDomainObject)ValueA).InitializeValues();

                    this.ValueBs =
                        new ConcreteToAbstractCollectionAdapter<DomainObject_ValueObjectB, PocDomain.IValueObjectB>(new List<DomainObject_ValueObjectB>());
                    this.Two = null;
                    this.Threes = new ConcreteToAbstractCollectionAdapter<DomainObject_EntityThree, PocDomain.IEntityThree>(
                        new List<DomainObject_EntityThree>());
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                void IPocImportExportValues.ImportValues(IPocEntityRepo entityRepo, object[] values)
                {
                    this.Id = (int)values[0];
                    this.IntScalar = (int)values[1];
                    this.StringScalar = (string)values[2];
                    this.ValueA = PocDomainRuntimeHelpers.ImportEmbeddedDomainObject<PocDomain.IValueObjectA, DomainObject_ValueObjectA>(
                        entityRepo,
                        values[3],
                        () => new DomainObject_ValueObjectA(_components));
                    this.ValueBs = PocDomainRuntimeHelpers.ImportEmbeddedDomainCollection<PocDomain.IValueObjectB, DomainObject_ValueObjectB>(
                        entityRepo,
                        values[4],
                        () => new DomainObject_ValueObjectB(_components));
                    this.Two = PocDomainRuntimeHelpers.ImportDomainLazyLoadObject<PocDomain.IEntityTwo, DomainObject_EntityTwo>(
                        entityRepo,
                        values[5],
                        () => new DomainObject_EntityTwo(_components));
                    this.Threes = PocDomainRuntimeHelpers.ImportDomainLazyLoadObjectCollection<PocDomain.IEntityThree, DomainObject_EntityThree>(
                        entityRepo,
                        values[6],
                        () => new DomainObject_EntityThree(_components),
                        out m_ThreesLazyLoader);
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                object[] IPocImportExportValues.ExportValues(IPocEntityRepo entityRepo)
                {
                    return new object[] { Id, IntScalar, StringScalar, ValueA, ValueBs, Two, Threes };
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                void IPocDomainObject.SetLazyLoader(IPersistableObjectLazyLoader lazyLoader)
                {
                    this.Id = (int)lazyLoader.EntityId;
                    _lazyLoader = lazyLoader;
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                object IPocDomainObject.EntityId
                {
                    get { return Id; }
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                #region Implementation of IEntityOne

                public int Id { get; private set; }
                public int IntScalar { get; set; }
                public string StringScalar { get; set; }
                public PocDomain.IValueObjectA ValueA { get; private set; }
                public ICollection<PocDomain.IValueObjectB> ValueBs { get; private set; }
                public PocDomain.IEntityTwo Two { get; set; }
                public ICollection<PocDomain.IEntityThree> Threes { get; private set; }

                #endregion
            }


            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public class DomainObject_EntityTwo : PocDomain.IEntityTwo, IPocDomainObject
            {
                private readonly IComponentContext _components;
                private IPersistableObjectLazyLoader _lazyLoader;

                //---------------------------------------------------------------------------------------------------------------------------------------------

                public DomainObject_EntityTwo(IComponentContext components)
                {
                    _components = components;
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                void IPocDomainObject.InitializeValues()
                {
                    this.Id = ObjectId.GenerateNewId();
                    this.StringValue = "ABC2";
                    this.One = null;
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                void IPocDomainObject.SetLazyLoader(IPersistableObjectLazyLoader lazyLoader)
                {
                    _lazyLoader = lazyLoader;
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                void IPocImportExportValues.ImportValues(IPocEntityRepo entityRepo, object[] values)
                {
                    this.Id = (ObjectId)values[0];
                    this.StringValue = (string)values[1];
                    this.One = PocDomainRuntimeHelpers.ImportDomainLazyLoadObject<PocDomain.IEntityOne, DomainObject_EntityOne>(
                        entityRepo,
                        values[2],
                        () => new DomainObject_EntityOne(_components));
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                object[] IPocImportExportValues.ExportValues(IPocEntityRepo entityRepo)
                {
                    return new object[] { this.Id, this.StringValue, this.One };
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                object IPocDomainObject.EntityId
                {
                    get { return Id; }
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                #region Implementation of IEntityTwo

                public object Id { get; private set; }
                public string StringValue { get; set; }
                public PocDomain.IEntityOne One { get; set; }

                #endregion
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public class DomainObject_EntityThree : PocDomain.IEntityThree, IPocDomainObject
            {
                private readonly IComponentContext _components;
                private IPersistableObjectLazyLoader _lazyLoader;
                private IPersistableObjectCollectionLazyLoader _foursLazyLoader;

                //---------------------------------------------------------------------------------------------------------------------------------------------

                public DomainObject_EntityThree(IComponentContext components)
                {
                    _components = components;
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                void IPocDomainObject.InitializeValues()
                {
                    this.Id = ObjectId.GenerateNewId();
                    this.StringValue = "ABC3";
                    this.Fours = new ConcreteToAbstractCollectionAdapter<DomainObject_EntityFour, PocDomain.IEntityFour>(new List<DomainObject_EntityFour>());
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                void IPocImportExportValues.ImportValues(IPocEntityRepo entityRepo, object[] values)
                {
                    this.Id = (ObjectId)values[0];
                    this.StringValue = (string)values[1];
                    this.Fours = PocDomainRuntimeHelpers.ImportDomainLazyLoadObjectCollection<PocDomain.IEntityFour, DomainObject_EntityFour>(
                        entityRepo,
                        values[2],
                        () => new DomainObject_EntityFour(_components),
                        out _foursLazyLoader);
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                object[] IPocImportExportValues.ExportValues(IPocEntityRepo entityRepo)
                {
                    return new object[] { Id, StringValue, Fours };
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                object IPocDomainObject.EntityId
                {
                    get { return Id; }
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                void IPocDomainObject.SetLazyLoader(IPersistableObjectLazyLoader lazyLoader)
                {
                    _lazyLoader = lazyLoader;
                    this.Id = lazyLoader.EntityId;
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                #region Implementation of IEntityThree

                public object Id { get; private set; }
                public string StringValue { get; set; }
                public ICollection<PocDomain.IEntityFour> Fours { get; private set; }

                #endregion
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public class DomainObject_EntityFour : PocDomain.IEntityFour, IPocDomainObject
            {
                private readonly IComponentContext _components;
                private IPersistableObjectLazyLoader _lazyLoader;

                //---------------------------------------------------------------------------------------------------------------------------------------------

                public DomainObject_EntityFour(IComponentContext components)
                {
                    _components = components;
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                #region Implementation of IEntityFour

                public string Id { get; set; }
                public string StringValue { get; set; }
                public PocDomain.IEntityThree Three { get; set; }

                #endregion

                //---------------------------------------------------------------------------------------------------------------------------------------------

                void IPocDomainObject.InitializeValues()
                {
                    this.Id = null;
                    this.StringValue = "ABC4";
                    this.Three = null;
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                void IPocImportExportValues.ImportValues(IPocEntityRepo entityRepo, object[] values)
                {
                    this.Id = (string)values[0];
                    this.StringValue = (string)values[1];
                    this.Three = PocDomainRuntimeHelpers.ImportDomainLazyLoadObject<PocDomain.IEntityThree, DomainObject_EntityThree>(
                        entityRepo,
                        values[2],
                        () => new DomainObject_EntityThree(_components));
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                object[] IPocImportExportValues.ExportValues(IPocEntityRepo entityRepo)
                {
                    return new object[] { Id, StringValue, Three };
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                void IPocDomainObject.SetLazyLoader(IPersistableObjectLazyLoader lazyLoader)
                {
                    _lazyLoader = lazyLoader;
                    this.Id = (string)lazyLoader.EntityId;
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                object IPocDomainObject.EntityId
                {
                    get { return Id; }
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public class DomainObject_ValueObjectA : PocDomain.IValueObjectA, IPocDomainObject
            {
                private readonly IComponentContext _components;

                //---------------------------------------------------------------------------------------------------------------------------------------------

                public DomainObject_ValueObjectA(IComponentContext components)
                {
                    _components = components;
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                void IPocDomainObject.InitializeValues()
                {
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                void IPocImportExportValues.ImportValues(IPocEntityRepo entityRepo, object[] values)
                {
                    TimeSpanScalar = (TimeSpan)values[0];
                    DateTimeScalar = (DateTime)values[1];
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                void IPocDomainObject.SetLazyLoader(IPersistableObjectLazyLoader lazyLoader)
                {
                    throw new NotSupportedException();
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                object[] IPocImportExportValues.ExportValues(IPocEntityRepo entityRepo)
                {
                    return new object[] { TimeSpanScalar, DateTimeScalar };
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                object IPocDomainObject.EntityId
                {
                    get { return null; }
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                #region Implementation of IValueObjectA

                public TimeSpan TimeSpanScalar { get; set; }
                public DateTime DateTimeScalar { get; set; }

                #endregion
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public class DomainObject_ValueObjectB : PocDomain.IValueObjectB, IPocDomainObject
            {
                private readonly IComponentContext _components;

                //---------------------------------------------------------------------------------------------------------------------------------------------

                public DomainObject_ValueObjectB(IComponentContext components)
                {
                    _components = components;
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                void IPocDomainObject.InitializeValues()
                {
                    this.ValueCs = new ConcreteToAbstractListAdapter<DomainObject_ValueObjectC, PocDomain.IValueObjectC>(new List<DomainObject_ValueObjectC>());
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                void IPocImportExportValues.ImportValues(IPocEntityRepo entityRepo, object[] values)
                {
                    GuidScalar = (Guid)values[0];
                    EnumScalar = (DayOfWeek)values[1];
                    ValueCs = PocDomainRuntimeHelpers.ImportEmbeddedDomainCollection<PocDomain.IValueObjectC, DomainObject_ValueObjectC>(
                        entityRepo,
                        values[2],
                        () => new DomainObject_ValueObjectC(_components));
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                object[] IPocImportExportValues.ExportValues(IPocEntityRepo entityRepo)
                {
                    return new object[] { GuidScalar, EnumScalar, ValueCs };
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                void IPocDomainObject.SetLazyLoader(IPersistableObjectLazyLoader lazyLoader)
                {
                    throw new NotSupportedException();
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                object IPocDomainObject.EntityId
                {
                    get { return null; }
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                #region Implementation of IValueObjectB

                public Guid GuidScalar { get; set; }
                public DayOfWeek EnumScalar { get; set; }
                public IList<PocDomain.IValueObjectC> ValueCs { get; private set; }

                #endregion
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public class DomainObject_ValueObjectC : PocDomain.IValueObjectC, IPocDomainObject
            {
                private readonly IComponentContext _components;
                private IPersistableObjectCollectionLazyLoader _foursLazyLoader;

                //---------------------------------------------------------------------------------------------------------------------------------------------

                public DomainObject_ValueObjectC(IComponentContext components)
                {
                    _components = components;
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                void IPocDomainObject.InitializeValues()
                {
                    this.Fours = new List<PocDomain.IEntityFour>();
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                void IPocImportExportValues.ImportValues(IPocEntityRepo entityRepo, object[] values)
                {
                    FlagsEnumScalar = (PocDomain.AFlagsEnum)values[0];
                    DecimalValue = (decimal)values[1];
                    Three = PocDomainRuntimeHelpers.ImportDomainLazyLoadObject<PocDomain.IEntityThree, DomainObject_EntityThree>(
                        entityRepo,
                        values[2],
                        () => new DomainObject_EntityThree(_components));
                    Fours = PocDomainRuntimeHelpers.ImportDomainLazyLoadObjectCollection<PocDomain.IEntityFour, DomainObject_EntityFour>(
                        entityRepo,
                        values[3],
                        () => new DomainObject_EntityFour(_components),
                        out _foursLazyLoader);
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                object[] IPocImportExportValues.ExportValues(IPocEntityRepo entityRepo)
                {
                    return new object[] { FlagsEnumScalar, DecimalValue, Three, Fours };
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                void IPocDomainObject.SetLazyLoader(IPersistableObjectLazyLoader lazyLoader)
                {
                    throw new NotSupportedException();
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                object IPocDomainObject.EntityId
                {
                    get { return null; }
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                #region Implementation of IValueObjectC

                public PocDomain.AFlagsEnum FlagsEnumScalar { get; set; }
                public decimal DecimalValue { get; set; }
                public PocDomain.IEntityThree Three { get; set; }
                public ICollection<PocDomain.IEntityFour> Fours { get; private set; }

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


                    _repoByContract[typeof(PocDomain.IEntityOne)] = this.Ones;
                    _repoByContract[typeof(PocDomain.IEntityTwo)] = this.Twos;
                    _repoByContract[typeof(PocDomain.IEntityThree)] = this.Threes;
                    _repoByContract[typeof(PocDomain.IEntityFour)] = this.Ones;
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
                    ((IPocDomainObject)obj).InitializeValues();

                    obj.Id = id;
                    return obj;
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                PocDomain.IValueObjectB PocDomain.IMyPocContext.NewValueObjectB(Guid guidScalar, DayOfWeek enumScalar, params PocDomain.IValueObjectC[] valueCs)
                {
                    var obj = new DomainObject_ValueObjectB(_components);
                    ((IPocDomainObject)obj).InitializeValues();

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
                    ((IPocDomainObject)obj).InitializeValues();

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
            void InitializeValues();
            void SetLazyLoader(IPersistableObjectLazyLoader lazyLoader);
            object EntityId { get; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface IPocPersistableObject : IPocImportExportValues
        {
            object EntityId { get; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public  interface IPersistableObjectLazyLoader
        {
            IPocPersistableObject Load();
            Type EntityContractType { get; }
            Type DomainContextType { get; }
            object EntityId { get; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public  interface IPersistableObjectCollectionLazyLoader
        {
            IEnumerable<IPocPersistableObject> Load();
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
            //MongoCollection GetMongoCollection();
            bool TryGetEntityFromCache(object id, out IPocDomainObject entity);
            Type ContractType { get; }
            Type PersistableType { get; }
            Type DomainType { get; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public  interface IPocEntityRepo<TEntity> : IPocEntityRepo
        {
            TEntity New();
            T New<T>(Type pocHintDomainImplType) where T : TEntity;
            void Insert(TEntity entity);
            TEntity GetById(object id);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public  interface IPocDomainContext
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
                    ((IPocDomainObject)impl).InitializeValues();
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
                            ((IPocDomainObject)impl).InitializeValues();
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
                    ((IPocDomainObject)impl).SetLazyLoader(null);
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
                            ((IPocDomainObject)itemImpl).InitializeValues();
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

            public static void LazyLoadObject<TContract, TImpl>(IPocEntityRepo entityRepo, TImpl impl, ref IPersistableObjectLazyLoader lazyLoader)
                where TContract : class
                where TImpl : TContract
            {
                var persisted = lazyLoader.Load();
                ((IPocDomainObject)impl).ImportValues(entityRepo, persisted.ExportValues(entityRepo));
                lazyLoader = null;
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

            public IPocPersistableObject Load()
            {
                var collection = _database.GetCollection(_collectionName);
                return (IPocPersistableObject)collection.FindOneByIdAs(_documentType, BsonValue.Create(_documentId));
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

            public IPocPersistableObject Load()
            {
                var collection = _database.GetCollection(_documentType, _collectionName);
                var query = Query.EQ(_keyPropertyName, BsonValue.Create(_keyPropertyValue));
                return (IPocPersistableObject)collection.FindOneAs(_documentType, query);
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

            public IEnumerable<IPocPersistableObject> Load()
            {
                var collection = _database.GetCollection(_documentType, _collectionName);
                var query = Query.In("_id", new BsonArray(_documentIds));
                var cursor = collection.FindAs(_documentType, query);

                return cursor.Cast<IPocPersistableObject>();
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

            public IEnumerable<IPocPersistableObject> Load()
            {
                var collection = _database.GetCollection(_documentType, _collectionName);
                var query = Query.EQ(_foreignKeyPropertyName, BsonValue.Create(_foreignKeyValue));
                var cursor = collection.FindAs(_documentType, query);

                return cursor.Cast<IPocPersistableObject>();
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
                ((IPocDomainObject)entity).InitializeValues();
                return entity;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public T New<T>(Type pocHintDomainImplType) where T : TContract
            {
                var entity = (T)Activator.CreateInstance(pocHintDomainImplType, _ownerContext.Components);
                ((IPocDomainObject)entity).InitializeValues();
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
