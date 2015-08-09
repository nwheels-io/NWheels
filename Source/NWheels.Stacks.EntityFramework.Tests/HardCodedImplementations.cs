using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Hapil;
using Newtonsoft.Json;
using NWheels.Conventions.Core;
using NWheels.DataObjects;
using NWheels.DataObjects.Core;
using NWheels.DataObjects.Core.StorageTypes;
using NWheels.Entities;
using NWheels.Entities.Core;
using NWheels.Stacks.EntityFramework.EFConventions;
using NWheels.Testing.Entities.Impl;
using NWheels.Testing.Entities.Stacks;
using System.Data.SqlClient;
using NWheels.Stacks.EntityFramework.Factories;
using System.Configuration;
using NWheels.Testing;
using System.ComponentModel.DataAnnotations.Schema;
using NWheels.TypeModel.Core.Factories;

namespace NWheels.Stacks.EntityFramework.Tests
{
    public static class HardCodedImplementations
    {
        public class DataRepositoryFactory_OnlineStoreRepository : DataRepositoryFactoryBase
        {
            private readonly TestFramework _framework;
            private readonly IComponentContext _components;
            private readonly ConnectionStringSettings _connectionSettings;

            public DataRepositoryFactory_OnlineStoreRepository(
                TestFramework framework, 
                DynamicModule module, 
                TypeMetadataCache metadataCache, 
                ConnectionStringSettings connectionSettings)
                : base(module, metadataCache)
            {
                _framework = framework;
                _components = framework.Components;
                _connectionSettings = connectionSettings;
            }

            public override IApplicationDataRepository NewUnitOfWork(Type repositoryType, bool autoCommit, System.Data.IsolationLevel? isolationLevel = null)
            {
                if ( repositoryType != typeof(Interfaces.Repository1.IOnlineStoreRepository) )
                {
                    throw new NotSupportedException("Not supported by hard-coded data repository factory.");
                }

                base.MetadataCache.GetTypeMetadata(typeof(Interfaces.Repository1.ICategory));
                base.MetadataCache.GetTypeMetadata(typeof(Interfaces.Repository1.IProduct));
                base.MetadataCache.GetTypeMetadata(typeof(Interfaces.Repository1.IOrder));
                base.MetadataCache.GetTypeMetadata(typeof(Interfaces.Repository1.IOrderLine));
                base.MetadataCache.GetTypeMetadata(typeof(Interfaces.Repository1.IAttribute));
                base.MetadataCache.GetTypeMetadata(typeof(Interfaces.Repository1.IAttributeValue));
                base.MetadataCache.GetTypeMetadata(typeof(Interfaces.Repository1.IAttributeValueChoice));
                base.MetadataCache.GetTypeMetadata(typeof(Interfaces.Repository1.IPostalAddress));
                base.MetadataCache.GetTypeMetadata(typeof(Interfaces.Repository1.ICustomer));
                base.MetadataCache.GetTypeMetadata(typeof(Interfaces.Repository1.IContactDetail));
                base.MetadataCache.GetTypeMetadata(typeof(Interfaces.Repository1.IEmailContactDetail));

                base.MetadataCache.AcceptVisitor(new CrossTypeFixupMetadataVisitor(base.MetadataCache));

                base.MetadataCache.EnsureRelationalMapping(base.MetadataCache.GetTypeMetadata(typeof(Interfaces.Repository1.ICategory)));
                base.MetadataCache.EnsureRelationalMapping(base.MetadataCache.GetTypeMetadata(typeof(Interfaces.Repository1.IProduct)));
                base.MetadataCache.EnsureRelationalMapping(base.MetadataCache.GetTypeMetadata(typeof(Interfaces.Repository1.IOrder)));
                base.MetadataCache.EnsureRelationalMapping(base.MetadataCache.GetTypeMetadata(typeof(Interfaces.Repository1.IOrderLine)));
                base.MetadataCache.EnsureRelationalMapping(base.MetadataCache.GetTypeMetadata(typeof(Interfaces.Repository1.IAttribute)));
                base.MetadataCache.EnsureRelationalMapping(base.MetadataCache.GetTypeMetadata(typeof(Interfaces.Repository1.IAttributeValue)));
                base.MetadataCache.EnsureRelationalMapping(base.MetadataCache.GetTypeMetadata(typeof(Interfaces.Repository1.IAttributeValueChoice)));
                base.MetadataCache.EnsureRelationalMapping(base.MetadataCache.GetTypeMetadata(typeof(Interfaces.Repository1.IPostalAddress)));
                base.MetadataCache.EnsureRelationalMapping(base.MetadataCache.GetTypeMetadata(typeof(Interfaces.Repository1.ICustomer)));
                base.MetadataCache.EnsureRelationalMapping(base.MetadataCache.GetTypeMetadata(typeof(Interfaces.Repository1.IContactDetail)));
                base.MetadataCache.EnsureRelationalMapping(base.MetadataCache.GetTypeMetadata(typeof(Interfaces.Repository1.IEmailContactDetail)));

                var dbProviderName = _connectionSettings.ProviderName;
                var dbProviderFactory = DbProviderFactories.GetFactory(dbProviderName);
                var dbConfig = _framework.ConfigSection<IFrameworkDatabaseConfig>();
                dbConfig.ConnectionString = _connectionSettings.ConnectionString;

                var connection = dbProviderFactory.CreateConnection();
                connection.ConnectionString = _connectionSettings.ConnectionString;
                connection.Open();

                return new EfDataRepository_OnlineStoreRepository(
                    _components, 
                    new EntityObjectFactory_OnlineStoreRepository(_components), 
                    base.MetadataCache,
                    connection,
                    false);
            }
        }

        public class EntityObjectFactory_OnlineStoreRepository : IEntityObjectFactory
        {
            private readonly IComponentContext _components;

            public EntityObjectFactory_OnlineStoreRepository(IComponentContext components)
            {
                _components = components;
            }

            public TEntityContract NewEntity<TEntityContract>() where TEntityContract : class
            {
                if (typeof(TEntityContract) == typeof(Interfaces.Repository1.IOrder))
                {
                    return (TEntityContract)(object)new EfEntityObject_Order(_components);
                }
                if (typeof(TEntityContract) == typeof(Interfaces.Repository1.IOrderLine))
                {
                    return (TEntityContract)(object)new EfEntityObject_OrderLine(_components);
                }
                if (typeof(TEntityContract) == typeof(Interfaces.Repository1.IProduct))
                {
                    return (TEntityContract)(object)new EfEntityObject_Product(_components);
                }
                if (typeof(TEntityContract) == typeof(Interfaces.Repository1.ICategory))
                {
                    return (TEntityContract)(object)new EfEntityObject_Category(_components);
                }
                if (typeof(TEntityContract) == typeof(Interfaces.Repository1.IAttribute))
                {
                    return (TEntityContract)(object)new EfEntityObject_Attribute(_components);
                }
                if (typeof(TEntityContract) == typeof(Interfaces.Repository1.IAttributeValue))
                {
                    return (TEntityContract)(object)new EfEntityObject_AttributeValue(_components);
                }
                if (typeof(TEntityContract) == typeof(Interfaces.Repository1.IAttributeValueChoice))
                {
                    return (TEntityContract)(object)new EfEntityObject_AttributeValueChoice();
                }
                if (typeof(TEntityContract) == typeof(Interfaces.Repository1.ICustomer))
                {
                    return (TEntityContract)(object)new EfEntityObject_Customer();
                }
                if (typeof(TEntityContract) == typeof(Interfaces.Repository1.IEmailContactDetail))
                {
                    return (TEntityContract)(object)new EfEntityObject_EmailContactDetail();
                }
                if (typeof(TEntityContract) == typeof(Interfaces.Repository1.IPhoneContactDetail))
                {
                    return (TEntityContract)(object)new EfEntityObject_PhoneContactDetail();
                }
                if (typeof(TEntityContract) == typeof(Interfaces.Repository1.IPostContactDetail))
                {
                    return (TEntityContract)(object)new EfEntityObject_PostContactDetail();
                }

                throw new NotSupportedException(
                    string.Format("Entity contract '{0}' is not supported by hard-coded entity factory.", typeof(TEntityContract).Name));
            }

            public object NewEntity(Type entityContractType)
            {
                throw new NotImplementedException();
            }
        }

