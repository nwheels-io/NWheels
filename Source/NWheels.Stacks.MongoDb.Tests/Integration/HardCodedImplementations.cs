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
using NWheels.Conventions.Core;
using NWheels.DataObjects;
using NWheels.DataObjects.Core;
using NWheels.DataObjects.Core.StorageTypes;
using NWheels.Entities;
using NWheels.Entities.Core;
using NWheels.Extensions;
using NWheels.Stacks.MongoDb.Factories;
using NWheels.Testing.Entities.Stacks;
using NWheels.Utilities;
using IR1 = NWheels.Testing.Entities.Stacks.Interfaces.Repository1;

namespace NWheels.Stacks.MongoDb.Tests.Integration
{
    public static class HardCodedImplementations
    {
        internal static Func<IR1.IOnlineStoreRepository> CurrentDataRepoFactory { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Data Repository IOnlineStoreRepository

        public class DataRepositoryObject_OnlineStoreRepository : MongoDataRepositoryBase, IR1.IOnlineStoreRepository
        {
            private IEntityObjectFactory _entityFactory;

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            #region Constructor

            public DataRepositoryObject_OnlineStoreRepository(
                IComponentContext components, 
                IEntityObjectFactory objectFactory, 
                ITypeMetadataCache metadataCache, 
                MongoDatabase database, 
                bool autoCommit)
                : base(
                    components,
                    objectFactory, 
                    GetOrBuildDbCompiledModel(metadataCache, database), 
                    database, 
                    autoCommit)
            {
                _entityFactory = objectFactory;

                m_Products = new MongoEntityRepository<IR1.IProduct,EntityObject_Product>(this, metadataCache, objectFactory);
                base.RegisterEntityRepository<IR1.IProduct, EntityObject_Product>(m_Products);

                m_Categories = new MongoEntityRepository<IR1.ICategory, EntityObject_Category>(this, metadataCache, objectFactory);
                base.RegisterEntityRepository<IR1.ICategory, EntityObject_Category>(m_Categories);

                m_Attributes = new MongoEntityRepository<IR1.IAttribute, EntityObject_Attribute>(this, metadataCache, objectFactory);
                base.RegisterEntityRepository<IR1.IAttribute, EntityObject_Attribute>(m_Attributes);

                m_Orders = new MongoEntityRepository<IR1.IOrder, EntityObject_Order>(this, metadataCache, objectFactory);
                base.RegisterEntityRepository<IR1.IOrder, EntityObject_Order>(m_Orders);

                m_OrdersLines = new MongoEntityRepository<IR1.IOrderLine, EntityObject_OrderLine>(this, metadataCache, objectFactory);
                base.RegisterEntityRepository<IR1.IOrderLine, EntityObject_OrderLine>(m_OrdersLines);

                m_Customers = null;//TODO: add EntityObject_Customer
                //m_Customers = new MongoEntityRepository<IR1.IOrderLine, EntityObject_Cus>(this, metadataCache, objectFactory);
                //base.RegisterEntityRepository<IR1.IOrderLine, EntityObject_OrderLine>(m_OrdersLines);
            }

            #endregion

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            #region Base Methods

            public override Type[] GetEntityContractsInRepository()
            {
                return new Type[] {
                    typeof(IR1.IProduct),
                    typeof(IR1.ICategory),
                    typeof(IR1.IAttribute),
                    typeof(IR1.IAttributeValue),
                    typeof(IR1.IOrder),
                    typeof(IR1.IOrderLine),
                    typeof(IR1.IAttributeValueChoice),
                };
            }

            public override Type[] GetEntityTypesInRepository()
            {
                return new Type[] {
                    typeof(EntityObject_Product),
                    typeof(EntityObject_Category),
                    typeof(EntityObject_Attribute),
                    typeof(EntityPartObject_AttributeValue),
                    typeof(EntityObject_Order),
                    typeof(EntityObject_OrderLine),
                    typeof(EntityPartObject_AttributeValueChoice),
                };
            }

            public override IEntityRepository[] GetEntityRepositories()
            {
                return new IEntityRepository[] {
                    (IEntityRepository)m_Products,
                    (IEntityRepository)m_Categories,
                    (IEntityRepository)m_Attributes,
                    null,
                    (IEntityRepository)m_Orders,
                    (IEntityRepository)m_OrdersLines,
                    null
                };
            }

            #endregion

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            #region Method NewOrderLine 

            public IR1.IOrderLine NewOrderLine(IR1.IOrder order, IR1.IProduct product, int quantity)
            {
                var entity = _entityFactory.NewEntity<IR1.IOrderLine>();

                entity.Order = order;
                entity.Product = product;
                entity.Quantity = quantity;

                return entity;
            }

            #endregion

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            #region Methid NewAttributeValue

            public IR1.IAttributeValue NewAttributeValue(string value, int displayOrder)
            {
                var entity = _entityFactory.NewEntity<IR1.IAttributeValue>();

                entity.Value = value;
                entity.DisplayOrder = displayOrder;

                return entity;
            }

            #endregion

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            #region Method NewAttributeValueChoice

            public IR1.IAttributeValueChoice NewAttributeValueChoice(IR1.IAttribute attribute, string value)
            {
                var entity = _entityFactory.NewEntity<IR1.IAttributeValueChoice>();

                entity.Attribute = attribute;
                entity.Value = value;

                return entity;
            }

            #endregion

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            #region Method NewPostalAdderss

            public IR1.IPostalAddress NewPostalAddress(string streetAddress, string city, string zipCode, string country)
            {
                var entity = _entityFactory.NewEntity<IR1.IPostalAddress>();

                entity.StreetAddress = streetAddress;
                entity.City = city;
                entity.ZipCode = zipCode;
                entity.Country = country;

                return entity;
            }

            #endregion

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            #region Method NewEmailContactDetail

            public Interfaces.Repository1.IEmailContactDetail NewEmailContactDetail(string email)
            {
                Interfaces.Repository1.IEmailContactDetail contactDeatil = this._entityFactory.NewEntity<Interfaces.Repository1.IEmailContactDetail>();
                contactDeatil.Email = email;
                return contactDeatil;
            }

            #endregion

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            #region Entity Repository Categories

            private IEntityRepository<IR1.ICategory> m_Categories; 

            public IEntityRepository<IR1.ICategory> Categories
            {
                get { return m_Categories; }
            }

            #endregion

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            #region Entity Repository Products

            private IEntityRepository<IR1.IProduct> m_Products;

            public IEntityRepository<IR1.IProduct> Products
            {
                get { return m_Products; }
            }

            #endregion

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            #region Entity Repository Orders

            private IEntityRepository<IR1.IOrder> m_Orders;

            public IEntityRepository<IR1.IOrder> Orders
            {
                get { return m_Orders; }
            }

            #endregion

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            #region Entity Repository OrdersLines

            private IEntityRepository<IR1.IOrderLine> m_OrdersLines;

            public IEntityRepository<IR1.IOrderLine> OrdersLines
            {
                get { return m_OrdersLines; }
            }

            #endregion

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            #region Entity Repository Attributes

            private IEntityRepository<IR1.IAttribute> m_Attributes;

            public IEntityRepository<IR1.IAttribute> Attributes
            {
                get { return m_Attributes; }
            }

            #endregion

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            #region Entity Repository Customers

            private IEntityRepository<IR1.ICustomer> m_Customers;

            public IEntityRepository<IR1.ICustomer> Customers
            {
                get { return m_Customers; }
            }

            #endregion

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            #region GetOrBuildDbCompiledModel

            private static object _s_compiledModel;
            private static object _s_compiledModelSyncRoot = new object();

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
                            // derived entities registered here...
                            //BsonClassMap.LookupClassMap(typeof(MongoEntityObject_FrontEndUserAccountEntity));
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

            #endregion



            IR1.IOrderLine IR1.IOnlineStoreRepository.NewOrderLine(IR1.IOrder order, IR1.IProduct product, int quantity)
            {
                throw new NotImplementedException();
            }

            IR1.IAttributeValue IR1.IOnlineStoreRepository.NewAttributeValue(string value, int displayOrder)
            {
                throw new NotImplementedException();
            }

            IR1.IAttributeValueChoice IR1.IOnlineStoreRepository.NewAttributeValueChoice(IR1.IAttribute attribute, string value)
            {
                throw new NotImplementedException();
            }

            IR1.IPostalAddress IR1.IOnlineStoreRepository.NewPostalAddress(string streetAddress, string city, string zipCode, string country)
            {
                throw new NotImplementedException();
            }

            IR1.IEmailContactDetail IR1.IOnlineStoreRepository.NewEmailContactDetail(string email)
            {
                throw new NotImplementedException();
            }

            IEntityRepository<IR1.ICategory> IR1.IOnlineStoreRepository.Categories
            {
                get { throw new NotImplementedException(); }
            }

            IEntityRepository<IR1.IProduct> IR1.IOnlineStoreRepository.Products
            {
                get { throw new NotImplementedException(); }
            }

            IEntityRepository<IR1.IOrder> IR1.IOnlineStoreRepository.Orders
            {
                get { throw new NotImplementedException(); }
            }

            IEntityRepository<IR1.IOrderLine> IR1.IOnlineStoreRepository.OrdersLines
            {
                get { throw new NotImplementedException(); }
            }

            IEntityRepository<IR1.IAttribute> IR1.IOnlineStoreRepository.Attributes
            {
                get { throw new NotImplementedException(); }
            }

            IEntityRepository<IR1.ICustomer> IR1.IOnlineStoreRepository.Customers
            {
                get { throw new NotImplementedException(); }
            }

            IEntityRepository<IR1.IContactDetail> IR1.IOnlineStoreRepository.ContactDetails
            {
                get { throw new NotImplementedException(); }
            }

            IEntityRepository<IR1.IEmailContactDetail> IR1.IOnlineStoreRepository.ContactEmails
            {
                get { throw new NotImplementedException(); }
            }

            void IApplicationDataRepository.InvokeGenericOperation(Type contractType, IDataRepositoryCallback callback)
            {
                throw new NotImplementedException();
            }

            Type[] IApplicationDataRepository.GetEntityTypesInRepository()
            {
                throw new NotImplementedException();
            }

            Type[] IApplicationDataRepository.GetEntityContractsInRepository()
            {
                throw new NotImplementedException();
            }

            IEntityRepository[] IApplicationDataRepository.GetEntityRepositories()
            {
                throw new NotImplementedException();
            }

            void IUnitOfWork.CommitChanges()
            {
                throw new NotImplementedException();
            }

            void IUnitOfWork.RollbackChanges()
            {
                throw new NotImplementedException();
            }

            bool IUnitOfWork.IsAutoCommitMode
            {
                get { throw new NotImplementedException(); }
            }

            UnitOfWorkState IUnitOfWork.UnitOfWorkState
            {
                get { throw new NotImplementedException(); }
            }

            void IDisposable.Dispose()
            {
                throw new NotImplementedException();
            }
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

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
                    return (TEntityContract)(object)new EntityObject_Order(_components);
                }
                if (typeof(TEntityContract) == typeof(Interfaces.Repository1.IOrderLine))
                {
                    return (TEntityContract)(object)new EntityObject_OrderLine(_components);
                }
                if (typeof(TEntityContract) == typeof(Interfaces.Repository1.IProduct))
                {
                    return (TEntityContract)(object)new EntityObject_Product(_components);
                }
                if (typeof(TEntityContract) == typeof(Interfaces.Repository1.ICategory))
                {
                    return (TEntityContract)(object)new EntityObject_Category(_components);
                }
                if (typeof(TEntityContract) == typeof(Interfaces.Repository1.IAttribute))
                {
                    return (TEntityContract)(object)new EntityObject_Attribute(_components);
                }
                if (typeof(TEntityContract) == typeof(Interfaces.Repository1.IAttributeValue))
                {
                    return (TEntityContract)(object)new EntityPartObject_AttributeValue(_components);
                }
                if (typeof(TEntityContract) == typeof(Interfaces.Repository1.IAttributeValueChoice))
                {
                    return (TEntityContract)(object)new EntityPartObject_AttributeValueChoice(_components);
                }
                if (typeof(TEntityContract) == typeof(Interfaces.Repository1.IPostalAddress))
                {
                    return (TEntityContract)(object)new EntityPartObject_PostalAddress(_components);
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

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Entity Order

        public class EntityObject_Order : 
            Interfaces.Repository1.IOrder, 
            IEntityPartId<ObjectId>, 
            IEntityObject, 
            IHaveDependencies,
            IHaveNestedObjects
        {
            private IComponentContext _components;

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            #region Constructors

            public EntityObject_Order()
            {
            }
            public EntityObject_Order(IComponentContext components)
            {
                InjectDependencies(components);

                // For property OrderLines
                this.m_OrderLines_ContractValue = new List<Interfaces.Repository1.IOrderLine>();
                this.m_OrderLines_ValueState = DualValueStates.Contract;

                // For property BillingAddress
                this.m_BillingAddress = new EntityPartObject_PostalAddress(components);

                // For property DeliveryAddress
                this.m_DeliveryAddress = new EntityPartObject_PostalAddress(components);
                    
                // For property Status
                this.m_Status = Interfaces.Repository1.OrderStatus.New;

                // For property Id
                this.m_Id = ObjectId.GenerateNewId();
            }

            #endregion

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            #region IEntityObject

            IEntityId IEntityObject.GetId()
            {
                return new EntityId<Interfaces.Repository1.IOrder, ObjectId>(this.Id);
            }
            void IEntityObject.SetId(object value)
            {
                this.Id = (ObjectId)value;
            }
            Type IObject.ContractType
            {
                get { return typeof(Interfaces.Repository1.IOrder); }
            }

            #endregion

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            #region IHaveDependencies

            public void InjectDependencies(IComponentContext components)
            {
                _components = components;
            }

            #endregion

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            #region IHaveNestedObjects

            void IHaveNestedObjects.DeepListNestedObjects(HashSet<object> nestedObjects)
            {
                // For property OrderLines
                if ( m_OrderLines_ValueState.HasFlag(DualValueStates.Contract) )
                {
                    nestedObjects.UnionWith(m_OrderLines_ContractValue);

                    foreach ( var orderLine in m_OrderLines_ContractValue )
                    {
                        ((IHaveNestedObjects)orderLine).DeepListNestedObjects(nestedObjects);
                    }
                }
            }

            #endregion

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            #region Property Id

            private ObjectId m_Id;

            public ObjectId Id
            {
                get { return this.m_Id; }
                set { this.m_Id = value; }
            }

            #endregion

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            #region Property OrderNo

            private string m_OrderNo;

            public string OrderNo
            {
                get { return this.m_OrderNo; }
                set { this.m_OrderNo = value; }
            }

            #endregion

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            #region Property OrderLines

            private ICollection<Interfaces.Repository1.IOrderLine> m_OrderLines_ContractValue;
            private ObjectId[] m_OrderLines_StorageValue;
            private DualValueStates m_OrderLines_ValueState;
            
            [BsonIgnore]
            ICollection<Interfaces.Repository1.IOrderLine> Interfaces.Repository1.IOrder.OrderLines
            {
                get
                {
                    if ( this.m_OrderLines_ValueState == DualValueStates.Storage )
                    {
                        this.m_OrderLines_ContractValue = LazyLoad_OrderLines(m_OrderLines_StorageValue);
                        this.m_OrderLines_ValueState = DualValueStates.Contract; // = instead of |=, because collection is mutable
                    }
                    return this.m_OrderLines_ContractValue;
                }
            }

            public virtual ObjectId[] OrderLines
            {
                get
                {
                    if ( this.m_OrderLines_ValueState == DualValueStates.Contract )
                    {
                        this.m_OrderLines_StorageValue = m_OrderLines_ContractValue.Select(e => EntityId.Of(e).ValueAs<ObjectId>()).ToArray();
                        this.m_OrderLines_ValueState |= DualValueStates.Storage;
                    }
                    return this.m_OrderLines_StorageValue;
                }
                set
                {
                    this.m_OrderLines_StorageValue = value;
                    this.m_OrderLines_ValueState = DualValueStates.Storage;
                }
            }

            private Interfaces.Repository1.IOrderLine[] LazyLoad_OrderLines(ObjectId[] ids)
            {
                return MongoDataRepositoryBase.ResolveFrom(_components).LazyLoadByIdList<Interfaces.Repository1.IOrderLine, ObjectId>(ids).ToArray();
            }

            #endregion

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            #region Property BillingAddress

            private EntityPartObject_PostalAddress m_BillingAddress;

            [BsonIgnore]
            Interfaces.Repository1.IPostalAddress Interfaces.Repository1.IOrder.BillingAddress
            {
                get
                {
                    return this.m_BillingAddress;
                }
            }

            public virtual EntityPartObject_PostalAddress BillingAddress
            {
                get { return m_BillingAddress; }
                set { m_BillingAddress = value; }
            }

            #endregion

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            #region Property DeliveryAddress

            private EntityPartObject_PostalAddress m_DeliveryAddress;

            [BsonIgnore]
            Interfaces.Repository1.IPostalAddress Interfaces.Repository1.IOrder.DeliveryAddress
            {
                get
                {
                    return this.m_DeliveryAddress;
                }
            }

            public virtual EntityPartObject_PostalAddress DeliveryAddress
            {
                get { return m_DeliveryAddress; }
                set { m_DeliveryAddress = value; }
            }

            #endregion

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            #region Property PlacedAt

            private DateTime m_PlacedAt;

            public DateTime PlacedAt
            {
                get { return this.m_PlacedAt; }
                set { this.m_PlacedAt = value; }
            }

            #endregion

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            #region Property Status

            private Interfaces.Repository1.OrderStatus m_Status;

            public Interfaces.Repository1.OrderStatus Status
            {
                get { return m_Status; }
                set { m_Status = value; }
            }

            #endregion
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Entity OrderLine

        public class EntityObject_OrderLine : 
            Interfaces.Repository1.IOrderLine, 
            IEntityPartId<ObjectId>, 
            IEntityObject, 
            IHaveDependencies,
            IHaveNestedObjects
        {
            private IComponentContext _components;

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            #region Constructors

            public EntityObject_OrderLine()
            {
            }
            public EntityObject_OrderLine(IComponentContext components)
            {
                InjectDependencies(components);

                this.m_Id = ObjectId.GenerateNewId();
                this.m_Attributes_Adapter =
                    new EntityObjectFactory.CollectionAdapter<EntityPartObject_AttributeValueChoice, Interfaces.Repository1.IAttributeValueChoice>(this.m_Attributes);
            }

            #endregion

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            #region IEntityObject

            IEntityId IEntityObject.GetId()
            {
                return new EntityId<Interfaces.Repository1.IOrderLine, ObjectId>(this.Id);
            }
            void IEntityObject.SetId(object value)
            {
                this.Id = (ObjectId)value;
            }
            Type IObject.ContractType
            {
                get { return typeof(Interfaces.Repository1.IOrderLine); }
            }

            #endregion

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            #region IHaveDependencies

            public void InjectDependencies(IComponentContext components)
            {
                _components = components;
            }

            #endregion

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            #region IHaveNestedObjects

            void IHaveNestedObjects.DeepListNestedObjects(HashSet<object> nestedObjects)
            {
                // For property Product
                if ( m_Product_ValueState.HasFlag(DualValueStates.Contract) )
                {
                    nestedObjects.Add(m_Product_ContractValue);
                    ((IHaveNestedObjects)m_Product_ContractValue).DeepListNestedObjects(nestedObjects);
                }

                // For property Order
                if ( m_Order_ValueState.HasFlag(DualValueStates.Contract) )
                {
                    nestedObjects.Add(m_Order_ContractValue);
                    ((IHaveNestedObjects)m_Order_ContractValue).DeepListNestedObjects(nestedObjects);
                }

                // For property Attributes
                if ( m_Attributes != null )
                {
                    nestedObjects.UnionWith(m_Attributes);
                    
                    foreach ( var attribute in m_Attributes )
                    {
                        ((IHaveNestedObjects)attribute).DeepListNestedObjects(nestedObjects);
                    }
                }
            }

            #endregion

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            #region Property Id

            private ObjectId m_Id;

            public ObjectId Id
            {
                get { return m_Id; }
                set { m_Id = value; }
            }

            #endregion

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            #region Property Product

            private Interfaces.Repository1.IProduct m_Product_ContractValue;
            private ObjectId m_Product_StorageValue;
            private DualValueStates m_Product_ValueState;

            [BsonIgnore]
            Interfaces.Repository1.IProduct Interfaces.Repository1.IOrderLine.Product
            {
                get
                {
                    if (this.m_Product_ValueState == DualValueStates.Storage)
                    {
                        this.m_Product_ContractValue = LazyLoad_Product(m_Product_StorageValue);
                        this.m_Product_ValueState |= DualValueStates.Contract;
                    }
                    return this.m_Product_ContractValue;
                }
                set
                {
                    this.m_Product_ContractValue = value;
                    this.m_Product_ValueState = DualValueStates.Contract;
                }
            }

            public virtual ObjectId Product
            {
                get
                {
                    if (this.m_Product_ValueState == DualValueStates.Contract)
                    {
                        this.m_Product_StorageValue = EntityId.Of(m_Product_ContractValue).ValueAs<ObjectId>();
                        this.m_Product_ValueState |= DualValueStates.Storage;
                    }
                    return this.m_Product_StorageValue;
                }
                set
                {
                    this.m_Product_StorageValue = value;
                    this.m_Product_ValueState = DualValueStates.Storage;
                }
            }

            private Interfaces.Repository1.IProduct LazyLoad_Product(ObjectId id)
            {
                return MongoDataRepositoryBase.ResolveFrom(_components).LazyLoadById<Interfaces.Repository1.IProduct, ObjectId>(id);

                //using (var repo = _components.Resolve<IFramework>().NewUnitOfWork<Interfaces.Repository1.IOnlineStoreRepository>())
                //using (var repo = CurrentDataRepoFactory())
                //{
                //    var mongoRepo = (MongoDataRepositoryBase)repo;
                //    var query = Query.EQ("_id", new BsonObjectId(id));
                //    var queryResult = mongoRepo.GetCollection<EntityObject_Product>("Product").FindOne(query);
                    
                //    ObjectUtility.InjectDependenciesToObject(queryResult, _components);
                 
                //    return queryResult;
                //}
            }

            #endregion

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            #region Property Order

            private Interfaces.Repository1.IOrder m_Order_ContractValue;
            private ObjectId m_Order_StorageValue;
            private DualValueStates m_Order_ValueState;

            [BsonIgnore]
            Interfaces.Repository1.IOrder Interfaces.Repository1.IOrderLine.Order
            {
                get
                {
                    if (this.m_Order_ValueState == DualValueStates.Storage)
                    {
                        this.m_Order_ContractValue = LazyLoad_Order(m_Order_StorageValue);
                        this.m_Order_ValueState |= DualValueStates.Contract;
                    }
                    return this.m_Order_ContractValue;
                }
                set
                {
                    this.m_Order_ContractValue = value;
                    this.m_Order_ValueState = DualValueStates.Contract;
                }
            }

            public virtual ObjectId Order
            {
                get
                {
                    if (this.m_Order_ValueState == DualValueStates.Contract)
                    {
                        this.m_Order_StorageValue = EntityId.Of(m_Order_ContractValue).ValueAs<ObjectId>();
                        this.m_Order_ValueState |= DualValueStates.Storage;
                    }
                    return this.m_Order_StorageValue;
                }
                set
                {
                    this.m_Order_StorageValue = value;
                    this.m_Order_ValueState = DualValueStates.Storage;
                }
            }

            private Interfaces.Repository1.IOrder LazyLoad_Order(ObjectId id)
            {
                return MongoDataRepositoryBase.ResolveFrom(_components).LazyLoadById<Interfaces.Repository1.IOrder, ObjectId>(id);

                //using (var repo = _components.Resolve<IFramework>().NewUnitOfWork<Interfaces.Repository1.IOnlineStoreRepository>())
                //using (var repo = CurrentDataRepoFactory())
                //{
                //    var mongoRepo = (MongoDataRepositoryBase)repo;
                //    var query = Query.EQ("_id", new BsonObjectId(id));
                //    var queryResult = mongoRepo.GetCollection<EntityObject_Order>("Order").FindOne(query);

                //    ObjectUtility.InjectDependenciesToObject(queryResult, _components);
                    
                //    return queryResult;
                //}
            }

            #endregion

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            #region Property Quantity

            private int m_Quantity;

            public int Quantity
            {
                get { return this.m_Quantity; }
                set { this.m_Quantity = value; }
            }

            #endregion

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            #region Property Attributes

            private ICollection<EntityPartObject_AttributeValueChoice> m_Attributes = new HashSet<EntityPartObject_AttributeValueChoice>();
            private EntityObjectFactory.CollectionAdapter<EntityPartObject_AttributeValueChoice, Interfaces.Repository1.IAttributeValueChoice> m_Attributes_Adapter;

            [BsonIgnore]
            ICollection<Interfaces.Repository1.IAttributeValueChoice> Interfaces.Repository1.IOrderLine.Attributes
            {
                get { return this.m_Attributes_Adapter; }
            }

            public virtual ICollection<EntityPartObject_AttributeValueChoice> Attributes
            {
                get { return this.m_Attributes; }
                set
                {
                    this.m_Attributes_Adapter = new EntityObjectFactory.CollectionAdapter<EntityPartObject_AttributeValueChoice, Interfaces.Repository1.IAttributeValueChoice>(value);
                    this.m_Attributes = value;

                    ObjectUtility.InjectDependenciesToManyObjects(value, _components);
                }
            }

            #endregion
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Entity Product

        public class EntityObject_Product : 
            Interfaces.Repository1.IProduct, 
            IEntityPartId<ObjectId>, 
            IEntityObject, 
            IHaveDependencies,
            IHaveNestedObjects
        {
            private IComponentContext _components;

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            #region Constructors

            public EntityObject_Product()
            {
            }
            public EntityObject_Product(IComponentContext components)
            {
                InjectDependencies(components);

                // For property Id
                this.Id = ObjectId.GenerateNewId();
                
                // For property Categories
                this.m_Categories_ContractValue = new HashSet<IR1.ICategory>();
                this.m_Categories_ValueState = DualValueStates.Contract;

                // For property Attributes
                this.m_Attributes_ContractValue = new HashSet<IR1.IAttribute>();
                this.m_Attributes_ValueState = DualValueStates.Contract;
            }

            #endregion

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            #region IEntityObject

            IEntityId IEntityObject.GetId()
            {
                return new EntityId<Interfaces.Repository1.IProduct, ObjectId>(this.Id);
            }
            void IEntityObject.SetId(object value)
            {
                this.Id = (ObjectId)value;
            }
            Type IObject.ContractType
            {
                get { return typeof(Interfaces.Repository1.IProduct); }
            }

            #endregion

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            #region IHaveDependencies

            public void InjectDependencies(IComponentContext components)
            {
                _components = components;
            }
            
            #endregion

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            #region IHaveNestedObjects
            
            void IHaveNestedObjects.DeepListNestedObjects(HashSet<object> nestedObjects)
            {
                // For property Categories
                if ( m_Categories_ValueState.HasFlag(DualValueStates.Contract) )
                {
                    nestedObjects.UnionWith(m_Categories_ContractValue);
                }

                // For property Attributes
                if ( m_Attributes_ValueState.HasFlag(DualValueStates.Contract) )
                {
                    nestedObjects.UnionWith(m_Attributes_ContractValue);

                    foreach ( var attribute in m_Attributes_ContractValue )
                    {
                        ((IHaveNestedObjects)attribute).DeepListNestedObjects(nestedObjects);
                    }
                }
            }

            #endregion

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            #region Property Id

            private ObjectId m_Id;

            public ObjectId Id
            {
                get { return m_Id; }
                set { m_Id = value; }
            }

            #endregion

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            #region Property CatalogNo

            private string m_CatalogNo;

            public string CatalogNo
            {
                get { return this.m_CatalogNo; }
                set { this.m_CatalogNo = value; }
            }

            #endregion

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            #region Property Name

            private string m_Name;

            public string Name
            {
                get { return this.m_Name; }
                set { this.m_Name = value; }
            }

            #endregion

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            #region Property Price

            private decimal m_Price;

            public decimal Price
            {
                get { return this.m_Price; }
                set { this.m_Price = value; }
            }

            #endregion

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            #region Property Categories

            private ICollection<Interfaces.Repository1.ICategory> m_Categories_ContractValue;
            private ObjectId[] m_Categories_StorageValue;
            private DualValueStates m_Categories_ValueState;

            [BsonIgnore]
            ICollection<Interfaces.Repository1.ICategory> Interfaces.Repository1.IProduct.Categories
            {
                get
                {
                    if ( this.m_Categories_ValueState == DualValueStates.Storage )
                    {
                        this.m_Categories_ContractValue = LazyLoad_Categories(m_Categories_StorageValue);
                        this.m_Categories_ValueState = DualValueStates.Contract; // = instead of |=, because collection is mutable
                    }
                    return this.m_Categories_ContractValue;
                }
            }

            public virtual ObjectId[] Categories
            {
                get
                {
                    if (this.m_Categories_ValueState == DualValueStates.Contract)
                    {
                        this.m_Categories_StorageValue = m_Categories_ContractValue.Select(e => EntityId.Of(e).ValueAs<ObjectId>()).ToArray();
                        this.m_Categories_ValueState |= DualValueStates.Storage;
                    }
                    return this.m_Categories_StorageValue;
                }
                set
                {
                    this.m_Categories_StorageValue = value;
                    this.m_Categories_ValueState = DualValueStates.Storage;
                }
            }

            private Interfaces.Repository1.ICategory[] LazyLoad_Categories(ObjectId[] ids)
            {
                return MongoDataRepositoryBase.ResolveFrom(_components).LazyLoadByIdList<Interfaces.Repository1.ICategory, ObjectId>(ids).ToArray();

                //using ( var repo = _components.Resolve<IFramework>().NewUnitOfWork<Interfaces.Repository1.IOnlineStoreRepository>() )
                //using (var repo = CurrentDataRepoFactory())
                //{
                //    var mongoRepo = (MongoDataRepositoryBase)repo;
                //    var query = mongoRepo.GetCollection<EntityObject_Category>("Category").Find(Query.In("_id", new BsonArray(ids)));
                //    return query.InjectDependenciesFrom(_components).Cast<Interfaces.Repository1.ICategory>().ToArray();
                //}
            }

            #endregion

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            #region Property Attributes

            private ICollection<Interfaces.Repository1.IAttribute> m_Attributes_ContractValue;
            private ObjectId[] m_Attributes_StorageValue;
            private DualValueStates m_Attributes_ValueState;

            [BsonIgnore]
            ICollection<Interfaces.Repository1.IAttribute> Interfaces.Repository1.IProduct.Attributes
            {
                get
                {
                    if (this.m_Attributes_ValueState == DualValueStates.Storage)
                    {
                        this.m_Attributes_ContractValue = LazyLoad_Attributes(m_Attributes_StorageValue);
                        this.m_Attributes_ValueState = DualValueStates.Contract; // = instead of |=, because collection is mutable
                    }
                    return this.m_Attributes_ContractValue;
                }
            }

            public virtual ObjectId[] Attributes
            {
                get
                {
                    if (this.m_Attributes_ValueState == DualValueStates.Contract)
                    {
                        this.m_Attributes_StorageValue = m_Attributes_ContractValue.Select(e => EntityId.Of(e).ValueAs<ObjectId>()).ToArray();
                        this.m_Attributes_ValueState |= DualValueStates.Storage;
                    }
                    return this.m_Attributes_StorageValue;
                }
                set
                {
                    this.m_Attributes_StorageValue = value;
                    this.m_Attributes_ValueState = DualValueStates.Storage;
                }
            }

            private Interfaces.Repository1.IAttribute[] LazyLoad_Attributes(ObjectId[] ids)
            {
                return MongoDataRepositoryBase.ResolveFrom(_components).LazyLoadByIdList<Interfaces.Repository1.IAttribute, ObjectId>(ids).ToArray();

                //using (var repo = _components.Resolve<IFramework>().NewUnitOfWork<Interfaces.Repository1.IOnlineStoreRepository>())
                //using (var repo = CurrentDataRepoFactory())
                //{
                //    var mongoRepo = (MongoDataRepositoryBase)repo;
                //    var query = mongoRepo.GetCollection<EntityObject_Attribute>("Attribute").Find(Query.In("_id", new BsonArray(ids)));
                //    return query.InjectDependenciesFrom(_components).Cast<Interfaces.Repository1.IAttribute>().ToArray();
                //}
            }

            #endregion
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Entity Category

        public class EntityObject_Category : Interfaces.Repository1.ICategory, IEntityPartId<ObjectId>, IEntityObject
        {
            #region Constructors

            public EntityObject_Category()
            {
            }
            public EntityObject_Category(IComponentContext components)
            {
                this.m_Id = ObjectId.GenerateNewId();
            }

            #endregion

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            #region IEntityObject

            IEntityId IEntityObject.GetId()
            {
                return new EntityId<Interfaces.Repository1.ICategory, ObjectId>(this.Id);
            }
            void IEntityObject.SetId(object value)
            {
                this.Id = (ObjectId)value;
            }
            Type IObject.ContractType
            {
                get { return typeof(Interfaces.Repository1.ICategory); }
            }

            #endregion

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            #region Property Id

            private ObjectId m_Id;

            public ObjectId Id
            {
                get { return m_Id; }
                set { m_Id = value; }
            }

            #endregion

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            #region Property Name

            private string m_Name;

            public string Name
            {
                get { return this.m_Name; }
                set { this.m_Name = value; }
            }

            #endregion
        }

        #endregion

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        #region Entity Attribute

        public class EntityObject_Attribute : 
            Interfaces.Repository1.IAttribute, 
            IEntityPartId<ObjectId>, 
            IEntityObject, 
            IHaveDependencies,
            IHaveNestedObjects
        {
            private IComponentContext _components;

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            #region Constructors

            public EntityObject_Attribute()
            {
            }
            public EntityObject_Attribute(IComponentContext arg0)
            {
                m_Values_Adapter = new EntityObjectFactory.ListAdapter<EntityPartObject_AttributeValue, IR1.IAttributeValue>(m_Values);
                this.Id = ObjectId.GenerateNewId();
            }

            #endregion

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            #region IEntityObject

            IEntityId IEntityObject.GetId()
            {
                return new EntityId<Interfaces.Repository1.IAttribute, ObjectId>(this.Id);
            }
            void IEntityObject.SetId(object value)
            {
                this.Id = (ObjectId)value;
            }
            Type IObject.ContractType
            {
                get { return typeof(Interfaces.Repository1.IAttribute); }
            }

            #endregion

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            #region IHaveDependencies

            void IHaveDependencies.InjectDependencies(IComponentContext components)
            {
                _components = components;
            }

            #endregion

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            #region IHaveNestedObjects

            void IHaveNestedObjects.DeepListNestedObjects(HashSet<object> nestedObjects)
            {
                // For property Values
                nestedObjects.UnionWith(m_Values);
            }

            #endregion

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            #region Property Id

            private ObjectId m_Id;

            public ObjectId Id
            {
                get { return m_Id; }
                set { m_Id = value; }
            }

            #endregion

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            #region Property Values

            private IList<EntityPartObject_AttributeValue> m_Values = new List<EntityPartObject_AttributeValue>();
            private EntityObjectFactory.ListAdapter<EntityPartObject_AttributeValue, Interfaces.Repository1.IAttributeValue> m_Values_Adapter;

            [BsonIgnore]
            IList<Interfaces.Repository1.IAttributeValue> Interfaces.Repository1.IAttribute.Values
            {
                get { return this.m_Values_Adapter; }
            }

            public virtual IList<EntityPartObject_AttributeValue> Values
            {
                get
                {
                    return this.m_Values;
                }
                set
                {
                    this.m_Values_Adapter = new EntityObjectFactory.ListAdapter<EntityPartObject_AttributeValue, Interfaces.Repository1.IAttributeValue>(value);
                    this.m_Values = value;
                    ObjectUtility.InjectDependenciesToManyObjects(value, _components);
                }
            }

            #endregion

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            #region Property Name

            private string m_Name;

            public string Name
            {
                get { return m_Name; }
                set { m_Name = value; }
            }

            #endregion

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            #region Property TitleForUser

            private string m_TitleForUser;

            public string TitleForUser
            {
                get { return m_TitleForUser; }
                set { m_TitleForUser = value; }
            }

            #endregion
        }

        #endregion

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        #region Entity Part AttributeValue

        public class EntityPartObject_AttributeValue : Interfaces.Repository1.IAttributeValue, IEntityPartObject
        {
            #region Constructors

            public EntityPartObject_AttributeValue()
            {
            }
            public EntityPartObject_AttributeValue(IComponentContext components)
            {
            }

            #endregion

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            #region IEntityPartObject

            Type IObject.ContractType
            {
                get { return typeof(Interfaces.Repository1.IAttributeValue); }
            }

            #endregion

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            #region Property DisplayOrder

            private int m_DisplayOrder;

            public int DisplayOrder
            {
                get { return m_DisplayOrder; }
                set { m_DisplayOrder = value; }
            }

            #endregion

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            #region Property Value

            private string m_Value;

            public string Value
            {
                get { return m_Value; }
                set { m_Value = value; }
            }

            #endregion
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Entity Part AttributeValueChoice

        public class EntityPartObject_AttributeValueChoice : 
            Interfaces.Repository1.IAttributeValueChoice, 
            IEntityPartObject, 
            IHaveDependencies, 
            IHaveNestedObjects
        {
            private IComponentContext _components;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            #region Constructors

            public EntityPartObject_AttributeValueChoice()
            {
            }
            public EntityPartObject_AttributeValueChoice(IComponentContext components)
            {
                InjectDependencies(components);
            }

            #endregion

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            #region IObject

            Type IObject.ContractType
            {
                get { return typeof(Interfaces.Repository1.IAttributeValue); }
            }

            #endregion

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            #region IHaveDependencies

            public virtual void InjectDependencies(IComponentContext components)
            {
                _components = components;
            }

            #endregion

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            #region IHaveNestedObjects

            void IHaveNestedObjects.DeepListNestedObjects(HashSet<object> nestedObjects)
            {
                // For property Attribute
                if ( m_Attribute_ValueState.HasFlag(DualValueStates.Contract) )
                {
                    nestedObjects.Add(m_Attribute_ContractValue);
                }
            }

            #endregion

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            #region Property Attribute

            private Interfaces.Repository1.IAttribute m_Attribute_ContractValue;
            private ObjectId m_Attribute_StorageValue;
            private DualValueStates m_Attribute_ValueState;

            Interfaces.Repository1.IAttribute Interfaces.Repository1.IAttributeValueChoice.Attribute
            {
                get
                {
                    if ( this.m_Attribute_ValueState == DualValueStates.Storage )
                    {
                        this.m_Attribute_ContractValue = LazyLoad_Attribute(this.Attribute);
                        this.m_Attribute_ValueState |= DualValueStates.Contract;
                    }
                    return this.m_Attribute_ContractValue;
                }
                set
                {
                    this.m_Attribute_ContractValue = value;
                    this.m_Attribute_ValueState = DualValueStates.Contract;
                }
            }

            public virtual ObjectId Attribute
            {
                get
                {
                    if ( this.m_Attribute_ValueState == DualValueStates.Contract )
                    {
                        this.m_Attribute_StorageValue = EntityId.Of(this.m_Attribute_ContractValue).ValueAs<ObjectId>();
                        this.m_Attribute_ValueState |= DualValueStates.Storage;
                    }
                    return this.m_Attribute_StorageValue;
                }
                set
                {
                    this.m_Attribute_StorageValue = value;
                    this.m_Attribute_ValueState = DualValueStates.Storage;
                }
            }

            private Interfaces.Repository1.IAttribute LazyLoad_Attribute(ObjectId id)
            {
                return MongoDataRepositoryBase.ResolveFrom(_components).LazyLoadById<Interfaces.Repository1.IAttribute, ObjectId>(id);
            }

            #endregion

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            #region Property Value

            private string m_Value;

            public string Value
            {
                get { return m_Value; }
                set { m_Value = value; }
            }

            #endregion
        }

        #endregion

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        #region Entity Part PostalAddress

        public class EntityPartObject_PostalAddress : Interfaces.Repository1.IPostalAddress, IEntityPartObject
        {
            #region Constructors

            public EntityPartObject_PostalAddress()
            {
            }
            public EntityPartObject_PostalAddress(IComponentContext components)
            {
            }

            #endregion

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            #region IEntityPartObject

            Type IObject.ContractType
            {
                get { return typeof(Interfaces.Repository1.IPostalAddress); }
            }

            #endregion

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            #region Property StreetAddress

            private string m_StreetAddress;

            public string StreetAddress
            {
                get { return m_StreetAddress; }
                set { m_StreetAddress = value; }
            }

            #endregion

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            #region Property City

            private string m_City;

            public string City
            {
                get { return m_City; }
                set { m_City = value; }
            }

            #endregion

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            #region Property ZipCode

            private string m_ZipCode;

            public string ZipCode
            {
                get { return m_ZipCode; }
                set { m_ZipCode = value; }
            }

            #endregion

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            #region Property Country

            private string m_Country;

            public string Country
            {
                get { return m_Country; }
                set { m_Country = value; }
            }

            #endregion
        }

        #endregion
    }
}
