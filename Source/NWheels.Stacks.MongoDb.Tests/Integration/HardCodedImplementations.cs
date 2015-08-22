using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Autofac;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using MongoDB.Driver.Linq;
using Newtonsoft.Json;
using NWheels.Concurrency;
using NWheels.Conventions.Core;
using NWheels.DataObjects;
using NWheels.DataObjects.Core;
using NWheels.DataObjects.Core.StorageTypes;
using NWheels.Entities;
using NWheels.Entities.Core;
using NWheels.Extensions;
using NWheels.Stacks.MongoDb.Factories;
using NWheels.Testing.Entities.Stacks;
using NWheels.TypeModel.Core;
using NWheels.TypeModel.Core.Factories;
using NWheels.Utilities;
using IR1 = NWheels.Testing.Entities.Stacks.Interfaces.Repository1;

namespace NWheels.Stacks.MongoDb.Tests.Integration
{
    public static class HardCodedImplementations
    {
        #region Entity Factory

        public class HardCodedEntityFactory : IEntityObjectFactory
        {
            private readonly IComponentContext _components;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public HardCodedEntityFactory(IComponentContext components)
            {
                _components = components;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public TEntityContract NewEntity<TEntityContract>() where TEntityContract : class
            {
                if (typeof(TEntityContract) == typeof(Interfaces.Repository1.IOrder))
                {
                    return (TEntityContract)(object)new MongoEntityObject_Order(_components);
                }
                if (typeof(TEntityContract) == typeof(Interfaces.Repository1.IOrderLine))
                {
                    return (TEntityContract)(object)new MongoEntityObject_OrderLine(_components);
                }
                if (typeof(TEntityContract) == typeof(Interfaces.Repository1.IProduct))
                {
                    return (TEntityContract)(object)new MongoEntityObject_Product(_components);
                }
                if (typeof(TEntityContract) == typeof(Interfaces.Repository1.ICategory))
                {
                    return (TEntityContract)(object)new MongoEntityObject_Category(_components);
                }
                if (typeof(TEntityContract) == typeof(Interfaces.Repository1.IAttribute))
                {
                    return (TEntityContract)(object)new MongoEntityObject_Attribute(_components);
                }
                if (typeof(TEntityContract) == typeof(Interfaces.Repository1.IAttributeValue))
                {
                    return (TEntityContract)(object)new MongoEntityObject_AttributeValue(_components);
                }
                if (typeof(TEntityContract) == typeof(Interfaces.Repository1.IAttributeValueChoice))
                {
                    return (TEntityContract)(object)new MongoEntityObject_AttributeValueChoice(_components);
                }
                if (typeof(TEntityContract) == typeof(Interfaces.Repository1.IPostalAddress))
                {
                    return (TEntityContract)(object)new MongoEntityObject_PostalAddress(_components);
                }
                if (typeof(TEntityContract) == typeof(Interfaces.Repository1.ICustomer))
                {
                    return (TEntityContract)(object)new MongoEntityObject_Customer(_components);
                }
                if (typeof(TEntityContract) == typeof(Interfaces.Repository1.IEmailContactDetail))
                {
                    return (TEntityContract)(object)new MongoEntityObject_EmailContactDetail(_components);
                }
                if (typeof(TEntityContract) == typeof(Interfaces.Repository1.IPhoneContactDetail))
                {
                    return (TEntityContract)(object)new MongoEntityObject_PhoneContactDetail(_components);
                }
                if (typeof(TEntityContract) == typeof(Interfaces.Repository1.IPostContactDetail))
                {
                    return (TEntityContract)(object)new MongoEntityObject_PostContactDetail(_components);
                }

                throw new NotSupportedException(
                    string.Format("Entity contract '{0}' is not supported by HardCodedEntityFactory.", typeof(TEntityContract).Name));
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public object NewEntity(Type entityContractType)
            {
                throw new NotImplementedException();
            }
        }

        #endregion

        #region Generated Code

        public class MongoDataRepository_OnlineStoreRepository : MongoDataRepositoryBase, Interfaces.Repository1.IOnlineStoreRepository
        {
            private IDomainObjectFactory _domainFactory;
            private ITypeMetadataCache _metadataCache;
            private static object _s_compiledModel;
            private static object _s_compiledModelSyncRoot = new object();
            public IEntityObjectFactory EntityFactory;
            private IEntityRepository<Interfaces.Repository1.IAttribute> m_Attributes;
            private IEntityRepository<Interfaces.Repository1.ICategory> m_Categories;
            private IEntityRepository<Interfaces.Repository1.IContactDetail> m_ContactDetails;
            private IEntityRepository<Interfaces.Repository1.IEmailContactDetail> m_ContactEmails;
            private IEntityRepository<Interfaces.Repository1.ICustomer> m_Customers;
            private IEntityRepository<Interfaces.Repository1.IOrder> m_Orders;
            private IEntityRepository<Interfaces.Repository1.IOrderLine> m_OrdersLines;
            private IEntityRepository<Interfaces.Repository1.IProduct> m_Products;

            public MongoDataRepository_OnlineStoreRepository(IResourceConsumerScopeHandle arg0, IComponentContext arg1, IEntityObjectFactory arg2, ITypeMetadataCache arg3, MongoDatabase arg4, bool arg5)
                : base(arg0, arg1, arg2, GetOrBuildDbCompiledModel(arg3, arg4), arg4, arg5)
            {
                this.EntityFactory = arg2;
                this._domainFactory = arg1.Resolve<IDomainObjectFactory>();
                this._metadataCache = arg3;
                this.m_Categories = new MongoEntityRepository<Interfaces.Repository1.ICategory, MongoEntityObject_Category>(this, this._metadataCache, this.EntityFactory);
                base.RegisterEntityRepository<Interfaces.Repository1.ICategory, MongoEntityObject_Category>(this.m_Categories);
                this.m_Products = new MongoEntityRepository<Interfaces.Repository1.IProduct, MongoEntityObject_Product>(this, this._metadataCache, this.EntityFactory);
                base.RegisterEntityRepository<Interfaces.Repository1.IProduct, MongoEntityObject_Product>(this.m_Products);
                this.m_Orders = new MongoEntityRepository<Interfaces.Repository1.IOrder, MongoEntityObject_Order>(this, this._metadataCache, this.EntityFactory);
                base.RegisterEntityRepository<Interfaces.Repository1.IOrder, MongoEntityObject_Order>(this.m_Orders);
                this.m_OrdersLines = new MongoEntityRepository<Interfaces.Repository1.IOrderLine, MongoEntityObject_OrderLine>(this, this._metadataCache, this.EntityFactory);
                base.RegisterEntityRepository<Interfaces.Repository1.IOrderLine, MongoEntityObject_OrderLine>(this.m_OrdersLines);
                this.m_Attributes = new MongoEntityRepository<Interfaces.Repository1.IAttribute, MongoEntityObject_Attribute>(this, this._metadataCache, this.EntityFactory);
                base.RegisterEntityRepository<Interfaces.Repository1.IAttribute, MongoEntityObject_Attribute>(this.m_Attributes);
                this.m_Customers = new MongoEntityRepository<Interfaces.Repository1.ICustomer, MongoEntityObject_Customer>(this, this._metadataCache, this.EntityFactory);
                base.RegisterEntityRepository<Interfaces.Repository1.ICustomer, MongoEntityObject_Customer>(this.m_Customers);
                this.m_ContactDetails = new MongoEntityRepository<Interfaces.Repository1.IContactDetail, MongoEntityObject_ContactDetail>(this, this._metadataCache, this.EntityFactory);
                base.RegisterEntityRepository<Interfaces.Repository1.IContactDetail, MongoEntityObject_ContactDetail>(this.m_ContactDetails);
                this.m_ContactEmails = new MongoEntityRepository<Interfaces.Repository1.IEmailContactDetail, MongoEntityObject_EmailContactDetail>(this, this._metadataCache, this.EntityFactory);
                base.RegisterEntityRepository<Interfaces.Repository1.IEmailContactDetail, MongoEntityObject_EmailContactDetail>(this.m_ContactEmails);
            }

            public static object FactoryMethod1(IResourceConsumerScopeHandle handle1, IComponentContext context1, EntityObjectFactory factory1, ITypeMetadataCache cache1, MongoDatabase database1, bool flag1)
            {
                return new MongoDataRepository_OnlineStoreRepository(handle1, context1, factory1, cache1, database1, flag1);
            }

            public sealed override Type[] GetEntityContractsInRepository()
            {
                return new Type[] { typeof(Interfaces.Repository1.ICategory), typeof(Interfaces.Repository1.IProduct), typeof(Interfaces.Repository1.IOrder), typeof(Interfaces.Repository1.IOrderLine), typeof(Interfaces.Repository1.IAttribute), typeof(Interfaces.Repository1.ICustomer), typeof(Interfaces.Repository1.IContactDetail), typeof(Interfaces.Repository1.IEmailContactDetail), typeof(Interfaces.Repository1.IAttributeValue), typeof(Interfaces.Repository1.IAttributeValueChoice), typeof(Interfaces.Repository1.IPostalAddress), typeof(Interfaces.Repository1.IPhoneContactDetail), typeof(Interfaces.Repository1.IPostContactDetail) };
            }

            public sealed override IEntityRepository[] GetEntityRepositories()
            {
                IEntityRepository[] repositoryArray = new IEntityRepository[13];
                repositoryArray[0] = (IEntityRepository)this.m_Categories;
                repositoryArray[1] = (IEntityRepository)this.m_Products;
                repositoryArray[2] = (IEntityRepository)this.m_Orders;
                repositoryArray[3] = (IEntityRepository)this.m_OrdersLines;
                repositoryArray[4] = (IEntityRepository)this.m_Attributes;
                repositoryArray[5] = (IEntityRepository)this.m_Customers;
                repositoryArray[6] = (IEntityRepository)this.m_ContactDetails;
                repositoryArray[7] = (IEntityRepository)this.m_ContactEmails;
                repositoryArray[8] = null;
                repositoryArray[9] = null;
                repositoryArray[10] = null;
                repositoryArray[11] = null;
                repositoryArray[12] = null;
                return repositoryArray;
            }

            public sealed override Type[] GetEntityTypesInRepository()
            {
                return new Type[] { typeof(MongoEntityObject_Category), typeof(MongoEntityObject_Product), typeof(MongoEntityObject_Order), typeof(MongoEntityObject_OrderLine), typeof(MongoEntityObject_Attribute), typeof(MongoEntityObject_Customer), typeof(MongoEntityObject_ContactDetail), typeof(MongoEntityObject_EmailContactDetail), typeof(MongoEntityObject_AttributeValue), typeof(MongoEntityObject_AttributeValueChoice), typeof(MongoEntityObject_PostalAddress), typeof(MongoEntityObject_PhoneContactDetail), typeof(MongoEntityObject_PostContactDetail) };
            }

            public static object GetOrBuildDbCompiledModel(ITypeMetadataCache metadataCache, MongoDatabase connection)
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
                            BsonClassMap.RegisterClassMap<MongoEntityObject_ContactDetail>();
                            BsonClassMap.RegisterClassMap<MongoEntityObject_EmailContactDetail>();
                            BsonClassMap.RegisterClassMap<MongoEntityObject_PhoneContactDetail>();
                            BsonClassMap.RegisterClassMap<MongoEntityObject_PostContactDetail>();

                            BsonSerializer.RegisterDiscriminatorConvention(typeof(MongoEntityObject_ContactDetail), new PolymorphicDiscriminatorConvention("_polyt"));
                            BsonSerializer.RegisterDiscriminatorConvention(typeof(MongoEntityObject_EmailContactDetail), new PolymorphicDiscriminatorConvention("_polyt"));
                            BsonSerializer.RegisterDiscriminatorConvention(typeof(MongoEntityObject_PhoneContactDetail), new PolymorphicDiscriminatorConvention("_polyt"));
                            BsonSerializer.RegisterDiscriminatorConvention(typeof(MongoEntityObject_PostContactDetail), new PolymorphicDiscriminatorConvention("_polyt"));

                            _s_compiledModel = new object();
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
                Interfaces.Repository1.IAttributeValue value2 = this._domainFactory.CreateDomainObjectInstance<Interfaces.Repository1.IAttributeValue>(this.EntityFactory.NewEntity<Interfaces.Repository1.IAttributeValue>());
                value2.Value = value;
                value2.DisplayOrder = displayOrder;
                return value2;
            }

            public Interfaces.Repository1.IAttributeValueChoice NewAttributeValueChoice(Interfaces.Repository1.IAttribute attribute, string value)
            {
                Interfaces.Repository1.IAttributeValueChoice choice = this._domainFactory.CreateDomainObjectInstance<Interfaces.Repository1.IAttributeValueChoice>(this.EntityFactory.NewEntity<Interfaces.Repository1.IAttributeValueChoice>());
                choice.Attribute = attribute;
                choice.Value = value;
                return choice;
            }

            public Interfaces.Repository1.IEmailContactDetail NewEmailContactDetail(string email, bool isPrimary)
            {
                Interfaces.Repository1.IEmailContactDetail detail = this._domainFactory.CreateDomainObjectInstance<Interfaces.Repository1.IEmailContactDetail>(this.EntityFactory.NewEntity<Interfaces.Repository1.IEmailContactDetail>());
                detail.Email = email;
                detail.IsPrimary = isPrimary;
                return detail;
            }

            public Interfaces.Repository1.IOrderLine NewOrderLine(Interfaces.Repository1.IOrder order, Interfaces.Repository1.IProduct product, int quantity)
            {
                Interfaces.Repository1.IOrderLine line = this._domainFactory.CreateDomainObjectInstance<Interfaces.Repository1.IOrderLine>(this.EntityFactory.NewEntity<Interfaces.Repository1.IOrderLine>());
                line.Order = order;
                line.Product = product;
                line.Quantity = quantity;
                return line;
            }

            public Interfaces.Repository1.IPhoneContactDetail NewPhoneContactDetail(string phone, bool isPrimary)
            {
                Interfaces.Repository1.IPhoneContactDetail detail = this._domainFactory.CreateDomainObjectInstance<Interfaces.Repository1.IPhoneContactDetail>(this.EntityFactory.NewEntity<Interfaces.Repository1.IPhoneContactDetail>());
                detail.Phone = phone;
                detail.IsPrimary = isPrimary;
                return detail;
            }

            public Interfaces.Repository1.IPostalAddress NewPostalAddress(string streetAddress, string city, string zipCode, string country)
            {
                Interfaces.Repository1.IPostalAddress address = this._domainFactory.CreateDomainObjectInstance<Interfaces.Repository1.IPostalAddress>(this.EntityFactory.NewEntity<Interfaces.Repository1.IPostalAddress>());
                address.StreetAddress = streetAddress;
                address.City = city;
                address.ZipCode = zipCode;
                address.Country = country;
                return address;
            }

            public Interfaces.Repository1.IPostContactDetail NewPostContactDetail(bool isPrimary)
            {
                Interfaces.Repository1.IPostContactDetail detail = this._domainFactory.CreateDomainObjectInstance<Interfaces.Repository1.IPostContactDetail>(this.EntityFactory.NewEntity<Interfaces.Repository1.IPostContactDetail>());
                detail.IsPrimary = isPrimary;
                return detail;
            }

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

            public IEntityRepository<Interfaces.Repository1.ICustomer> Customers
            {
                get
                {
                    return this.m_Customers;
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
        }

        [BsonIgnoreExtraElements]
        public class MongoEntityObject_Attribute : Interfaces.Repository1.IAttribute, IEntityPartUniqueDisplayName, IEntityPartId<ObjectId>, IObject, IEntityObject, IHaveNestedObjects
        {
            private IDomainObject _domainObject;
            private ObjectId m_Id_storage;
            private string m_Name;
            private string m_TitleForUser;
            private ConcreteToAbstractListAdapter<MongoEntityObject_AttributeValue, Interfaces.Repository1.IAttributeValue> m_Values_adapter;
            private List<MongoEntityObject_AttributeValue> m_Values_storage;

            public MongoEntityObject_Attribute()
            {
            }

            public MongoEntityObject_Attribute(IComponentContext arg0)
            {
                this.m_Values_storage = new List<MongoEntityObject_AttributeValue>();
                this.m_Values_adapter = (ConcreteToAbstractListAdapter<MongoEntityObject_AttributeValue, Interfaces.Repository1.IAttributeValue>)RuntimeTypeModelHelpers.CreateCollectionAdapter<MongoEntityObject_AttributeValue, Interfaces.Repository1.IAttributeValue>(this.m_Values_storage, true);
                this.m_Id_storage = ObjectId.GenerateNewId();
            }

            public static object FactoryMethod1()
            {
                return new MongoEntityObject_Attribute();
            }

            public static object FactoryMethod2(IComponentContext context1)
            {
                return new MongoEntityObject_Attribute(context1);
            }

            public void DeepListNestedObjects(HashSet<object> nestedObjects)
            {
                RuntimeTypeModelHelpers.DeepListNestedObjectCollection(this.m_Values_adapter, nestedObjects);
            }

            IEntityId IEntityObject.GetId()
            {
                return new EntityId<Interfaces.Repository1.IAttribute, ObjectId>(this.m_Id_storage);
            }

            void IEntityObject.SetId(object value)
            {
                this.m_Id_storage = (ObjectId)value;
            }

            void IPersistableObject.SetContainerObject(IDomainObject container)
            {
                this._domainObject = container;
            }

            public void EnsureDomainObject()
            {
                throw new NotImplementedException();
            }

            IDomainObject IContainedIn<IDomainObject>.GetContainerObject()
            {
                return this._domainObject;
            }

            public virtual void WritePropertyValue_Id(ObjectId arg1)
            {
                this.m_Id_storage = arg1;
            }

            IList<Interfaces.Repository1.IAttributeValue> Interfaces.Repository1.IAttribute.Values
            {
                get
                {
                    return this.m_Values_adapter;
                }
            }

            public virtual ObjectId Id
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

            ObjectId IEntityPartId<ObjectId>.Id
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
                get
                {
                    return typeof(MongoEntityObjectFactory);
                }
            }

            bool IObject.IsModified
            {
                get { throw new NotImplementedException(); }
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

            public virtual List<MongoEntityObject_AttributeValue> Values
            {
                get
                {
                    return this.m_Values_storage;
                }
                set
                {
                    this.m_Values_adapter = (ConcreteToAbstractListAdapter<MongoEntityObject_AttributeValue, Interfaces.Repository1.IAttributeValue>)RuntimeTypeModelHelpers.CreateCollectionAdapter<MongoEntityObject_AttributeValue, Interfaces.Repository1.IAttributeValue>(value, true);
                    this.m_Values_storage = value;
                }
            }

            #region Implementation of IEntityObjectBase

            EntityState IEntityObjectBase.State
            {
                get { throw new NotImplementedException(); }
            }

            #endregion
        }

        [BsonIgnoreExtraElements]
        public class MongoEntityObject_AttributeValue : Interfaces.Repository1.IAttributeValue, IObject, IEntityPartObject
        {
            private IDomainObject _domainObject;
            private int m_DisplayOrder;
            private string m_Value;

            public MongoEntityObject_AttributeValue()
            {
            }

            public MongoEntityObject_AttributeValue(IComponentContext arg0)
            {
            }

            public static object FactoryMethod1()
            {
                return new MongoEntityObject_AttributeValue();
            }

            public static object FactoryMethod2(IComponentContext context1)
            {
                return new MongoEntityObject_AttributeValue(context1);
            }

            void IPersistableObject.SetContainerObject(IDomainObject container)
            {
                this._domainObject = container;
            }

            public void EnsureDomainObject()
            {
                throw new NotImplementedException();
            }

            IDomainObject IContainedIn<IDomainObject>.GetContainerObject()
            {
                return this._domainObject;
            }

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
                get
                {
                    return typeof(MongoEntityObjectFactory);
                }
            }

            bool IObject.IsModified
            {
                get { throw new NotImplementedException(); }
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

            #region Implementation of IEntityObjectBase

            EntityState IEntityObjectBase.State
            {
                get { throw new NotImplementedException(); }
            }

            #endregion
        }

        [BsonIgnoreExtraElements]
        public class MongoEntityObject_AttributeValueChoice : Interfaces.Repository1.IAttributeValueChoice, IObject, IEntityPartObject, IHaveDependencies, IHaveNestedObjects
        {
            private IComponentContext _components;
            private IDomainObject _domainObject;
            private Interfaces.Repository1.IAttribute m_Attribute_contract;
            private DualValueStates m_Attribute_state;
            private ObjectId m_Attribute_storage;
            private string m_Value;

            public MongoEntityObject_AttributeValueChoice()
            {
            }

            public MongoEntityObject_AttributeValueChoice(IComponentContext arg0)
            {
                this._components = arg0;
                ((IHaveDependencies)this).InjectDependencies(this._components);
            }

            public static object FactoryMethod1()
            {
                return new MongoEntityObject_AttributeValueChoice();
            }

            public static object FactoryMethod2(IComponentContext context1)
            {
                return new MongoEntityObject_AttributeValueChoice(context1);
            }

            public void DeepListNestedObjects(HashSet<object> nestedObjects)
            {
                if ((((int)this.m_Attribute_state) & 1) != 0)
                {
                    RuntimeTypeModelHelpers.DeepListNestedObject(this.m_Attribute_contract, nestedObjects);
                }
            }

            public virtual void InjectDependencies(IComponentContext components)
            {
                this._components = components;
            }

            void IPersistableObject.SetContainerObject(IDomainObject container)
            {
                this._domainObject = container;
            }

            public void EnsureDomainObject()
            {
                throw new NotImplementedException();
            }

            IDomainObject IContainedIn<IDomainObject>.GetContainerObject()
            {
                return this._domainObject;
            }

            public virtual ObjectId Attribute
            {
                get
                {
                    if (this.m_Attribute_state == DualValueStates.Contract)
                    {
                        this.m_Attribute_storage = ((IEntityObject)this.m_Attribute_contract).GetId().ValueAs<ObjectId>();
                        this.m_Attribute_state |= DualValueStates.Storage;
                    }
                    return this.m_Attribute_storage;
                }
                set
                {
                    this.m_Attribute_storage = value;
                    this.m_Attribute_state = DualValueStates.Storage;
                }
            }

            Interfaces.Repository1.IAttribute Interfaces.Repository1.IAttributeValueChoice.Attribute
            {
                get
                {
                    if (this.m_Attribute_state == DualValueStates.Storage)
                    {
                        this.m_Attribute_contract = MongoDataRepositoryBase.ResolveFrom(this._components).LazyLoadById<Interfaces.Repository1.IAttribute, ObjectId>(this.m_Attribute_storage);
                        this.m_Attribute_state |= DualValueStates.Contract;
                    }
                    return this.m_Attribute_contract;
                }
                set
                {
                    this.m_Attribute_contract = value;
                    this.m_Attribute_state = DualValueStates.Contract;
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
                get
                {
                    return typeof(MongoEntityObjectFactory);
                }
            }

            bool IObject.IsModified
            {
                get { throw new NotImplementedException(); }
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

            #region Implementation of IEntityObjectBase

            EntityState IEntityObjectBase.State
            {
                get { throw new NotImplementedException(); }
            }

            #endregion
        }

        [BsonIgnoreExtraElements]
        public class MongoEntityObject_Category : Interfaces.Repository1.ICategory, IEntityPartUniqueDisplayName, IEntityPartId<ObjectId>, IObject, IEntityObject
        {
            private IDomainObject _domainObject;
            private ObjectId m_Id_storage;
            private string m_Name;

            public MongoEntityObject_Category()
            {
            }

            public MongoEntityObject_Category(IComponentContext arg0)
            {
                this.m_Id_storage = ObjectId.GenerateNewId();
            }

            public static object FactoryMethod1()
            {
                return new MongoEntityObject_Category();
            }

            public static object FactoryMethod2(IComponentContext context1)
            {
                return new MongoEntityObject_Category(context1);
            }

            IEntityId IEntityObject.GetId()
            {
                return new EntityId<Interfaces.Repository1.ICategory, ObjectId>(this.m_Id_storage);
            }

            void IEntityObject.SetId(object value)
            {
                this.m_Id_storage = (ObjectId)value;
            }

            void IPersistableObject.SetContainerObject(IDomainObject container)
            {
                this._domainObject = container;
            }

            public void EnsureDomainObject()
            {
                throw new NotImplementedException();
            }

            IDomainObject IContainedIn<IDomainObject>.GetContainerObject()
            {
                return this._domainObject;
            }

            public virtual void WritePropertyValue_Id(ObjectId arg1)
            {
                this.m_Id_storage = arg1;
            }

            public virtual ObjectId Id
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

            ObjectId IEntityPartId<ObjectId>.Id
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
                get
                {
                    return typeof(MongoEntityObjectFactory);
                }
            }

            bool IObject.IsModified
            {
                get { throw new NotImplementedException(); }
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

            #region Implementation of IEntityObjectBase

            EntityState IEntityObjectBase.State
            {
                get { throw new NotImplementedException(); }
            }

            #endregion
        }

        [BsonIgnoreExtraElements, BsonDiscriminator("ContactDetail", Required = true)]
        public class MongoEntityObject_ContactDetail : Interfaces.Repository1.IContactDetail, IEntityPartId<ObjectId>, IObject, IEntityObject
        {
            private IDomainObject _domainObject;
            private ObjectId m_Id_storage;
            private bool m_IsPrimary;

            public MongoEntityObject_ContactDetail()
            {
            }

            public MongoEntityObject_ContactDetail(IComponentContext arg0)
            {
                this.m_Id_storage = ObjectId.GenerateNewId();
            }

            public static object FactoryMethod1()
            {
                return new MongoEntityObject_ContactDetail();
            }

            public static object FactoryMethod2(IComponentContext context1)
            {
                return new MongoEntityObject_ContactDetail(context1);
            }

            IEntityId IEntityObject.GetId()
            {
                return new EntityId<Interfaces.Repository1.IContactDetail, ObjectId>(this.m_Id_storage);
            }

            void IEntityObject.SetId(object value)
            {
                this.m_Id_storage = (ObjectId)value;
            }

            void IPersistableObject.SetContainerObject(IDomainObject container)
            {
                this._domainObject = container;
            }

            public void EnsureDomainObject()
            {
                throw new NotImplementedException();
            }

            IDomainObject IContainedIn<IDomainObject>.GetContainerObject()
            {
                return this._domainObject;
            }

            public virtual void WritePropertyValue_Id(ObjectId arg1)
            {
                this.m_Id_storage = arg1;
            }

            public virtual ObjectId Id
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

            ObjectId IEntityPartId<ObjectId>.Id
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
                get
                {
                    return typeof(MongoEntityObjectFactory);
                }
            }

            bool IObject.IsModified
            {
                get { throw new NotImplementedException(); }
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

            #region Implementation of IEntityObjectBase

            EntityState IEntityObjectBase.State
            {
                get { throw new NotImplementedException(); }
            }

            #endregion
        }

        [BsonIgnoreExtraElements]
        public class MongoEntityObject_Customer : Interfaces.Repository1.ICustomer, IEntityPartId<ObjectId>, IObject, IEntityObject, IHaveDependencies, IHaveNestedObjects
        {
            private IComponentContext _components;
            private IDomainObject _domainObject;
            private ICollection<Interfaces.Repository1.IContactDetail> m_ContactDetails_contract;
            private DualValueStates m_ContactDetails_state;
            private ObjectId[] m_ContactDetails_storage;
            private string m_FullName;
            private ObjectId m_Id_storage;

            public MongoEntityObject_Customer()
            {
            }

            public MongoEntityObject_Customer(IComponentContext arg0)
            {
                this._components = arg0;
                this.m_ContactDetails_contract = new HashSet<Interfaces.Repository1.IContactDetail>();
                this.m_ContactDetails_state = DualValueStates.Contract;
                ((IHaveDependencies)this).InjectDependencies(this._components);
                this.m_Id_storage = ObjectId.GenerateNewId();
            }

            public static object FactoryMethod1()
            {
                return new MongoEntityObject_Customer();
            }

            public static object FactoryMethod2(IComponentContext context1)
            {
                return new MongoEntityObject_Customer(context1);
            }

            public void DeepListNestedObjects(HashSet<object> nestedObjects)
            {
                if ((((int)this.m_ContactDetails_state) & 1) != 0)
                {
                    RuntimeTypeModelHelpers.DeepListNestedObjectCollection(this.m_ContactDetails_contract, nestedObjects);
                }
            }

            public void Delete()
            {
                throw new NotSupportedException("Methods of entity contracts are only supported by domain objects.");
            }

            public virtual void InjectDependencies(IComponentContext components)
            {
                this._components = components;
            }

            public bool IsInteredtedIn(Interfaces.Repository1.IProduct product)
            {
                throw new NotSupportedException();
            }

            IEntityId IEntityObject.GetId()
            {
                return new EntityId<Interfaces.Repository1.ICustomer, ObjectId>(this.m_Id_storage);
            }

            void IEntityObject.SetId(object value)
            {
                this.m_Id_storage = (ObjectId)value;
            }

            void IPersistableObject.SetContainerObject(IDomainObject container)
            {
                this._domainObject = container;
            }

            public void EnsureDomainObject()
            {
                throw new NotImplementedException();
            }

            IDomainObject IContainedIn<IDomainObject>.GetContainerObject()
            {
                return this._domainObject;
            }

            public bool QualifiesAsValuableCustomer()
            {
                throw new NotSupportedException();
            }

            public void Save()
            {
                throw new NotSupportedException("Methods of entity contracts are only supported by domain objects.");
            }

            public virtual void WritePropertyValue_Id(ObjectId arg1)
            {
                this.m_Id_storage = arg1;
            }

            public IR1.CustomerPriority? Priority
            {
                get { throw new NotImplementedException(); }
                set { throw new NotImplementedException(); }
            }

            public virtual ObjectId[] ContactDetails
            {
                get
                {
                    if (this.m_ContactDetails_state == DualValueStates.Contract)
                    {
                        int count = this.m_ContactDetails_contract.Count;
                        int num2 = 0;
                        ObjectId[] idArray = new ObjectId[count];
                        this.m_ContactDetails_contract.Cast<IEntityObject>();
                        IEnumerator<IEntityObject> enumerator = this.m_ContactDetails_contract.Cast<IEntityObject>().GetEnumerator();
                        using (enumerator)
                        {
                            while (enumerator.MoveNext())
                            {
                                IEntityObject current = enumerator.Current;
                                idArray[num2] = current.GetId().ValueAs<ObjectId>();
                                num2++;
                            }
                        }
                        this.m_ContactDetails_storage = idArray;
                        this.m_ContactDetails_state |= DualValueStates.Storage;
                    }
                    return this.m_ContactDetails_storage;
                }
                set
                {
                    this.m_ContactDetails_storage = value;
                    this.m_ContactDetails_state = DualValueStates.Storage;
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

            public int? Age
            {
                get { throw new NotImplementedException(); }
                set { throw new NotImplementedException(); }
            }

            ICollection<Interfaces.Repository1.IContactDetail> Interfaces.Repository1.ICustomer.ContactDetails
            {
                get
                {
                    if (this.m_ContactDetails_state == DualValueStates.Storage)
                    {
                        IEnumerable<Interfaces.Repository1.IContactDetail> collection = MongoDataRepositoryBase.ResolveFrom(this._components).LazyLoadByIdList<Interfaces.Repository1.IContactDetail, ObjectId>(this.m_ContactDetails_storage);
                        this.m_ContactDetails_contract = new HashSet<Interfaces.Repository1.IContactDetail>(collection);
                        this.m_ContactDetails_state |= DualValueStates.Contract;
                    }
                    return this.m_ContactDetails_contract;
                }
            }

            public virtual ObjectId Id
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

            ObjectId IEntityPartId<ObjectId>.Id
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
                get
                {
                    return typeof(MongoEntityObjectFactory);
                }
            }

            bool IObject.IsModified
            {
                get { throw new NotImplementedException(); }
            }

            #region Implementation of IEntityObjectBase

            EntityState IEntityObjectBase.State
            {
                get { throw new NotImplementedException(); }
            }

            #endregion
        }

        [BsonDiscriminator("EmailContactDetail", Required = true), BsonIgnoreExtraElements]
        public class MongoEntityObject_EmailContactDetail : MongoEntityObject_ContactDetail, Interfaces.Repository1.IEmailContactDetail, IObject
        {
            private string m_Email;

            public MongoEntityObject_EmailContactDetail()
            {
            }

            public MongoEntityObject_EmailContactDetail(IComponentContext arg0)
                : base(arg0)
            {
            }

            public static new object FactoryMethod1()
            {
                return new MongoEntityObject_EmailContactDetail();
            }

            public static new object FactoryMethod2(IComponentContext context1)
            {
                return new MongoEntityObject_EmailContactDetail(context1);
            }

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

            Type IObject.ContractType
            {
                get
                {
                    return typeof(Interfaces.Repository1.IEmailContactDetail);
                }
            }

            Type IObject.FactoryType
            {
                get
                {
                    return typeof(MongoEntityObjectFactory);
                }
            }
        }

        [BsonIgnoreExtraElements]
        public class MongoEntityObject_Order : Interfaces.Repository1.IOrder, IEntityPartId<ObjectId>, IObject, IEntityObject, IHaveDependencies, IHaveNestedObjects
        {
            private IComponentContext _components;
            private IDomainObject _domainObject;
            private MongoEntityObject_PostalAddress m_BillingAddress_storage;
            private Interfaces.Repository1.ICustomer m_Customer_contract;
            private DualValueStates m_Customer_state;
            private ObjectId m_Customer_storage;
            private MongoEntityObject_PostalAddress m_DeliveryAddress_storage;
            private ObjectId m_Id_storage;
            private ICollection<Interfaces.Repository1.IOrderLine> m_OrderLines_contract;
            private DualValueStates m_OrderLines_state;
            private ObjectId[] m_OrderLines_storage;
            private string m_OrderNo;
            private DateTime m_PlacedAt;
            private Interfaces.Repository1.OrderStatus m_Status;

            public MongoEntityObject_Order()
            {
            }

            public MongoEntityObject_Order(IComponentContext arg0)
            {
                this._components = arg0;
                this.m_OrderLines_contract = new HashSet<Interfaces.Repository1.IOrderLine>();
                this.m_OrderLines_state = DualValueStates.Contract;
                this.m_DeliveryAddress_storage = new MongoEntityObject_PostalAddress(arg0);
                this.m_BillingAddress_storage = new MongoEntityObject_PostalAddress(arg0);
                this.m_Status = Interfaces.Repository1.OrderStatus.New;
                ((IHaveDependencies)this).InjectDependencies(this._components);
                this.m_Id_storage = ObjectId.GenerateNewId();
            }

            public static object FactoryMethod1()
            {
                return new MongoEntityObject_Order();
            }

            public static object FactoryMethod2(IComponentContext context1)
            {
                return new MongoEntityObject_Order(context1);
            }

            public void DeepListNestedObjects(HashSet<object> nestedObjects)
            {
                if ((((int)this.m_OrderLines_state) & 1) != 0)
                {
                    RuntimeTypeModelHelpers.DeepListNestedObjectCollection(this.m_OrderLines_contract, nestedObjects);
                }
                RuntimeTypeModelHelpers.DeepListNestedObject(this.m_DeliveryAddress_storage, nestedObjects);
                RuntimeTypeModelHelpers.DeepListNestedObject(this.m_BillingAddress_storage, nestedObjects);
                if ((((int)this.m_Customer_state) & 1) != 0)
                {
                    RuntimeTypeModelHelpers.DeepListNestedObject(this.m_Customer_contract, nestedObjects);
                }
            }

            public virtual void InjectDependencies(IComponentContext components)
            {
                this._components = components;
            }

            IEntityId IEntityObject.GetId()
            {
                return new EntityId<Interfaces.Repository1.IOrder, ObjectId>(this.m_Id_storage);
            }

            void IEntityObject.SetId(object value)
            {
                this.m_Id_storage = (ObjectId)value;
            }

            void IPersistableObject.SetContainerObject(IDomainObject container)
            {
                this._domainObject = container;
            }

            public void EnsureDomainObject()
            {
                throw new NotImplementedException();
            }

            IDomainObject IContainedIn<IDomainObject>.GetContainerObject()
            {
                return this._domainObject;
            }

            public virtual void WritePropertyValue_BillingAddress(Interfaces.Repository1.IPostalAddress arg1)
            {
                this.m_BillingAddress_storage = (MongoEntityObject_PostalAddress)arg1;
            }

            public virtual void WritePropertyValue_DeliveryAddress(Interfaces.Repository1.IPostalAddress arg1)
            {
                this.m_DeliveryAddress_storage = (MongoEntityObject_PostalAddress)arg1;
            }

            public virtual void WritePropertyValue_Id(ObjectId arg1)
            {
                this.m_Id_storage = arg1;
            }

            public virtual MongoEntityObject_PostalAddress BillingAddress
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

            public virtual ObjectId Customer
            {
                get
                {
                    if (this.m_Customer_state == DualValueStates.Contract)
                    {
                        this.m_Customer_storage = ((IEntityObject)this.m_Customer_contract).GetId().ValueAs<ObjectId>();
                        this.m_Customer_state |= DualValueStates.Storage;
                    }
                    return this.m_Customer_storage;
                }
                set
                {
                    this.m_Customer_storage = value;
                    this.m_Customer_state = DualValueStates.Storage;
                }
            }

            public virtual MongoEntityObject_PostalAddress DeliveryAddress
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

            public virtual ObjectId Id
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

            ObjectId IEntityPartId<ObjectId>.Id
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

            Type IObject.FactoryType
            {
                get
                {
                    return typeof(MongoEntityObjectFactory);
                }
            }

            bool IObject.IsModified
            {
                get { throw new NotImplementedException(); }
            }

            Interfaces.Repository1.IPostalAddress Interfaces.Repository1.IOrder.BillingAddress
            {
                get
                {
                    return this.m_BillingAddress_storage;
                }
            }

            Interfaces.Repository1.ICustomer Interfaces.Repository1.IOrder.Customer
            {
                get
                {
                    if (this.m_Customer_state == DualValueStates.Storage)
                    {
                        this.m_Customer_contract = MongoDataRepositoryBase.ResolveFrom(this._components).LazyLoadById<Interfaces.Repository1.ICustomer, ObjectId>(this.m_Customer_storage);
                        this.m_Customer_state |= DualValueStates.Contract;
                    }
                    return this.m_Customer_contract;
                }
                set
                {
                    this.m_Customer_contract = value;
                    this.m_Customer_state = DualValueStates.Contract;
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
                    if (this.m_OrderLines_state == DualValueStates.Storage)
                    {
                        IEnumerable<Interfaces.Repository1.IOrderLine> collection = MongoDataRepositoryBase.ResolveFrom(this._components).LazyLoadByIdList<Interfaces.Repository1.IOrderLine, ObjectId>(this.m_OrderLines_storage);
                        this.m_OrderLines_contract = new HashSet<Interfaces.Repository1.IOrderLine>(collection);
                        this.m_OrderLines_state |= DualValueStates.Contract;
                    }
                    return this.m_OrderLines_contract;
                }
            }

            public virtual ObjectId[] OrderLines
            {
                get
                {
                    if (this.m_OrderLines_state == DualValueStates.Contract)
                    {
                        int count = this.m_OrderLines_contract.Count;
                        int num2 = 0;
                        ObjectId[] idArray = new ObjectId[count];
                        this.m_OrderLines_contract.Cast<IEntityObject>();
                        IEnumerator<IEntityObject> enumerator = this.m_OrderLines_contract.Cast<IEntityObject>().GetEnumerator();
                        using (enumerator)
                        {
                            while (enumerator.MoveNext())
                            {
                                IEntityObject current = enumerator.Current;
                                idArray[num2] = current.GetId().ValueAs<ObjectId>();
                                num2++;
                            }
                        }
                        this.m_OrderLines_storage = idArray;
                        this.m_OrderLines_state |= DualValueStates.Storage;
                    }
                    return this.m_OrderLines_storage;
                }
                set
                {
                    this.m_OrderLines_storage = value;
                    this.m_OrderLines_state = DualValueStates.Storage;
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

            #region Implementation of IEntityObjectBase

            EntityState IEntityObjectBase.State
            {
                get { throw new NotImplementedException(); }
            }

            #endregion
        }

        [BsonIgnoreExtraElements]
        public class MongoEntityObject_OrderLine : Interfaces.Repository1.IOrderLine, IEntityPartId<ObjectId>, IObject, IEntityObject, IHaveDependencies, IHaveNestedObjects
        {
            private IComponentContext _components;
            private IDomainObject _domainObject;
            private ConcreteToAbstractCollectionAdapter<MongoEntityObject_AttributeValueChoice, Interfaces.Repository1.IAttributeValueChoice> m_Attributes_adapter;
            private HashSet<MongoEntityObject_AttributeValueChoice> m_Attributes_storage;
            private ObjectId m_Id_storage;
            private Interfaces.Repository1.IOrder m_Order_contract;
            private DualValueStates m_Order_state;
            private ObjectId m_Order_storage;
            private Interfaces.Repository1.IProduct m_Product_contract;
            private DualValueStates m_Product_state;
            private ObjectId m_Product_storage;
            private int m_Quantity;

            public MongoEntityObject_OrderLine()
            {
            }

            public MongoEntityObject_OrderLine(IComponentContext arg0)
            {
                this._components = arg0;
                this.m_Attributes_storage = new HashSet<MongoEntityObject_AttributeValueChoice>();
                this.m_Attributes_adapter = (ConcreteToAbstractCollectionAdapter<MongoEntityObject_AttributeValueChoice, Interfaces.Repository1.IAttributeValueChoice>)RuntimeTypeModelHelpers.CreateCollectionAdapter<MongoEntityObject_AttributeValueChoice, Interfaces.Repository1.IAttributeValueChoice>(this.m_Attributes_storage, false);
                ((IHaveDependencies)this).InjectDependencies(this._components);
                this.m_Id_storage = ObjectId.GenerateNewId();
            }

            public static object FactoryMethod1()
            {
                return new MongoEntityObject_OrderLine();
            }

            public static object FactoryMethod2(IComponentContext context1)
            {
                return new MongoEntityObject_OrderLine(context1);
            }

            public void DeepListNestedObjects(HashSet<object> nestedObjects)
            {
                if ((((int)this.m_Order_state) & 1) != 0)
                {
                    RuntimeTypeModelHelpers.DeepListNestedObject(this.m_Order_contract, nestedObjects);
                }
                if ((((int)this.m_Product_state) & 1) != 0)
                {
                    RuntimeTypeModelHelpers.DeepListNestedObject(this.m_Product_contract, nestedObjects);
                }
                RuntimeTypeModelHelpers.DeepListNestedObjectCollection(this.m_Attributes_adapter, nestedObjects);
            }

            public virtual void InjectDependencies(IComponentContext components)
            {
                this._components = components;
            }

            IEntityId IEntityObject.GetId()
            {
                return new EntityId<Interfaces.Repository1.IOrderLine, ObjectId>(this.m_Id_storage);
            }

            void IEntityObject.SetId(object value)
            {
                this.m_Id_storage = (ObjectId)value;
            }

            void IPersistableObject.SetContainerObject(IDomainObject container)
            {
                this._domainObject = container;
            }

            public void EnsureDomainObject()
            {
                throw new NotImplementedException();
            }

            IDomainObject IContainedIn<IDomainObject>.GetContainerObject()
            {
                return this._domainObject;
            }

            public virtual void WritePropertyValue_Id(ObjectId arg1)
            {
                this.m_Id_storage = arg1;
            }

            public virtual HashSet<MongoEntityObject_AttributeValueChoice> Attributes
            {
                get
                {
                    return this.m_Attributes_storage;
                }
                set
                {
                    this.m_Attributes_adapter = (ConcreteToAbstractCollectionAdapter<MongoEntityObject_AttributeValueChoice, Interfaces.Repository1.IAttributeValueChoice>)RuntimeTypeModelHelpers.CreateCollectionAdapter<MongoEntityObject_AttributeValueChoice, Interfaces.Repository1.IAttributeValueChoice>(value, false);
                    this.m_Attributes_storage = value;
                }
            }

            public virtual ObjectId Id
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

            ObjectId IEntityPartId<ObjectId>.Id
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
                get
                {
                    return typeof(MongoEntityObjectFactory);
                }
            }

            bool IObject.IsModified
            {
                get { throw new NotImplementedException(); }
            }

            ICollection<Interfaces.Repository1.IAttributeValueChoice> Interfaces.Repository1.IOrderLine.Attributes
            {
                get
                {
                    return this.m_Attributes_adapter;
                }
            }

            Interfaces.Repository1.IOrder Interfaces.Repository1.IOrderLine.Order
            {
                get
                {
                    if (this.m_Order_state == DualValueStates.Storage)
                    {
                        this.m_Order_contract = MongoDataRepositoryBase.ResolveFrom(this._components).LazyLoadById<Interfaces.Repository1.IOrder, ObjectId>(this.m_Order_storage);
                        this.m_Order_state |= DualValueStates.Contract;
                    }
                    return this.m_Order_contract;
                }
                set
                {
                    this.m_Order_contract = value;
                    this.m_Order_state = DualValueStates.Contract;
                }
            }

            Interfaces.Repository1.IProduct Interfaces.Repository1.IOrderLine.Product
            {
                get
                {
                    if (this.m_Product_state == DualValueStates.Storage)
                    {
                        this.m_Product_contract = MongoDataRepositoryBase.ResolveFrom(this._components).LazyLoadById<Interfaces.Repository1.IProduct, ObjectId>(this.m_Product_storage);
                        this.m_Product_state |= DualValueStates.Contract;
                    }
                    return this.m_Product_contract;
                }
                set
                {
                    this.m_Product_contract = value;
                    this.m_Product_state = DualValueStates.Contract;
                }
            }

            public virtual ObjectId Order
            {
                get
                {
                    if (this.m_Order_state == DualValueStates.Contract)
                    {
                        this.m_Order_storage = ((IEntityObject)this.m_Order_contract).GetId().ValueAs<ObjectId>();
                        this.m_Order_state |= DualValueStates.Storage;
                    }
                    return this.m_Order_storage;
                }
                set
                {
                    this.m_Order_storage = value;
                    this.m_Order_state = DualValueStates.Storage;
                }
            }

            public virtual ObjectId Product
            {
                get
                {
                    if (this.m_Product_state == DualValueStates.Contract)
                    {
                        this.m_Product_storage = ((IEntityObject)this.m_Product_contract).GetId().ValueAs<ObjectId>();
                        this.m_Product_state |= DualValueStates.Storage;
                    }
                    return this.m_Product_storage;
                }
                set
                {
                    this.m_Product_storage = value;
                    this.m_Product_state = DualValueStates.Storage;
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

            #region Implementation of IEntityObjectBase

            EntityState IEntityObjectBase.State
            {
                get { throw new NotImplementedException(); }
            }

            #endregion
        }

        [BsonDiscriminator("PhoneContactDetail", Required = true), BsonIgnoreExtraElements]
        public class MongoEntityObject_PhoneContactDetail : MongoEntityObject_ContactDetail, Interfaces.Repository1.IPhoneContactDetail, IObject
        {
            private string m_Phone;

            public MongoEntityObject_PhoneContactDetail()
            {
            }

            public MongoEntityObject_PhoneContactDetail(IComponentContext arg0)
                : base(arg0)
            {
            }

            public new static object FactoryMethod1()
            {
                return new MongoEntityObject_PhoneContactDetail();
            }

            public new static object FactoryMethod2(IComponentContext context1)
            {
                return new MongoEntityObject_PhoneContactDetail(context1);
            }

            Type IObject.ContractType
            {
                get
                {
                    return typeof(Interfaces.Repository1.IPhoneContactDetail);
                }
            }

            Type IObject.FactoryType
            {
                get
                {
                    return typeof(MongoEntityObjectFactory);
                }
            }

            public virtual string Phone
            {
                get
                {
                    return this.m_Phone;
                }
                set
                {
                    this.m_Phone = value;
                }
            }
        }


        [BsonIgnoreExtraElements]
        public class MongoEntityObject_PostalAddress : Interfaces.Repository1.IPostalAddress, IObject, IEntityPartObject
        {
            private IDomainObject _domainObject;
            private string m_City;
            private string m_Country;
            private string m_StreetAddress;
            private string m_ZipCode;

            public MongoEntityObject_PostalAddress()
            {
            }

            public MongoEntityObject_PostalAddress(IComponentContext arg0)
            {
            }

            public static object FactoryMethod1()
            {
                return new MongoEntityObject_PostalAddress();
            }

            public static object FactoryMethod2(IComponentContext context1)
            {
                return new MongoEntityObject_PostalAddress(context1);
            }

            void IPersistableObject.SetContainerObject(IDomainObject container)
            {
                this._domainObject = container;
            }

            public void EnsureDomainObject()
            {
                throw new NotImplementedException();
            }

            IDomainObject IContainedIn<IDomainObject>.GetContainerObject()
            {
                return this._domainObject;
            }

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
                get
                {
                    return typeof(MongoEntityObjectFactory);
                }
            }

            bool IObject.IsModified
            {
                get { throw new NotImplementedException(); }
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

            #region Implementation of IEntityObjectBase

            EntityState IEntityObjectBase.State
            {
                get { throw new NotImplementedException(); }
            }

            #endregion
        }

        [BsonDiscriminator("PostContactDetail", Required = true), BsonIgnoreExtraElements]
        public class MongoEntityObject_PostContactDetail : MongoEntityObject_ContactDetail, Interfaces.Repository1.IPostContactDetail, IObject, IHaveNestedObjects
        {
            private MongoEntityObject_PostalAddress m_PostalAddress_storage;

            public MongoEntityObject_PostContactDetail()
            {
            }

            public MongoEntityObject_PostContactDetail(IComponentContext arg0)
                : base(arg0)
            {
                this.m_PostalAddress_storage = new MongoEntityObject_PostalAddress(arg0);
            }

            public new static object FactoryMethod1()
            {
                return new MongoEntityObject_PostContactDetail();
            }

            public new static object FactoryMethod2(IComponentContext context1)
            {
                return new MongoEntityObject_PostContactDetail(context1);
            }

            public void DeepListNestedObjects(HashSet<object> nestedObjects)
            {
                RuntimeTypeModelHelpers.DeepListNestedObject(this.m_PostalAddress_storage, nestedObjects);
            }

            public virtual void WritePropertyValue_PostalAddress(Interfaces.Repository1.IPostalAddress arg1)
            {
                this.m_PostalAddress_storage = (MongoEntityObject_PostalAddress)arg1;
            }

            Type IObject.ContractType
            {
                get
                {
                    return typeof(Interfaces.Repository1.IPostContactDetail);
                }
            }

            Type IObject.FactoryType
            {
                get
                {
                    return typeof(MongoEntityObjectFactory);
                }
            }

            Interfaces.Repository1.IPostalAddress Interfaces.Repository1.IPostContactDetail.PostalAddress
            {
                get
                {
                    return this.m_PostalAddress_storage;
                }
            }

            public virtual MongoEntityObject_PostalAddress PostalAddress
            {
                get
                {
                    return this.m_PostalAddress_storage;
                }
                set
                {
                    this.m_PostalAddress_storage = value;
                }
            }
        }

        [BsonIgnoreExtraElements]
        public class MongoEntityObject_Product : Interfaces.Repository1.IProduct, IEntityPartId<ObjectId>, IObject, IEntityObject, IHaveDependencies, IHaveNestedObjects
        {
            private IComponentContext _components;
            private IDomainObject _domainObject;
            private ICollection<Interfaces.Repository1.IAttribute> m_Attributes_contract;
            private DualValueStates m_Attributes_state;
            private ObjectId[] m_Attributes_storage;
            private string m_CatalogNo;
            private ICollection<Interfaces.Repository1.ICategory> m_Categories_contract;
            private DualValueStates m_Categories_state;
            private ObjectId[] m_Categories_storage;
            private ObjectId m_Id_storage;
            private string m_Name;
            private decimal m_Price;

            public MongoEntityObject_Product()
            {
            }

            public MongoEntityObject_Product(IComponentContext arg0)
            {
                this._components = arg0;
                this.m_Categories_contract = new HashSet<Interfaces.Repository1.ICategory>();
                this.m_Categories_state = DualValueStates.Contract;
                this.m_Attributes_contract = new HashSet<Interfaces.Repository1.IAttribute>();
                this.m_Attributes_state = DualValueStates.Contract;
                ((IHaveDependencies)this).InjectDependencies(this._components);
                this.m_Id_storage = ObjectId.GenerateNewId();
            }

            public static object FactoryMethod1()
            {
                return new MongoEntityObject_Product();
            }

            public static object FactoryMethod2(IComponentContext context1)
            {
                return new MongoEntityObject_Product(context1);
            }

            public void DeepListNestedObjects(HashSet<object> nestedObjects)
            {
                if ((((int)this.m_Categories_state) & 1) != 0)
                {
                    RuntimeTypeModelHelpers.DeepListNestedObjectCollection(this.m_Categories_contract, nestedObjects);
                }
                if ((((int)this.m_Attributes_state) & 1) != 0)
                {
                    RuntimeTypeModelHelpers.DeepListNestedObjectCollection(this.m_Attributes_contract, nestedObjects);
                }
            }

            public virtual void InjectDependencies(IComponentContext components)
            {
                this._components = components;
            }

            IEntityId IEntityObject.GetId()
            {
                return new EntityId<Interfaces.Repository1.IProduct, ObjectId>(this.m_Id_storage);
            }

            void IEntityObject.SetId(object value)
            {
                this.m_Id_storage = (ObjectId)value;
            }

            void IPersistableObject.SetContainerObject(IDomainObject container)
            {
                this._domainObject = container;
            }

            public void EnsureDomainObject()
            {
                throw new NotImplementedException();
            }

            IDomainObject IContainedIn<IDomainObject>.GetContainerObject()
            {
                return this._domainObject;
            }

            public virtual void WritePropertyValue_Id(ObjectId arg1)
            {
                this.m_Id_storage = arg1;
            }

            public virtual ObjectId[] Attributes
            {
                get
                {
                    if (this.m_Attributes_state == DualValueStates.Contract)
                    {
                        int count = this.m_Attributes_contract.Count;
                        int num2 = 0;
                        ObjectId[] idArray = new ObjectId[count];
                        this.m_Attributes_contract.Cast<IEntityObject>();
                        IEnumerator<IEntityObject> enumerator = this.m_Attributes_contract.Cast<IEntityObject>().GetEnumerator();
                        using (enumerator)
                        {
                            while (enumerator.MoveNext())
                            {
                                IEntityObject current = enumerator.Current;
                                idArray[num2] = current.GetId().ValueAs<ObjectId>();
                                num2++;
                            }
                        }
                        this.m_Attributes_storage = idArray;
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

            public virtual ObjectId[] Categories
            {
                get
                {
                    if (this.m_Categories_state == DualValueStates.Contract)
                    {
                        int count = this.m_Categories_contract.Count;
                        int num2 = 0;
                        ObjectId[] idArray = new ObjectId[count];
                        this.m_Categories_contract.Cast<IEntityObject>();
                        IEnumerator<IEntityObject> enumerator = this.m_Categories_contract.Cast<IEntityObject>().GetEnumerator();
                        using (enumerator)
                        {
                            while (enumerator.MoveNext())
                            {
                                IEntityObject current = enumerator.Current;
                                idArray[num2] = current.GetId().ValueAs<ObjectId>();
                                num2++;
                            }
                        }
                        this.m_Categories_storage = idArray;
                        this.m_Categories_state |= DualValueStates.Storage;
                    }
                    return this.m_Categories_storage;
                }
                set
                {
                    this.m_Categories_storage = value;
                    this.m_Categories_state = DualValueStates.Storage;
                }
            }

            public virtual ObjectId Id
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

            ObjectId IEntityPartId<ObjectId>.Id
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
                get
                {
                    return typeof(MongoEntityObjectFactory);
                }
            }

            bool IObject.IsModified
            {
                get { throw new NotImplementedException(); }
            }

            ICollection<Interfaces.Repository1.IAttribute> Interfaces.Repository1.IProduct.Attributes
            {
                get
                {
                    if (this.m_Attributes_state == DualValueStates.Storage)
                    {
                        IEnumerable<Interfaces.Repository1.IAttribute> collection = MongoDataRepositoryBase.ResolveFrom(this._components).LazyLoadByIdList<Interfaces.Repository1.IAttribute, ObjectId>(this.m_Attributes_storage);
                        this.m_Attributes_contract = new HashSet<Interfaces.Repository1.IAttribute>(collection);
                        this.m_Attributes_state |= DualValueStates.Contract;
                    }
                    return this.m_Attributes_contract;
                }
            }

            ICollection<Interfaces.Repository1.ICategory> Interfaces.Repository1.IProduct.Categories
            {
                get
                {
                    if (this.m_Categories_state == DualValueStates.Storage)
                    {
                        IEnumerable<Interfaces.Repository1.ICategory> collection = MongoDataRepositoryBase.ResolveFrom(this._components).LazyLoadByIdList<Interfaces.Repository1.ICategory, ObjectId>(this.m_Categories_storage);
                        this.m_Categories_contract = new HashSet<Interfaces.Repository1.ICategory>(collection);
                        this.m_Categories_state |= DualValueStates.Contract;
                    }
                    return this.m_Categories_contract;
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

            #region Implementation of IEntityObjectBase

            EntityState IEntityObjectBase.State
            {
                get { throw new NotImplementedException(); }
            }

            #endregion
        }


        #endregion
    }
}