        public class EfDataRepository_OnlineStoreRepository : EfDataRepositoryBase, Interfaces.Repository1.IOnlineStoreRepository
        {
            // Fields
            private IDomainObjectFactory _domainFactory;
            private ITypeMetadataCache _metadataCache;
            private static DbCompiledModel _s_compiledModel;
            private static object _s_compiledModelSyncRoot = new object();
            public new IEntityObjectFactory EntityFactory;
            private IEntityRepository<Interfaces.Repository1.IAttribute> m_Attributes;
            private IEntityRepository<Interfaces.Repository1.ICategory> m_Categories;
            private IEntityRepository<Interfaces.Repository1.IOrder> m_Orders;
            private IEntityRepository<Interfaces.Repository1.IOrderLine> m_OrdersLines;
            private IEntityRepository<Interfaces.Repository1.IProduct> m_Products;
            private IEntityRepository<Interfaces.Repository1.ICustomer> m_Customers;
            private IEntityRepository<Interfaces.Repository1.IContactDetail> m_ContactDetails;
            private IEntityRepository<Interfaces.Repository1.IEmailContactDetail> m_ContactEmails;

            // Methods
            public EfDataRepository_OnlineStoreRepository(IComponentContext components, IEntityObjectFactory entityFactory, ITypeMetadataCache metadataCache, DbConnection connection, bool autoCommit)
                : base(components, entityFactory, GetOrBuildDbCompiledModel(metadataCache, connection), connection, autoCommit)
            {
                this.EntityFactory = entityFactory;
                this._domainFactory = components.Resolve<IDomainObjectFactory>();
                this._metadataCache = metadataCache;
                this.m_Categories = new EfEntityRepository<Interfaces.Repository1.ICategory, EfEntityObject_Category, EfEntityObject_Category>(this);
                base.RegisterEntityRepository<Interfaces.Repository1.ICategory, EfEntityObject_Category>(this.m_Categories);
                this.m_Products = new EfEntityRepository<Interfaces.Repository1.IProduct, EfEntityObject_Product, EfEntityObject_Product>(this);
                base.RegisterEntityRepository<Interfaces.Repository1.IProduct, EfEntityObject_Product>(this.m_Products);
                this.m_Orders = new EfEntityRepository<Interfaces.Repository1.IOrder, EfEntityObject_Order, EfEntityObject_Order>(this);
                base.RegisterEntityRepository<Interfaces.Repository1.IOrder, EfEntityObject_Order>(this.m_Orders);
                this.m_OrdersLines = new EfEntityRepository<Interfaces.Repository1.IOrderLine, EfEntityObject_OrderLine, EfEntityObject_OrderLine>(this);
                base.RegisterEntityRepository<Interfaces.Repository1.IOrderLine, EfEntityObject_OrderLine>(this.m_OrdersLines);
                this.m_Attributes = new EfEntityRepository<Interfaces.Repository1.IAttribute, EfEntityObject_Attribute, EfEntityObject_Attribute>(this);
                base.RegisterEntityRepository<Interfaces.Repository1.IAttribute, EfEntityObject_Attribute>(this.m_Attributes);
                this.m_Customers = new EfEntityRepository<Interfaces.Repository1.ICustomer, EfEntityObject_Customer, EfEntityObject_Customer>(this);
                base.RegisterEntityRepository<Interfaces.Repository1.ICustomer, EfEntityObject_Customer>(this.m_Customers);
                this.m_ContactDetails = new EfEntityRepository<Interfaces.Repository1.IContactDetail, EfEntityObject_ContactDetail, EfEntityObject_ContactDetail>(this);
                base.RegisterEntityRepository<Interfaces.Repository1.IContactDetail, EfEntityObject_ContactDetail>(this.m_ContactDetails);
                this.m_ContactEmails = new EfEntityRepository<Interfaces.Repository1.IEmailContactDetail, EfEntityObject_ContactDetail, EfEntityObject_EmailContactDetail>(this);
                base.RegisterEntityRepository<Interfaces.Repository1.IEmailContactDetail, EfEntityObject_EmailContactDetail>(this.m_ContactEmails);
            }

            public static object FactoryMethod_1(IComponentContext context1, EntityObjectFactory factory1, ITypeMetadataCache cache1, DbConnection connection1, bool flag1)
            {
                return new EfDataRepository_OnlineStoreRepository(context1, factory1, cache1, connection1, flag1);
            }

            public sealed override Type[] GetEntityContractsInRepository()
            {
                return new Type[] { typeof(Interfaces.Repository1.ICategory), typeof(Interfaces.Repository1.IProduct), typeof(Interfaces.Repository1.IOrder), typeof(Interfaces.Repository1.IOrderLine), typeof(Interfaces.Repository1.IAttribute), typeof(Interfaces.Repository1.IAttributeValue), typeof(Interfaces.Repository1.IAttributeValueChoice), typeof(Interfaces.Repository1.IPostalAddress) };
            }

            public sealed override IEntityRepository[] GetEntityRepositories()
            {
                IEntityRepository[] repositoryArray = new IEntityRepository[8];
                repositoryArray[0] = (IEntityRepository)this.m_Categories;
                repositoryArray[1] = (IEntityRepository)this.m_Products;
                repositoryArray[2] = (IEntityRepository)this.m_Orders;
                repositoryArray[3] = (IEntityRepository)this.m_OrdersLines;
                repositoryArray[4] = (IEntityRepository)this.m_Attributes;
                repositoryArray[5] = null;
                repositoryArray[6] = null;
                repositoryArray[7] = null;
                return repositoryArray;
            }

            public sealed override Type[] GetEntityTypesInRepository()
            {
                return new Type[] { typeof(EfEntityObject_Category), typeof(EfEntityObject_Product), typeof(EfEntityObject_Order), typeof(EfEntityObject_OrderLine), typeof(EfEntityObject_Attribute), typeof(EfEntityObject_AttributeValue), typeof(EfEntityObject_AttributeValueChoice), typeof(EfEntityObject_PostalAddress) };
            }

            public static DbCompiledModel GetOrBuildDbCompiledModel(ITypeMetadataCache metadataCache, DbConnection connection)
            {
                if (_s_compiledModel == null)
                {
                    if (!Monitor.TryEnter(_s_compiledModelSyncRoot, 0x2710))
                    {
                        throw new TimeoutException("Lock could not be acquired within allotted timeout (10000 ms).");
                    }
                    try
                    {
                        if (_s_compiledModel == null)
                        {
                            ((TypeMetadataBuilder)metadataCache.GetTypeMetadata(typeof(Interfaces.Repository1.ICategory))).UpdateImplementation(typeof(EfEntityObjectFactory), typeof(EfEntityObject_Category));
                            ((TypeMetadataBuilder)metadataCache.GetTypeMetadata(typeof(Interfaces.Repository1.IProduct))).UpdateImplementation(typeof(EfEntityObjectFactory), typeof(EfEntityObject_Product));
                            ((TypeMetadataBuilder)metadataCache.GetTypeMetadata(typeof(Interfaces.Repository1.IOrder))).UpdateImplementation(typeof(EfEntityObjectFactory), typeof(EfEntityObject_Order));
                            ((TypeMetadataBuilder)metadataCache.GetTypeMetadata(typeof(Interfaces.Repository1.IOrderLine))).UpdateImplementation(typeof(EfEntityObjectFactory), typeof(EfEntityObject_OrderLine));
                            ((TypeMetadataBuilder)metadataCache.GetTypeMetadata(typeof(Interfaces.Repository1.IAttribute))).UpdateImplementation(typeof(EfEntityObjectFactory), typeof(EfEntityObject_Attribute));
                            ((TypeMetadataBuilder)metadataCache.GetTypeMetadata(typeof(Interfaces.Repository1.IAttributeValue))).UpdateImplementation(typeof(EfEntityObjectFactory), typeof(EfEntityObject_AttributeValue));
                            ((TypeMetadataBuilder)metadataCache.GetTypeMetadata(typeof(Interfaces.Repository1.IAttributeValueChoice))).UpdateImplementation(typeof(EfEntityObjectFactory), typeof(EfEntityObject_AttributeValueChoice));
                            ((TypeMetadataBuilder)metadataCache.GetTypeMetadata(typeof(Interfaces.Repository1.IPostalAddress))).UpdateImplementation(typeof(EfEntityObjectFactory), typeof(EfEntityObject_PostalAddress));
                            ((TypeMetadataBuilder)metadataCache.GetTypeMetadata(typeof(Interfaces.Repository1.ICustomer))).UpdateImplementation(typeof(EfEntityObjectFactory), typeof(EfEntityObject_Customer));
                            ((TypeMetadataBuilder)metadataCache.GetTypeMetadata(typeof(Interfaces.Repository1.IContactDetail))).UpdateImplementation(typeof(EfEntityObjectFactory), typeof(EfEntityObject_ContactDetail));
                            ((TypeMetadataBuilder)metadataCache.GetTypeMetadata(typeof(Interfaces.Repository1.IEmailContactDetail))).UpdateImplementation(typeof(EfEntityObjectFactory), typeof(EfEntityObject_EmailContactDetail));
                            ((TypeMetadataBuilder)metadataCache.GetTypeMetadata(typeof(Interfaces.Repository1.IPhoneContactDetail))).UpdateImplementation(typeof(EfEntityObjectFactory), typeof(EfEntityObject_PhoneContactDetail));
                            ((TypeMetadataBuilder)metadataCache.GetTypeMetadata(typeof(Interfaces.Repository1.IPostContactDetail))).UpdateImplementation(typeof(EfEntityObjectFactory), typeof(EfEntityObject_PostContactDetail));

                            DbModelBuilder modelBuilder = new DbModelBuilder();
                            //IConvention[] conventions = new IConvention[] { new NoUnderscoreForeignKeyNamingConvention() };
                            //modelBuilder.Conventions.Add(conventions);

                            EfEntityObject_Category.ConfigureEfModel(metadataCache, modelBuilder);
                            EfEntityObject_Product.ConfigureEfModel(metadataCache, modelBuilder);
                            EfEntityObject_Order.ConfigureEfModel(metadataCache, modelBuilder);
                            EfEntityObject_OrderLine.ConfigureEfModel(metadataCache, modelBuilder);
                            EfEntityObject_Attribute.ConfigureEfModel(metadataCache, modelBuilder);
                            EfEntityObject_AttributeValue.ConfigureEfModel(metadataCache, modelBuilder);
                            EfEntityObject_AttributeValueChoice.ConfigureEfModel(metadataCache, modelBuilder);
                            EfEntityObject_PostalAddress.ConfigureEfModel(metadataCache, modelBuilder);
                            EfEntityObject_Customer.ConfigureEfModel(metadataCache, modelBuilder);
                            EfEntityObject_ContactDetail.ConfigureEfModel(metadataCache, modelBuilder);
                            EfEntityObject_EmailContactDetail.ConfigureEfModel(metadataCache, modelBuilder);
                            EfEntityObject_PhoneContactDetail.ConfigureEfModel(metadataCache, modelBuilder);
                            EfEntityObject_PostContactDetail.ConfigureEfModel(metadataCache, modelBuilder);

                            _s_compiledModel = modelBuilder.Build(connection).Compile();
                        }
                    }
                    finally
                    {
                        Monitor.Exit(_s_compiledModelSyncRoot);
                    }
                }
                return _s_compiledModel;
            }

            public Interfaces.Repository1.IAttributeValue NewAttributeValue(string value, int displayOrder)
            {
                Interfaces.Repository1.IAttributeValue value2 = _domainFactory.CreateDomainObjectInstance(this.EntityFactory.NewEntity<Interfaces.Repository1.IAttributeValue>());
                value2.Value = value;
                value2.DisplayOrder = displayOrder;
                return value2;
            }

            public Interfaces.Repository1.IAttributeValueChoice NewAttributeValueChoice(Interfaces.Repository1.IAttribute attribute, string value)
            {
                Interfaces.Repository1.IAttributeValueChoice choice = _domainFactory.CreateDomainObjectInstance(this.EntityFactory.NewEntity<Interfaces.Repository1.IAttributeValueChoice>());
                choice.Attribute = attribute;
                choice.Value = value;
                return choice;
            }

            public Interfaces.Repository1.IOrderLine NewOrderLine(Interfaces.Repository1.IOrder order, Interfaces.Repository1.IProduct product, int quantity)
            {
                Interfaces.Repository1.IOrderLine line = _domainFactory.CreateDomainObjectInstance(this.EntityFactory.NewEntity<Interfaces.Repository1.IOrderLine>());
                line.Order = order;
                line.Product = product;
                line.Quantity = quantity;
                return line;
            }

            public Interfaces.Repository1.IPostalAddress NewPostalAddress(string streetAddress, string city, string zipCode, string country)
            {
                Interfaces.Repository1.IPostalAddress address = _domainFactory.CreateDomainObjectInstance(this.EntityFactory.NewEntity<Interfaces.Repository1.IPostalAddress>());
                address.StreetAddress = streetAddress;
                address.City = city;
                address.ZipCode = zipCode;
                address.Country = country;
                return address;
            }

            public Interfaces.Repository1.IEmailContactDetail NewEmailContactDetail(string email, bool isPrimary)
            {
                Interfaces.Repository1.IEmailContactDetail contactDeatil = _domainFactory.CreateDomainObjectInstance(this.EntityFactory.NewEntity<Interfaces.Repository1.IEmailContactDetail>());
                contactDeatil.IsPrimary = isPrimary;
                contactDeatil.Email = email;
                return contactDeatil;
            }

            public Interfaces.Repository1.IPhoneContactDetail NewPhoneContactDetail(string phone, bool isPrimary)
            {
                Interfaces.Repository1.IPhoneContactDetail contactDeatil = _domainFactory.CreateDomainObjectInstance(this.EntityFactory.NewEntity<Interfaces.Repository1.IPhoneContactDetail>());
                contactDeatil.IsPrimary = isPrimary;
                contactDeatil.Phone = phone;
                return contactDeatil;
            }

            public Interfaces.Repository1.IPostContactDetail NewPostContactDetail(bool isPrimary)
            {
                Interfaces.Repository1.IPostContactDetail contactDeatil = _domainFactory.CreateDomainObjectInstance(this.EntityFactory.NewEntity<Interfaces.Repository1.IPostContactDetail>());
                contactDeatil.IsPrimary = isPrimary;
                return contactDeatil;
            }

            // Properties
            public IEntityRepository<Interfaces.Repository1.IAttribute> Attributes
            {
                get
                {
                    return this.m_Attributes;
                }
            }

            public IEntityRepository<Interfaces.Repository1.ICategory> Categories
            {
                get
                {
                    return this.m_Categories;
                }
            }

            public IEntityRepository<Interfaces.Repository1.IOrder> Orders
            {
                get
                {
                    return this.m_Orders;
                }
            }

            public IEntityRepository<Interfaces.Repository1.IOrderLine> OrdersLines
            {
                get
                {
                    return this.m_OrdersLines;
                }
            }

            public IEntityRepository<Interfaces.Repository1.IProduct> Products
            {
                get
                {
                    return this.m_Products;
                }
            }

            public IEntityRepository<Interfaces.Repository1.ICustomer> Customers
            {
                get
                {
                    return this.m_Customers;
                }
            }

            public IEntityRepository<Interfaces.Repository1.IContactDetail> ContactDetails
            {
                get
                {
                    return this.m_ContactDetails;
                }
            }
            
            public IEntityRepository<Interfaces.Repository1.IEmailContactDetail> ContactEmails
            {
                get
                {
                    return this.m_ContactEmails;
                }
            }
        }

        public class EfEntityObject_Attribute : Interfaces.Repository1.IAttribute, IEntityPartUniqueDisplayName, IEntityPartId<int>, IObject, IEntityObject, IHaveNestedObjects
        {
            // Fields
            private int m_Id_storage;
            private string m_Name;
            private string m_TitleForUser;
            private ConcreteToAbstractListAdapter<EfEntityObject_AttributeValue, Interfaces.Repository1.IAttributeValue> m_Values_adapter;
            private List<EfEntityObject_AttributeValue> m_Values_concrete;
            private DualValueStates m_Values_state;
            private string m_Values_storage;

            // Methods
            public EfEntityObject_Attribute()
            {
            }

            public EfEntityObject_Attribute(IComponentContext arg0)
            {
                this.m_Values_concrete = new List<EfEntityObject_AttributeValue>();
                this.m_Values_adapter = new ConcreteToAbstractListAdapter<EfEntityObject_AttributeValue, Interfaces.Repository1.IAttributeValue>(this.m_Values_concrete);
                this.m_Values_state = DualValueStates.Contract;
                this.m_Id_storage = arg0.Resolve<TestIntIdValueGenerator>().GenerateValue("Attribute.Id");
            }

            public static object FactoryMethod_1()
            {
                return new EfEntityObject_Attribute();
            }

            public static object FactoryMethod_2(IComponentContext context1)
            {
                return new EfEntityObject_Attribute(context1);
            }

            public static void ConfigureEfModel(ITypeMetadataCache metadataCache, DbModelBuilder modelBuilder)
            {
                ITypeMetadata typeMetadata = metadataCache.GetTypeMetadata(typeof(Interfaces.Repository1.IAttribute));
                EntityTypeConfiguration<EfEntityObject_Attribute> entity = EfModelApi.EntityType<EfEntityObject_Attribute>(modelBuilder, typeMetadata, metadataCache);
                EfModelApi.StringProperty<EfEntityObject_Attribute>(entity, typeMetadata.GetPropertyByName("TitleForUser"));
                EfModelApi.StringProperty<EfEntityObject_Attribute>(entity, typeMetadata.GetPropertyByName("Values"));
                EfModelApi.StringProperty<EfEntityObject_Attribute>(entity, typeMetadata.GetPropertyByName("Name"));
                EfModelApi.ValueTypePrimitiveProperty<EfEntityObject_Attribute, int>(entity, typeMetadata.GetPropertyByName("Id"));
            }

            public void DeepListNestedObjects(HashSet<object> nestedObjects)
            {
                RuntimeTypeModelHelpers.DeepListNestedObjectCollection(m_Values_adapter, nestedObjects);
            }

            IEntityId IEntityObject.GetId()
            {
                return new EntityId<Interfaces.Repository1.IAttribute, int>(this.m_Id_storage);
            }

            void IEntityObject.SetId(object value)
            {
                this.m_Id_storage = (int)value;
            }

            // Properties
            IList<Interfaces.Repository1.IAttributeValue> Interfaces.Repository1.IAttribute.Values
            {
                get
                {
                    if (this.m_Values_state == DualValueStates.Storage)
                    {
                        this.m_Values_concrete = JsonConvert.DeserializeObject<List<EfEntityObject_AttributeValue>>(this.m_Values_storage);
                        this.m_Values_adapter = new ConcreteToAbstractListAdapter<EfEntityObject_AttributeValue, Interfaces.Repository1.IAttributeValue>(this.m_Values_concrete);
                        this.m_Values_state |= DualValueStates.Contract;
                    }
                    return this.m_Values_adapter;
                }
            }

            public virtual int Id
            {
                get
                {
                    return this.m_Id_storage;
                }
                set
                {
                    this.m_Id_storage = value;
                }
            }

            int IEntityPartId<int>.Id
            {
                get
                {
                    return this.m_Id_storage;
                }
            }

            Type IObject.ContractType
            {
                get
                {
                    return typeof(Interfaces.Repository1.IAttribute);
                }
            }

            Type IObject.FactoryType
            {
                get { return typeof(EfEntityObjectFactory); }
            }

            public virtual string Name
            {
                get
                {
                    return this.m_Name;
                }
                set
                {
                    this.m_Name = value;
                }
            }

            public virtual string TitleForUser
            {
                get
                {
                    return this.m_TitleForUser;
                }
                set
                {
                    this.m_TitleForUser = value;
                }
            }

            public virtual string Values
            {
                get
                {
                    if (this.m_Values_state == DualValueStates.Contract)
                    {
                        this.m_Values_storage = JsonConvert.SerializeObject(this.m_Values_concrete);
                        this.m_Values_state |= DualValueStates.Storage;
                    }
                    return this.m_Values_storage;
                }
                set
                {
                    this.m_Values_storage = value;
                    this.m_Values_state = DualValueStates.Storage;
                }
            }

            public virtual ICollection<EfEntityObject_Product> Inverse_Product_Attributes { get; set; }

            #region Implementation of IContainedIn<out IDomainObject>

            private IDomainObject _domainObject;

            public IDomainObject GetContainerObject()
            {
                return _domainObject;
            }

            public void SetContainerObject(IDomainObject container)
            {
                _domainObject = container;
            }

            #endregion
        }

        public class EfEntityObject_AttributeValue : Interfaces.Repository1.IAttributeValue, IObject, IEntityPartObject
        {
            // Fields
            private int m_DisplayOrder;
            private string m_Value;

            // Methods
            public EfEntityObject_AttributeValue()
            {
            }

            public EfEntityObject_AttributeValue(IComponentContext arg0)
            {
            }

            public static object FactoryMethod_1()
            {
                return new EfEntityObject_AttributeValue();
            }

            public static object FactoryMethod_2(IComponentContext context1)
            {
                return new EfEntityObject_AttributeValue(context1);
            }

            public static void ConfigureEfModel(ITypeMetadataCache metadataCache, DbModelBuilder modelBuilder)
            {
                EfModelApi.ComplexType<EfEntityObject_AttributeValue>(modelBuilder);
            }

            // Properties
            public virtual int DisplayOrder
            {
                get
                {
                    return this.m_DisplayOrder;
                }
                set
                {
                    this.m_DisplayOrder = value;
                }
            }

            Type IObject.ContractType
            {
                get
                {
                    return typeof(Interfaces.Repository1.IAttributeValue);
                }
            }

            Type IObject.FactoryType
            {
                get { return typeof(EfEntityObjectFactory); }
            }

            public virtual string Value
            {
                get
                {
                    return this.m_Value;
                }
                set
                {
                    this.m_Value = value;
                }
            }

            #region Implementation of IContainedIn<out IDomainObject>

            private IDomainObject _domainObject;

            public IDomainObject GetContainerObject()
            {
                return _domainObject;
            }

            public void SetContainerObject(IDomainObject container)
            {
                _domainObject = container;
            }

            #endregion
        }

        public class EfEntityObject_AttributeValueChoice : Interfaces.Repository1.IAttributeValueChoice, IObject, IEntityPartObject, IHaveNestedObjects
        {
            // Fields
            private EfEntityObject_Attribute m_Attribute_storage;
            private string m_Value;

            // Methods
            public EfEntityObject_AttributeValueChoice()
            {
            }

            public EfEntityObject_AttributeValueChoice(IComponentContext arg0)
            {
            }

            public static object FactoryMethod_1()
            {
                return new EfEntityObject_AttributeValueChoice();
            }

            public static object FactoryMethod_2(IComponentContext context1)
            {
                return new EfEntityObject_AttributeValueChoice(context1);
            }

            public static void ConfigureEfModel(ITypeMetadataCache metadataCache, DbModelBuilder modelBuilder)
            {
                EfModelApi.ComplexType<EfEntityObject_AttributeValueChoice>(modelBuilder);
            }

            public void DeepListNestedObjects(HashSet<object> nestedObjects)
            {
                nestedObjects.Add(this.m_Attribute_storage);
                this.m_Attribute_storage.DeepListNestedObjects(nestedObjects);
            }

            // Properties
            [NotMapped]
            public virtual EfEntityObject_Attribute Attribute
            {
                get
                {
                    return this.m_Attribute_storage;
                }
                set
                {
                    this.m_Attribute_storage = value;
                }
            }

            Interfaces.Repository1.IAttribute Interfaces.Repository1.IAttributeValueChoice.Attribute
            {
                get
                {
                    return this.m_Attribute_storage;
                }
                set
                {
                    this.m_Attribute_storage = (EfEntityObject_Attribute)value;
                }
            }

            Type IObject.ContractType
            {
                get
                {
                    return typeof(Interfaces.Repository1.IAttributeValueChoice);
                }
            }

            Type IObject.FactoryType
            {
                get { return typeof(EfEntityObjectFactory); }
            }

            public virtual string Value
            {
                get
                {
                    return this.m_Value;
                }
                set
                {
                    this.m_Value = value;
                }
            }

            #region Implementation of IContainedIn<out IDomainObject>

            private IDomainObject _domainObject;

            public IDomainObject GetContainerObject()
            {
                return _domainObject;
            }

            public void SetContainerObject(IDomainObject container)
            {
                _domainObject = container;
            }

            #endregion
        }

        public class EfEntityObject_Category : Interfaces.Repository1.ICategory, IEntityPartUniqueDisplayName, IEntityPartId<int>, IObject, IEntityObject
        {
            // Fields
            private int m_Id_storage;
            private string m_Name;

            // Methods
            public EfEntityObject_Category()
            {
            }

            public EfEntityObject_Category(IComponentContext arg0)
            {
                this.m_Id_storage = arg0.Resolve<TestIntIdValueGenerator>().GenerateValue("Category.Id");
            }

            public static object FactoryMethod_1()
            {
                return new EfEntityObject_Category();
            }

            public static object FactoryMethod_2(IComponentContext context1)
            {
                return new EfEntityObject_Category(context1);
            }

            public static void ConfigureEfModel(ITypeMetadataCache metadataCache, DbModelBuilder modelBuilder)
            {
                ITypeMetadata typeMetadata = metadataCache.GetTypeMetadata(typeof(Interfaces.Repository1.ICategory));
                EntityTypeConfiguration<EfEntityObject_Category> entity = EfModelApi.EntityType<EfEntityObject_Category>(modelBuilder, typeMetadata, metadataCache);
                EfModelApi.StringProperty<EfEntityObject_Category>(entity, typeMetadata.GetPropertyByName("Name"));
                EfModelApi.ValueTypePrimitiveProperty<EfEntityObject_Category, int>(entity, typeMetadata.GetPropertyByName("Id"));
            }

            IEntityId IEntityObject.GetId()
            {
                return new EntityId<Interfaces.Repository1.ICategory, int>(this.m_Id_storage);
            }

            void IEntityObject.SetId(object value)
            {
                this.m_Id_storage = (int)value;
            }

            // Properties
            public virtual int Id
            {
                get
                {
                    return this.m_Id_storage;
                }
                set
                {
                    this.m_Id_storage = value;
                }
            }

            int IEntityPartId<int>.Id
            {
                get
                {
                    return this.m_Id_storage;
                }
            }

            Type IObject.ContractType
            {
                get
                {
                    return typeof(Interfaces.Repository1.ICategory);
                }
            }

            Type IObject.FactoryType
            {
                get { return typeof(EfEntityObjectFactory); }
            }

            public virtual string Name
            {
                get
                {
                    return this.m_Name;
                }
                set
                {
                    this.m_Name = value;
                }
            }

            public virtual ICollection<EfEntityObject_Product> Inverse_Product_Categories { get; set; }

            #region Implementation of IContainedIn<out IDomainObject>

            private IDomainObject _domainObject;

            public IDomainObject GetContainerObject()
            {
                return _domainObject;
            }

            public void SetContainerObject(IDomainObject container)
            {
                _domainObject = container;
            }

            #endregion
        }

        public class EfEntityObject_Order : Interfaces.Repository1.IOrder, IEntityPartId<int>, IObject, IEntityObject, IHaveNestedObjects
        {
            // Fields
            private EfEntityObject_PostalAddress m_BillingAddress_storage;
            private EfEntityObject_PostalAddress m_DeliveryAddress_storage;
            private int m_Id_storage;
            private ConcreteToAbstractCollectionAdapter<EfEntityObject_OrderLine, Interfaces.Repository1.IOrderLine> m_OrderLines_adapter;
            private HashSet<EfEntityObject_OrderLine> m_OrderLines_storage;
            private string m_OrderNo;
            private DateTime m_PlacedAt;
            private Interfaces.Repository1.OrderStatus m_Status;

            // Methods
            public EfEntityObject_Order()
            {
            }

            public EfEntityObject_Order(IComponentContext arg0)
            {
                this.m_OrderLines_storage = new HashSet<EfEntityObject_OrderLine>();
                this.m_OrderLines_adapter = new ConcreteToAbstractCollectionAdapter<EfEntityObject_OrderLine, Interfaces.Repository1.IOrderLine>(this.m_OrderLines_storage);
                this.m_DeliveryAddress_storage = new EfEntityObject_PostalAddress(arg0);
                this.m_BillingAddress_storage = new EfEntityObject_PostalAddress(arg0);
                this.m_Status = Interfaces.Repository1.OrderStatus.New;
                this.m_Id_storage = arg0.Resolve<TestIntIdValueGenerator>().GenerateValue("Order.Id");
            }

            public static object FactoryMethod_1()
            {
                return new EfEntityObject_Order();
            }

            public static object FactoryMethod_2(IComponentContext context1)
            {
                return new EfEntityObject_Order(context1);
            }

            public static void ConfigureEfModel(ITypeMetadataCache metadataCache, DbModelBuilder modelBuilder)
            {
                ITypeMetadata typeMetadata = metadataCache.GetTypeMetadata(typeof(Interfaces.Repository1.IOrder));
                EntityTypeConfiguration<EfEntityObject_Order> entity = EfModelApi.EntityType<EfEntityObject_Order>(modelBuilder, typeMetadata, metadataCache);
                EfModelApi.StringProperty<EfEntityObject_Order>(entity, typeMetadata.GetPropertyByName("OrderNo"));
                EfModelApi.ValueTypePrimitiveProperty<EfEntityObject_Order, DateTime>(entity, typeMetadata.GetPropertyByName("PlacedAt"));
                EfModelApi.ValueTypePrimitiveProperty<EfEntityObject_Order, Interfaces.Repository1.OrderStatus>(entity, typeMetadata.GetPropertyByName("Status"));
                EfModelApi.ValueTypePrimitiveProperty<EfEntityObject_Order, int>(entity, typeMetadata.GetPropertyByName("Id"));

                modelBuilder.ComplexType<EfEntityObject_PostalAddress>();
                var complexMetaProperty1 = typeMetadata.GetPropertyByName("BillingAddress");
                EfModelApi.ComplexTypeProperty<EfEntityObject_Order, string>(modelBuilder, entity, complexMetaProperty1, complexMetaProperty1.Relation.RelatedPartyType.GetPropertyByName("City"));
                EfModelApi.ComplexTypeProperty<EfEntityObject_Order, string>(modelBuilder, entity, complexMetaProperty1, complexMetaProperty1.Relation.RelatedPartyType.GetPropertyByName("Country"));
                EfModelApi.ComplexTypeProperty<EfEntityObject_Order, string>(modelBuilder, entity, complexMetaProperty1, complexMetaProperty1.Relation.RelatedPartyType.GetPropertyByName("StreetAddress"));
                EfModelApi.ComplexTypeProperty<EfEntityObject_Order, string>(modelBuilder, entity, complexMetaProperty1, complexMetaProperty1.Relation.RelatedPartyType.GetPropertyByName("ZipCode"));

                modelBuilder.ComplexType<EfEntityObject_PostalAddress>();
                var complexMetaProperty2 = typeMetadata.GetPropertyByName("DeliveryAddress");
                EfModelApi.ComplexTypeProperty<EfEntityObject_Order, string>(modelBuilder, entity, complexMetaProperty2, complexMetaProperty2.Relation.RelatedPartyType.GetPropertyByName("City"));
                EfModelApi.ComplexTypeProperty<EfEntityObject_Order, string>(modelBuilder, entity, complexMetaProperty2, complexMetaProperty2.Relation.RelatedPartyType.GetPropertyByName("Country"));
                EfModelApi.ComplexTypeProperty<EfEntityObject_Order, string>(modelBuilder, entity, complexMetaProperty2, complexMetaProperty2.Relation.RelatedPartyType.GetPropertyByName("StreetAddress"));
                EfModelApi.ComplexTypeProperty<EfEntityObject_Order, string>(modelBuilder, entity, complexMetaProperty2, complexMetaProperty2.Relation.RelatedPartyType.GetPropertyByName("ZipCode"));

                //modelBuilder.ComplexType<EfEntityObject_PostalAddress>();
                //modelBuilder.Types<EfEntityObject_Order>().Configure(cfg => cfg.Property(x => x.BillingAddress.City).HasColumnName("bbb_aaa"));
            }

            public void DeepListNestedObjects(HashSet<object> nestedObjects)
            {
                RuntimeTypeModelHelpers.DeepListNestedObjectCollection(m_OrderLines_adapter, nestedObjects);
                RuntimeTypeModelHelpers.DeepListNestedObject(m_DeliveryAddress_storage, nestedObjects);
                RuntimeTypeModelHelpers.DeepListNestedObject(m_BillingAddress_storage, nestedObjects);
            }

            IEntityId IEntityObject.GetId()
            {
                return new EntityId<Interfaces.Repository1.IOrder, int>(this.m_Id_storage);
            }

            void IEntityObject.SetId(object value)
            {
                this.m_Id_storage = (int)value;
            }

            // Properties
            public virtual EfEntityObject_PostalAddress BillingAddress
            {
                get
                {
                    return this.m_BillingAddress_storage;
                }
                set
                {
                    this.m_BillingAddress_storage = value;
                }
            }

            public virtual EfEntityObject_PostalAddress DeliveryAddress
            {
                get
                {
                    return this.m_DeliveryAddress_storage;
                }
                set
                {
                    this.m_DeliveryAddress_storage = value;
                }
            }

            public virtual int Id
            {
                get
                {
                    return this.m_Id_storage;
                }
                set
                {
                    this.m_Id_storage = value;
                }
            }

            int IEntityPartId<int>.Id
            {
                get
                {
                    return this.m_Id_storage;
                }
            }

            Type IObject.ContractType
            {
                get
                {
                    return typeof(Interfaces.Repository1.IOrder);
                }
            }

            Interfaces.Repository1.IPostalAddress Interfaces.Repository1.IOrder.BillingAddress
            {
                get
                {
                    return this.m_BillingAddress_storage;
                }
            }

            Interfaces.Repository1.IPostalAddress Interfaces.Repository1.IOrder.DeliveryAddress
            {
                get
                {
                    return this.m_DeliveryAddress_storage;
                }
            }

            ICollection<Interfaces.Repository1.IOrderLine> Interfaces.Repository1.IOrder.OrderLines
            {
                get
                {
                    return this.m_OrderLines_adapter;
                }
            }

            Type IObject.FactoryType
            {
                get { return typeof(EfEntityObjectFactory); }
            }

            public virtual HashSet<EfEntityObject_OrderLine> OrderLines
            {
                get
                {
                    return this.m_OrderLines_storage;
                }
                set
                {
                    this.m_OrderLines_adapter = new ConcreteToAbstractCollectionAdapter<EfEntityObject_OrderLine, Interfaces.Repository1.IOrderLine>(value);
                    this.m_OrderLines_storage = value;
                }
            }

            public virtual string OrderNo
            {
                get
                {
                    return this.m_OrderNo;
                }
                set
                {
                    this.m_OrderNo = value;
                }
            }

            public virtual DateTime PlacedAt
            {
                get
                {
                    return this.m_PlacedAt;
                }
                set
                {
                    this.m_PlacedAt = value;
                }
            }

            public virtual Interfaces.Repository1.OrderStatus Status
            {
                get
                {
                    return this.m_Status;
                }
                set
                {
                    this.m_Status = value;
                }
            }

            #region Implementation of IContainedIn<out IDomainObject>

            private IDomainObject _domainObject;

            public IDomainObject GetContainerObject()
            {
                return _domainObject;
            }

            public void SetContainerObject(IDomainObject container)
            {
                _domainObject = container;
            }

            #endregion

            public Interfaces.Repository1.ICustomer Customer { get; set; }
        }

        public class EfEntityObject_OrderLine : Interfaces.Repository1.IOrderLine, IEntityPartId<int>, IObject, IEntityObject, IHaveNestedObjects
        {
            // Fields
            private ConcreteToAbstractCollectionAdapter<EfEntityObject_AttributeValueChoice, Interfaces.Repository1.IAttributeValueChoice> m_Attributes_adapter;
            private HashSet<EfEntityObject_AttributeValueChoice> m_Attributes_concrete;
            private DualValueStates m_Attributes_state;
            private string m_Attributes_storage;
            private int m_Id_storage;
            private EfEntityObject_Order m_Order_storage;
            private EfEntityObject_Product m_Product_storage;
            private int m_Quantity;

            // Methods
            public EfEntityObject_OrderLine()
            {
            }

            public EfEntityObject_OrderLine(IComponentContext arg0)
            {
                this.m_Attributes_concrete = new HashSet<EfEntityObject_AttributeValueChoice>();
                this.m_Attributes_adapter = new ConcreteToAbstractCollectionAdapter<EfEntityObject_AttributeValueChoice, Interfaces.Repository1.IAttributeValueChoice>(this.m_Attributes_concrete);
                this.m_Attributes_state = DualValueStates.Contract;
                this.m_Id_storage = arg0.Resolve<TestIntIdValueGenerator>().GenerateValue("OrderLine.Id");
            }

            public static object FactoryMethod_1()
            {
                return new EfEntityObject_OrderLine();
            }

            public static object FactoryMethod_2(IComponentContext context1)
            {
                return new EfEntityObject_OrderLine(context1);
            }

            public static void ConfigureEfModel(ITypeMetadataCache metadataCache, DbModelBuilder modelBuilder)
            {
                ITypeMetadata typeMetadata = metadataCache.GetTypeMetadata(typeof(Interfaces.Repository1.IOrderLine));
                EntityTypeConfiguration<EfEntityObject_OrderLine> manyEntity = EfModelApi.EntityType<EfEntityObject_OrderLine>(modelBuilder, typeMetadata, metadataCache);
                EfModelApi.ManyToOneRelationProperty<EfEntityObject_OrderLine, EfEntityObject_Order>(manyEntity, typeMetadata.GetPropertyByName("Order"));
                
                //BEGIN CHANGE
                //-added:
                EfModelApi.ManyToOneRelationProperty<EfEntityObject_OrderLine, EfEntityObject_Product>(manyEntity, typeMetadata.GetPropertyByName("Product"));
                //END CHANGE

                EfModelApi.ValueTypePrimitiveProperty<EfEntityObject_OrderLine, int>(manyEntity, typeMetadata.GetPropertyByName("Quantity"));
                EfModelApi.StringProperty<EfEntityObject_OrderLine>(manyEntity, typeMetadata.GetPropertyByName("Attributes"));
                EfModelApi.ValueTypePrimitiveProperty<EfEntityObject_OrderLine, int>(manyEntity, typeMetadata.GetPropertyByName("Id"));
            }

            public void DeepListNestedObjects(HashSet<object> nestedObjects)
            {
                nestedObjects.Add(this.m_Order_storage);
                nestedObjects.Add(this.m_Product_storage);
                this.m_Product_storage.DeepListNestedObjects(nestedObjects);
                nestedObjects.UnionWith(this.m_Attributes_adapter.Cast<object>());
                this.m_Attributes_adapter.OfType<IHaveNestedObjects>();
                IEnumerator<IHaveNestedObjects> enumerator = this.m_Attributes_adapter.OfType<IHaveNestedObjects>().GetEnumerator();
                using (enumerator)
                {
                    while (enumerator.MoveNext())
                    {
                        enumerator.Current.DeepListNestedObjects(nestedObjects);
                    }
                }
            }

            IEntityId IEntityObject.GetId()
            {
                return new EntityId<Interfaces.Repository1.IOrderLine, int>(this.m_Id_storage);
            }

            void IEntityObject.SetId(object value)
            {
                this.m_Id_storage = (int)value;
            }

            // Properties
            public virtual string Attributes
            {
                get
                {
                    if (this.m_Attributes_state == DualValueStates.Contract)
                    {
                        this.m_Attributes_storage = JsonConvert.SerializeObject(this.m_Attributes_concrete);
                        this.m_Attributes_state |= DualValueStates.Storage;
                    }
                    return this.m_Attributes_storage;
                }
                set
                {
                    this.m_Attributes_storage = value;
                    this.m_Attributes_state = DualValueStates.Storage;
                }
            }

            public virtual int Id
            {
                get
                {
                    return this.m_Id_storage;
                }
                set
                {
                    this.m_Id_storage = value;
                }
            }

            int IEntityPartId<int>.Id
            {
                get
                {
                    return this.m_Id_storage;
                }
            }

            Type IObject.ContractType
            {
                get
                {
                    return typeof(Interfaces.Repository1.IOrderLine);
                }
            }

            Type IObject.FactoryType
            {
                get { return typeof(EfEntityObjectFactory); }
            }

            ICollection<Interfaces.Repository1.IAttributeValueChoice> Interfaces.Repository1.IOrderLine.Attributes
            {
                get
                {
                    if (this.m_Attributes_state == DualValueStates.Storage)
                    {
                        this.m_Attributes_concrete = JsonConvert.DeserializeObject<HashSet<EfEntityObject_AttributeValueChoice>>(this.m_Attributes_storage);
                        this.m_Attributes_adapter = new ConcreteToAbstractCollectionAdapter<EfEntityObject_AttributeValueChoice, Interfaces.Repository1.IAttributeValueChoice>(this.m_Attributes_concrete);
                        this.m_Attributes_state |= DualValueStates.Contract;
                    }
                    return this.m_Attributes_adapter;
                }
            }

            Interfaces.Repository1.IOrder Interfaces.Repository1.IOrderLine.Order
            {
                get
                {
                    return this.m_Order_storage;
                }
                set
                {
                    this.m_Order_storage = (EfEntityObject_Order)value;
                }
            }

            Interfaces.Repository1.IProduct Interfaces.Repository1.IOrderLine.Product
            {
                get
                {
                    return this.m_Product_storage;
                }
                set
                {
                    this.m_Product_storage = (EfEntityObject_Product)value;
                }
            }

            public virtual EfEntityObject_Order Order
            {
                get
                {
                    return this.m_Order_storage;
                }
                set
                {
                    this.m_Order_storage = value;
                }
            }

            public virtual EfEntityObject_Product Product
            {
                get
                {
                    return this.m_Product_storage;
                }
                set
                {
                    this.m_Product_storage = value;
                }
            }

            public virtual int Quantity
            {
                get
                {
                    return this.m_Quantity;
                }
                set
                {
                    this.m_Quantity = value;
                }
            }

            #region Implementation of IContainedIn<out IDomainObject>

            private IDomainObject _domainObject;

            public IDomainObject GetContainerObject()
            {
                return _domainObject;
            }

            public void SetContainerObject(IDomainObject container)
            {
                _domainObject = container;
            }

            #endregion
        }

        public class EfEntityObject_PostalAddress : Interfaces.Repository1.IPostalAddress, IObject, IEntityPartObject
        {
            // Fields
            private string m_City;
            private string m_Country;
            private string m_StreetAddress;
            private string m_ZipCode;

            // Methods
            public EfEntityObject_PostalAddress()
            {
            }

            public EfEntityObject_PostalAddress(IComponentContext arg0)
            {
            }

            public static object FactoryMethod_1()
            {
                return new EfEntityObject_PostalAddress();
            }

            public static object FactoryMethod_2(IComponentContext context1)
            {
                return new EfEntityObject_PostalAddress(context1);
            }

            public static void ConfigureEfModel(ITypeMetadataCache metadataCache, DbModelBuilder modelBuilder)
            {
                EfModelApi.ComplexType<EfEntityObject_PostalAddress>(modelBuilder);
            }

            // Properties
            public virtual string City
            {
                get
                {
                    return this.m_City;
                }
                set
                {
                    this.m_City = value;
                }
            }

            public virtual string Country
            {
                get
                {
                    return this.m_Country;
                }
                set
                {
                    this.m_Country = value;
                }
            }

            Type IObject.ContractType
            {
                get
                {
                    return typeof(Interfaces.Repository1.IPostalAddress);
                }
            }

            Type IObject.FactoryType
            {
                get { return typeof(EfEntityObjectFactory); }
            }

            public virtual string StreetAddress
            {
                get
                {
                    return this.m_StreetAddress;
                }
                set
                {
                    this.m_StreetAddress = value;
                }
            }

            public virtual string ZipCode
            {
                get
                {
                    return this.m_ZipCode;
                }
                set
                {
                    this.m_ZipCode = value;
                }
            }

            #region Implementation of IContainedIn<out IDomainObject>

            private IDomainObject _domainObject;

            public IDomainObject GetContainerObject()
            {
                return _domainObject;
            }

            public void SetContainerObject(IDomainObject container)
            {
                _domainObject = container;
            }

            #endregion
        }

        public class EfEntityObject_Product : Interfaces.Repository1.IProduct, IEntityPartId<int>, IObject, IEntityObject, IHaveNestedObjects
        {
            // Fields
            private ConcreteToAbstractCollectionAdapter<EfEntityObject_Attribute, Interfaces.Repository1.IAttribute> m_Attributes_adapter;
            private HashSet<EfEntityObject_Attribute> m_Attributes_storage;
            private string m_CatalogNo;
            private ConcreteToAbstractCollectionAdapter<EfEntityObject_Category, Interfaces.Repository1.ICategory> m_Categories_adapter;
            private HashSet<EfEntityObject_Category> m_Categories_storage;
            private int m_Id_storage;
            private string m_Name;
            private decimal m_Price;

            // Methods
            public EfEntityObject_Product()
            {
            }

            public EfEntityObject_Product(IComponentContext arg0)
            {
                this.m_Categories_storage = new HashSet<EfEntityObject_Category>();
                this.m_Categories_adapter = new ConcreteToAbstractCollectionAdapter<EfEntityObject_Category, Interfaces.Repository1.ICategory>(this.m_Categories_storage);
                this.m_Attributes_storage = new HashSet<EfEntityObject_Attribute>();
                this.m_Attributes_adapter = new ConcreteToAbstractCollectionAdapter<EfEntityObject_Attribute, Interfaces.Repository1.IAttribute>(this.m_Attributes_storage);
                this.m_Id_storage = arg0.Resolve<TestIntIdValueGenerator>().GenerateValue("Product.Id");
            }

            public static object FactoryMethod_1()
            {
                return new EfEntityObject_Product();
            }

            public static object FactoryMethod_2(IComponentContext context1)
            {
                return new EfEntityObject_Product(context1);
            }

            public static void ConfigureEfModel(ITypeMetadataCache metadataCache, DbModelBuilder modelBuilder)
            {
                ITypeMetadata typeMetadata = metadataCache.GetTypeMetadata(typeof(Interfaces.Repository1.IProduct));
                EntityTypeConfiguration<EfEntityObject_Product> entity = EfModelApi.EntityType<EfEntityObject_Product>(modelBuilder, typeMetadata, metadataCache);
                EfModelApi.StringProperty<EfEntityObject_Product>(entity, typeMetadata.GetPropertyByName("CatalogNo"));
                EfModelApi.StringProperty<EfEntityObject_Product>(entity, typeMetadata.GetPropertyByName("Name"));
                EfModelApi.ValueTypePrimitiveProperty<EfEntityObject_Product, decimal>(entity, typeMetadata.GetPropertyByName("Price"));

                //BEGIN CHANGE
                //-removed:
                //EfModelApi.ManyToOneRelationProperty<EfEntityObject_Product, EfEntityObject_Category>(entity, typeMetadata.GetPropertyByName("Categories"));
                //EfModelApi.ManyToOneRelationProperty<EfEntityObject_Product, EfEntityObject_Attribute>(entity, typeMetadata.GetPropertyByName("Attributes"));
                //-added:
                EfModelApi.ManyToManyRelationProperty<EfEntityObject_Product, EfEntityObject_Category>(entity, typeMetadata.GetPropertyByName("Categories"), metadataCache);
                EfModelApi.ManyToManyRelationProperty<EfEntityObject_Product, EfEntityObject_Attribute>(entity, typeMetadata.GetPropertyByName("Attributes"), metadataCache);
                
                //entity.HasMany(x => x.Attributes).WithMany(x => x.Inverse_Product_Categories).Map(cfg =>
                //{
                //    cfg.MapLeftKey("ProductId");
                //    cfg.MapRightKey("CategoryId");
                //    cfg.ToTable("Products_Categories");
                //});

                //entity.HasMany(x => x.Categories).WithMany(x => x.Inverse_Product_Attributes).Map(cfg => {
                //    cfg.MapLeftKey("ProductId");
                //    cfg.MapRightKey("AttributeId");
                //    cfg.ToTable("Products_Attributes");
                //});
                //END CHANGE

                EfModelApi.ValueTypePrimitiveProperty<EfEntityObject_Product, int>(entity, typeMetadata.GetPropertyByName("Id"));
            }

            public void DeepListNestedObjects(HashSet<object> nestedObjects)
            {
                RuntimeTypeModelHelpers.DeepListNestedObjectCollection(m_Categories_adapter, nestedObjects);
                RuntimeTypeModelHelpers.DeepListNestedObjectCollection(m_Attributes_adapter, nestedObjects);
            }

            IEntityId IEntityObject.GetId()
            {
                return new EntityId<Interfaces.Repository1.IProduct, int>(this.m_Id_storage);
            }

            void IEntityObject.SetId(object value)
            {
                this.m_Id_storage = (int)value;
            }

            // Properties
            public virtual HashSet<EfEntityObject_Attribute> Attributes
            {
                get
                {
                    return this.m_Attributes_storage;
                }
                set
                {
                    this.m_Attributes_adapter = new ConcreteToAbstractCollectionAdapter<EfEntityObject_Attribute, Interfaces.Repository1.IAttribute>(value);
                    this.m_Attributes_storage = value;
                }
            }

            public virtual string CatalogNo
            {
                get
                {
                    return this.m_CatalogNo;
                }
                set
                {
                    this.m_CatalogNo = value;
                }
            }

            public virtual HashSet<EfEntityObject_Category> Categories
            {
                get
                {
                    return this.m_Categories_storage;
                }
                set
                {
                    this.m_Categories_adapter = new ConcreteToAbstractCollectionAdapter<EfEntityObject_Category, Interfaces.Repository1.ICategory>(value);
                    this.m_Categories_storage = value;
                }
            }

            public virtual int Id
            {
                get
                {
                    return this.m_Id_storage;
                }
                set
                {
                    this.m_Id_storage = value;
                }
            }

            int IEntityPartId<int>.Id
            {
                get
                {
                    return this.m_Id_storage;
                }
            }

            Type IObject.ContractType
            {
                get
                {
                    return typeof(Interfaces.Repository1.IProduct);
                }
            }

            Type IObject.FactoryType
            {
                get { return typeof(EfEntityObjectFactory); }
            }

            ICollection<Interfaces.Repository1.IAttribute> Interfaces.Repository1.IProduct.Attributes
            {
                get
                {
                    return this.m_Attributes_adapter;
                }
            }

            ICollection<Interfaces.Repository1.ICategory> Interfaces.Repository1.IProduct.Categories
            {
                get
                {
                    return this.m_Categories_adapter;
                }
            }

            public virtual string Name
            {
                get
                {
                    return this.m_Name;
                }
                set
                {
                    this.m_Name = value;
                }
            }

            public virtual decimal Price
            {
                get
                {
                    return this.m_Price;
                }
                set
                {
                    this.m_Price = value;
                }
            }

            #region Implementation of IContainedIn<out IDomainObject>

            private IDomainObject _domainObject;

            public IDomainObject GetContainerObject()
            {
                return _domainObject;
            }

            public void SetContainerObject(IDomainObject container)
            {
                _domainObject = container;
            }

            #endregion
        }

        public class EfEntityObject_Customer : Interfaces.Repository1.ICustomer, IEntityPartId<int>, IObject, IEntityObject, IHaveNestedObjects
        {
            // Fields
            private ConcreteToAbstractCollectionAdapter<EfEntityObject_ContactDetail, Interfaces.Repository1.IContactDetail> m_ContactDetails_adapter;
            private HashSet<EfEntityObject_ContactDetail> m_ContactDetails_storage;
            private string m_FullName;
            private int m_Id_storage;

            // Methods
            public EfEntityObject_Customer()
            {
            }

            public EfEntityObject_Customer(IComponentContext arg0)
            {
                this.m_ContactDetails_storage = new HashSet<EfEntityObject_ContactDetail>();
                this.m_ContactDetails_adapter = (ConcreteToAbstractCollectionAdapter<EfEntityObject_ContactDetail, Interfaces.Repository1.IContactDetail>) RuntimeTypeModelHelpers.CreateCollectionAdapter<EfEntityObject_ContactDetail, Interfaces.Repository1.IContactDetail>(this.m_ContactDetails_storage, false);
                this.m_Id_storage = arg0.Resolve<TestIntIdValueGenerator>().GenerateValue("Customer.Id");
            }

            public static object FactoryMethod1()
            {
                return new EfEntityObject_Customer();
            }

            public static object FactoryMethod2(IComponentContext context1)
            {
                return new EfEntityObject_Customer(context1);
            }

            public static void ConfigureEfModel(ITypeMetadataCache metadataCache, DbModelBuilder modelBuilder)
            {
                ITypeMetadata typeMetadata = metadataCache.GetTypeMetadata(typeof(Interfaces.Repository1.ICustomer));
                EntityTypeConfiguration<EfEntityObject_Customer> entity = EfModelApi.EntityType<EfEntityObject_Customer>(modelBuilder, typeMetadata, metadataCache);
                EfModelApi.StringProperty<EfEntityObject_Customer>(entity, typeMetadata.GetPropertyByName("FullName"));
                EfModelApi.ValueTypePrimitiveProperty<EfEntityObject_Customer, int>(entity, typeMetadata.GetPropertyByName("Id"));
                
                EfModelApi.OneToManyNoInverseRelationProperty<EfEntityObject_Customer, EfEntityObject_ContactDetail>(entity, typeMetadata.GetPropertyByName("ContactDetails"));
                
                //modelBuilder.Entity<EfEntityObject_Customer>().HasMany(x => x.ContactDetails).WithRequired().Map(cfg => cfg.MapKey(typeMetadata.GetPropertyByName("ContactDetails").RelationalMapping.RelatedColumnName));
            }

            public void DeepListNestedObjects(HashSet<object> nestedObjects)
            {
                nestedObjects.UnionWith(this.m_ContactDetails_adapter.Cast<object>());
            }

            IEntityId IEntityObject.GetId()
            {
                return new EntityId<Interfaces.Repository1.ICustomer, int>(this.m_Id_storage);
            }

            void IEntityObject.SetId(object value)
            {
                this.m_Id_storage = (int) value;
            }

            // Properties
            public virtual HashSet<EfEntityObject_ContactDetail> ContactDetails
            {
                get
                {
                    return this.m_ContactDetails_storage;
                }
                set
                {
                    this.m_ContactDetails_adapter = (ConcreteToAbstractCollectionAdapter<EfEntityObject_ContactDetail, Interfaces.Repository1.IContactDetail>) RuntimeTypeModelHelpers.CreateCollectionAdapter<EfEntityObject_ContactDetail, Interfaces.Repository1.IContactDetail>(value, false);
                    this.m_ContactDetails_storage = value;
                }
            }

            public virtual string FullName
            {
                get
                {
                    return this.m_FullName;
                }
                set
                {
                    this.m_FullName = value;
                }
            }

            ICollection<Interfaces.Repository1.IContactDetail> Interfaces.Repository1.ICustomer.ContactDetails
            {
                get
                {
                    return this.m_ContactDetails_adapter;
                }
            }

            public virtual int Id
            {
                get
                {
                    return this.m_Id_storage;
                }
                set
                {
                    this.m_Id_storage = value;
                }
            }

            int IEntityPartId<int>.Id
            {
                get
                {
                    return this.m_Id_storage;
                }
            }

            Type IObject.ContractType
            {
                get
                {
                    return typeof(Interfaces.Repository1.ICustomer);
                }
            }

            Type IObject.FactoryType
            {
                get { return typeof(EfEntityObjectFactory); }
            }

            bool Interfaces.Repository1.ICustomer.QualifiesAsValuableCustomer()
            {
                throw new NotSupportedException();
            }

            bool Interfaces.Repository1.ICustomer.IsInteredtedIn(Interfaces.Repository1.IProduct product)
            {
                throw new NotSupportedException();
            }

            #region Implementation of IContainedIn<out IDomainObject>

            private IDomainObject _domainObject;

            public IDomainObject GetContainerObject()
            {
                return _domainObject;
            }

            public void SetContainerObject(IDomainObject container)
            {
                _domainObject = container;
            }

            #endregion
        }

        public abstract class EfEntityObject_ContactDetail : Interfaces.Repository1.IContactDetail, IEntityPartId<int>, IObject, IEntityObject
        {
            // Fields
            private int m_Id_storage;
            private bool m_IsPrimary;

            // Methods
            public EfEntityObject_ContactDetail()
            {
            }

            public EfEntityObject_ContactDetail(IComponentContext arg0)
            {
                this.m_Id_storage = arg0.Resolve<TestIntIdValueGenerator>().GenerateValue("ContactDetail.Id");
            }

            //public static object FactoryMethod1()
            //{
            //    return new EfEntityObject_ContactDetail();
            //}

            //public static object FactoryMethod2(IComponentContext context1)
            //{
            //    return new EfEntityObject_ContactDetail(context1);
            //}

            public static void ConfigureEfModel(ITypeMetadataCache metadataCache, DbModelBuilder modelBuilder)
            {
                ITypeMetadata typeMetadata = metadataCache.GetTypeMetadata(typeof(Interfaces.Repository1.IContactDetail));
                var entity = EfModelApi.EntityType<EfEntityObject_ContactDetail>(modelBuilder, typeMetadata, metadataCache);
                EfModelApi.ValueTypePrimitiveProperty<EfEntityObject_ContactDetail, int>(entity, typeMetadata.GetPropertyByName("Id"));
            }

            IEntityId IEntityObject.GetId()
            {
                return new EntityId<Interfaces.Repository1.IContactDetail, int>(this.m_Id_storage);
            }

            void IEntityObject.SetId(object value)
            {
                this.m_Id_storage = (int)value;
            }

            // Properties
            public virtual int Id
            {
                get
                {
                    return this.m_Id_storage;
                }
                set
                {
                    this.m_Id_storage = value;
                }
            }

            public virtual bool IsPrimary
            {
                get
                {
                    return this.m_IsPrimary;
                }
                set
                {
                    this.m_IsPrimary = value;
                }
            }

            int IEntityPartId<int>.Id
            {
                get
                {
                    return this.m_Id_storage;
                }
            }

            Type IObject.ContractType
            {
                get
                {
                    return typeof(Interfaces.Repository1.IContactDetail);
                }
            }

            Type IObject.FactoryType
            {
                get { return typeof(EfEntityObjectFactory); }
            }

            #region Implementation of IContainedIn<out IDomainObject>

            private IDomainObject _domainObject;

            public IDomainObject GetContainerObject()
            {
                return _domainObject;
            }

            public void SetContainerObject(IDomainObject container)
            {
                _domainObject = container;
            }

            #endregion
        }

        public class EfEntityObject_EmailContactDetail : EfEntityObject_ContactDetail, Interfaces.Repository1.IEmailContactDetail
        {
            // Fields
            private string m_Email;

            // Methods
            public EfEntityObject_EmailContactDetail()
            {
            }

            public EfEntityObject_EmailContactDetail(IComponentContext arg0) : base(arg0)
            {
            }

            public static object FactoryMethod1()
            {
                return new EfEntityObject_EmailContactDetail();
            }

            public static object FactoryMethod2(IComponentContext context1)
            {
                return new EfEntityObject_EmailContactDetail(context1);
            }

            public static new void ConfigureEfModel(ITypeMetadataCache metadataCache, DbModelBuilder modelBuilder)
            {
                ITypeMetadata typeMetadata = metadataCache.GetTypeMetadata(typeof(Interfaces.Repository1.IEmailContactDetail));
                var thisEntity = EfModelApi.InheritedEntityType<EfEntityObject_ContactDetail, EfEntityObject_EmailContactDetail>(modelBuilder, typeMetadata, "_t", "Email");
                EfModelApi.StringProperty(thisEntity, typeMetadata.GetPropertyByName("Email"));
            }

            // Properties
            public virtual string Email
            {
                get
                {
                    return this.m_Email;
                }
                set
                {
                    this.m_Email = value;
                }
            }
        }

        public class EfEntityObject_PhoneContactDetail : EfEntityObject_ContactDetail, Interfaces.Repository1.IPhoneContactDetail, IObject
        {
            #region Implementation of IPhoneContactDetail

            public string Phone { get; set; }

            #endregion
        }

        public class EfEntityObject_PostContactDetail : EfEntityObject_ContactDetail, Interfaces.Repository1.IPostContactDetail, IObject, IHaveNestedObjects
        {
            #region Implementation of IPostContactDetail

            public Interfaces.Repository1.IPostalAddress PostalAddress { get; private set; }

            #endregion

            #region Implementation of IHaveNestedObjects

            public void DeepListNestedObjects(HashSet<object> nestedObjects)
            {
                //throw new NotImplementedException();
            }

            #endregion
        }
    }
}
