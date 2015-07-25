using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using NUnit.Framework;
using NWheels.Concurrency;
using NWheels.Entities;
using NWheels.Extensions;
using NWheels.Stacks.EntityFramework.Conventions;
using NWheels.Stacks.EntityFramework.EFConventions;
using NWheels.Stacks.EntityFramework.Impl;
using System.Data.Entity.ModelConfiguration;
using System.Linq.Expressions;
using Newtonsoft.Json;
using NWheels.Conventions.Core;
using NWheels.DataObjects;
using NWheels.DataObjects.Core.StorageTypes;
using NWheels.Entities.Core;
using NWheels.Testing.Entities.Stacks;

namespace NWheels.Stacks.EntityFramework.ComponentTests
{
    public static class HardCodedImplementations
    {
        private static int _s_nextIntId = 1;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static class Repository1
        {
            public static void RegisterEntityFineTunings(ContainerBuilder builder)
            {
                builder.NWheelsFeatures()
                    .Entities()
                    .RegisterRelationalMappingFineTune<Interfaces.Repository1.IProduct>(
                        ft => ft.Table("MY_PRODUCTS").Column(p => p.Price, columnName: "MY_SPECIAL_PRICE_COLUMN", dataType: "MONEY"));

                builder.NWheelsFeatures()
                    .Entities()
                    .RegisterRelationalMappingFineTune<Interfaces.Repository1.IOrder>(
                        ft => ft.Table("MY_ORDERS").Column(o => o.OrderNo, columnName: "MY_SPECIAL_ORDER_NO_COLUMN"));

                builder.NWheelsFeatures()
                    .Entities()
                    .RegisterRelationalMappingFineTune<Interfaces.Repository1.IOrderLine>(
                        ft => ft.Table("MY_ORDER_LINES").Column(ol => ol.Product, columnName: "MY_SPECIAL_PRODUCT_ID_COLUMN"));
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public class DataRepositoryObject_DataRepository : EfDataRepositoryBase, Interfaces.Repository1.IOnlineStoreRepository
            {
                private EfEntityRepository<Interfaces.Repository1.ICategory, EntityObject_Category> m_Categories;
                private EfEntityRepository<Interfaces.Repository1.IOrder, EntityObject_Order> m_Orders;
                private EfEntityRepository<Interfaces.Repository1.IProduct, EntityObject_Product> m_Products;
                private EfEntityRepository<Interfaces.Repository1.IAttribute, EntityObject_Attribute> m_Attributes;

                public DataRepositoryObject_DataRepository(DbConnection connection, bool autoCommit)
                    : base(new HardCodedEntityFactory(), GetOrBuildDbCompoledModel(connection), connection, autoCommit)
                {
                    this.m_Categories = new EfEntityRepository<Interfaces.Repository1.ICategory, EntityObject_Category>(this);
                    this.m_Products = new EfEntityRepository<Interfaces.Repository1.IProduct, EntityObject_Product>(this);
                    this.m_Orders = new EfEntityRepository<Interfaces.Repository1.IOrder, EntityObject_Order>(this);
                    this.m_Attributes = new EfEntityRepository<Interfaces.Repository1.IAttribute, EntityObject_Attribute>(this);
                }

                public override sealed Type[] GetEntityTypesInRepository()
                {
                    return new Type[] {
                        typeof(EntityObject_Category),
                        typeof(EntityObject_Product),
                        typeof(EntityObject_Order),
                        typeof(EntityObject_OrderLine),
                        typeof(EntityObject_Attribute)
                    };
                }

                public override Type[] GetEntityContractsInRepository()
                {
                    return new Type[] {
                        typeof(Interfaces.Repository1.ICategory),
                        typeof(Interfaces.Repository1.IProduct),
                        typeof(Interfaces.Repository1.IOrder),
                        typeof(Interfaces.Repository1.IOrderLine),
                        typeof(Interfaces.Repository1.IAttribute),
                    };
                }

                public override IEntityRepository[] GetEntityRepositories()
                {
                    return new IEntityRepository[] { m_Categories, m_Orders, m_Products, null, m_Attributes };
                }

                public Interfaces.Repository1.IOrderLine NewOrderLine(
                    Interfaces.Repository1.IOrder order,
                    Interfaces.Repository1.IProduct product,
                    int quantity)
                {
                    Interfaces.Repository1.IOrderLine orderLine = new EntityObject_OrderLine();
                    orderLine.Order = order;
                    orderLine.Product = product;
                    orderLine.Quantity = quantity;
                    return orderLine;
                }

                public Interfaces.Repository1.IAttributeValue NewAttributeValue(
                    string value, 
                    int displayOrder)
                {
                    Interfaces.Repository1.IAttributeValue valuePart = new EntityObject_AttributeValue();
                    valuePart.Value = value;
                    valuePart.DisplayOrder = displayOrder;
                    return valuePart;
                }

                public Interfaces.Repository1.IAttributeValueChoice NewAttributeValueChoice(
                    Interfaces.Repository1.IAttribute attribute, 
                    string value)
                {
                    Interfaces.Repository1.IAttributeValueChoice choicePart = new EntityObject_AttributeValueChoice();
                    choicePart.Attribute = attribute;
                    choicePart.Value = value;
                    return choicePart;
                }

                public IEntityRepository<Interfaces.Repository1.ICategory> Categories
                {
                    get { return this.m_Categories; }
                }

                public IEntityRepository<Interfaces.Repository1.IOrder> Orders
                {
                    get { return this.m_Orders; }
                }

                public IEntityRepository<Interfaces.Repository1.IProduct> Products
                {
                    get { return this.m_Products; }
                }

                public IEntityRepository<Interfaces.Repository1.IAttribute> Attributes
                {
                    get { return this.m_Attributes; }
                }

                private static readonly object _s_compiledModelSyncRoot = new object();
                private static DbCompiledModel _s_compiledModel;

                private static DbCompiledModel GetOrBuildDbCompoledModel(DbConnection connection)
                {
                    if ( _s_compiledModel == null )
                    {
                        lock ( _s_compiledModelSyncRoot )
                        {
                            if ( _s_compiledModel == null )
                            {
                                var modelBuilder = new DbModelBuilder();

                                modelBuilder.Conventions.Add(new NoUnderscoreForeignKeyNamingConvention());

                                var objectSetCategories = modelBuilder.Entity<EntityObject_Category>().HasEntitySetName("Category").ToTable("Categories");
                                var objectSetProducts = modelBuilder.Entity<EntityObject_Product>().HasEntitySetName("Product").ToTable("Products");
                                var objectSetOrders = modelBuilder.Entity<EntityObject_Order>().HasEntitySetName("Order").ToTable("Orders");
                                var objectSetOrderLines = modelBuilder.Entity<EntityObject_OrderLine>().HasEntitySetName("OrderLine").ToTable("OrderLines");
                                var objectSetAttributes = modelBuilder.Entity<EntityObject_Attribute>().HasEntitySetName("Attribute").ToTable("Attributes");

                                objectSetProducts.HasMany(x => x.Categories).WithMany(x => x.Products).Map(
                                    cfg => {
                                        cfg.MapLeftKey("ProductId");
                                        cfg.MapRightKey("CategoryId");
                                        cfg.ToTable("Products_Categories");
                                    });

                                objectSetProducts.HasMany(x => x.Attributes).WithMany(x => x.Products).Map(
                                    cfg => {
                                        cfg.MapLeftKey("ProductId");
                                        cfg.MapRightKey("AttributeId");
                                        cfg.ToTable("Products_Attributes");
                                    });

                                var model = modelBuilder.Build(connection);
                                _s_compiledModel = model.Compile();
                            }
                        }
                    }

                    return _s_compiledModel;
                }


                public IEntityRepository<Interfaces.Repository1.IOrderLine> OrdersLines
                {
                    get { throw new NotImplementedException(); }
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public class DataRepositoryObject_CustomNames : EfDataRepositoryBase, Interfaces.Repository1.IOnlineStoreRepository
            {
                private EfEntityRepository<Interfaces.Repository1.ICategory, EntityObject_Category> m_Categories;
                private EfEntityRepository<Interfaces.Repository1.IOrder, EntityObject_Order> m_Orders;
                private EfEntityRepository<Interfaces.Repository1.IProduct, EntityObject_Product> m_Products;

                public DataRepositoryObject_CustomNames(ITypeMetadataCache metadataCache, DbConnection connection, bool autoCommit)
                    : base(new HardCodedEntityFactory(), GetOrBuildDbCompiledModel(metadataCache, connection), connection, autoCommit)
                {
                    this.m_Categories = new EfEntityRepository<Interfaces.Repository1.ICategory, EntityObject_Category>(this);
                    this.m_Products = new EfEntityRepository<Interfaces.Repository1.IProduct, EntityObject_Product>(this);
                    this.m_Orders = new EfEntityRepository<Interfaces.Repository1.IOrder, EntityObject_Order>(this);
                }

                public override sealed Type[] GetEntityTypesInRepository()
                {
                    return new Type[] {
                        typeof(EntityObject_Category),
                        typeof(EntityObject_Product),
                        typeof(EntityObject_Order),
                        typeof(EntityObject_OrderLine)
                    };
                }

                public override Type[] GetEntityContractsInRepository()
                {
                    return new Type[] {
                        typeof(Interfaces.Repository1.ICategory),
                        typeof(Interfaces.Repository1.IProduct),
                        typeof(Interfaces.Repository1.IOrder),
                        typeof(Interfaces.Repository1.IOrderLine)
                    };
                }

                public override IEntityRepository[] GetEntityRepositories()
                {
                    return new IEntityRepository[] { m_Categories, m_Orders, m_Products, null };
                }

                public Interfaces.Repository1.IOrderLine NewOrderLine(
                    Interfaces.Repository1.IOrder order,
                    Interfaces.Repository1.IProduct product,
                    int quantity)
                {
                    Interfaces.Repository1.IOrderLine orderLine = new EntityObject_OrderLine();
                    orderLine.Order = order;
                    orderLine.Product = product;
                    orderLine.Quantity = quantity;
                    return orderLine;
                }

                public IEntityRepository<Interfaces.Repository1.ICategory> Categories
                {
                    get { return this.m_Categories; }
                }

                public IEntityRepository<Interfaces.Repository1.IOrder> Orders
                {
                    get { return this.m_Orders; }
                }

                public IEntityRepository<Interfaces.Repository1.IProduct> Products
                {
                    get { return this.m_Products; }
                }

                private static readonly object _s_compiledModelSyncRoot = new object();
                private static DbCompiledModel _s_compiledModel;

                private static DbCompiledModel GetOrBuildDbCompiledModel(ITypeMetadataCache metadataCache, DbConnection connection)
                {
                    ITypeMetadata typeMetadata;
                    object typeConfiguration;

                    if ( _s_compiledModel == null )
                    {
                        lock ( _s_compiledModelSyncRoot )
                        {
                            if ( _s_compiledModel == null )
                            {
                                var modelBuilder = new DbModelBuilder();

                                modelBuilder.Conventions.Add(new NoUnderscoreForeignKeyNamingConvention());

                                // CATEGORY

                                typeMetadata = metadataCache.GetTypeMetadata(typeof(Interfaces.Repository1.ICategory));
                                typeConfiguration = EfModelApi.EntityType<EntityObject_Category>(modelBuilder, typeMetadata);

                                EfModelApi.ValueTypePrimitiveProperty<EntityObject_Category, int>(
                                    (EntityTypeConfiguration<EntityObject_Category>)typeConfiguration,
                                    typeMetadata.GetPropertyByName("Id"));
                                EfModelApi.StringProperty<EntityObject_Category>(
                                    (EntityTypeConfiguration<EntityObject_Category>)typeConfiguration,
                                    typeMetadata.GetPropertyByName("Name"));

                                // PRODUCT

                                typeMetadata = metadataCache.GetTypeMetadata(typeof(Interfaces.Repository1.IProduct));
                                typeConfiguration = EfModelApi.EntityType<EntityObject_Product>(modelBuilder, typeMetadata);

                                EfModelApi.ValueTypePrimitiveProperty<EntityObject_Product, int>(
                                    (EntityTypeConfiguration<EntityObject_Product>)typeConfiguration,
                                    typeMetadata.GetPropertyByName("Id"));
                                EfModelApi.StringProperty<EntityObject_Product>(
                                    (EntityTypeConfiguration<EntityObject_Product>)typeConfiguration,
                                    typeMetadata.GetPropertyByName("Name"));
                                EfModelApi.ValueTypePrimitiveProperty<EntityObject_Product, decimal>(
                                    (EntityTypeConfiguration<EntityObject_Product>)typeConfiguration,
                                    typeMetadata.GetPropertyByName("Price"));


                                // ORDER

                                typeMetadata = metadataCache.GetTypeMetadata(typeof(Interfaces.Repository1.IOrder));
                                typeConfiguration = EfModelApi.EntityType<EntityObject_Order>(modelBuilder, typeMetadata);

                                EfModelApi.ValueTypePrimitiveProperty<EntityObject_Order, int>(
                                    (EntityTypeConfiguration<EntityObject_Order>)typeConfiguration,
                                    typeMetadata.GetPropertyByName("Id"));
                                EfModelApi.ValueTypePrimitiveProperty<EntityObject_Order, DateTime>(
                                    (EntityTypeConfiguration<EntityObject_Order>)typeConfiguration,
                                    typeMetadata.GetPropertyByName("PlacedAt"));
                                EfModelApi.ValueTypePrimitiveProperty<EntityObject_Order, Interfaces.Repository1.OrderStatus>(
                                    (EntityTypeConfiguration<EntityObject_Order>)typeConfiguration,
                                    typeMetadata.GetPropertyByName("Status"));

                                // ORDER LINE

                                typeMetadata = metadataCache.GetTypeMetadata(typeof(Interfaces.Repository1.IOrderLine));
                                typeConfiguration = EfModelApi.EntityType<EntityObject_OrderLine>(modelBuilder, typeMetadata);

                                EfModelApi.ValueTypePrimitiveProperty<EntityObject_OrderLine, int>(
                                    (EntityTypeConfiguration<EntityObject_OrderLine>)typeConfiguration,
                                    typeMetadata.GetPropertyByName("Id"));
                                EfModelApi.ManyToOneRelationProperty<EntityObject_OrderLine, EntityObject_Order>(
                                    (EntityTypeConfiguration<EntityObject_OrderLine>)typeConfiguration,
                                    typeMetadata.GetPropertyByName("Order"));
                                EfModelApi.ManyToOneRelationProperty<EntityObject_OrderLine, EntityObject_Product>(
                                    (EntityTypeConfiguration<EntityObject_OrderLine>)typeConfiguration,
                                    typeMetadata.GetPropertyByName("Product"));
                                EfModelApi.ValueTypePrimitiveProperty<EntityObject_OrderLine, int>(
                                    (EntityTypeConfiguration<EntityObject_OrderLine>)typeConfiguration,
                                    typeMetadata.GetPropertyByName("Quantity"));

                                var model = modelBuilder.Build(connection);
                                _s_compiledModel = model.Compile();
                            }
                        }
                    }

                    return _s_compiledModel;
                }


                public IEntityRepository<Interfaces.Repository1.IAttribute> Attributes
                {
                    get { throw new NotImplementedException(); }
                }


                public Interfaces.Repository1.IAttributeValue NewAttributeValue(string value, int displayOrder)
                {
                    throw new NotImplementedException();
                }


                public Interfaces.Repository1.IAttributeValueChoice NewAttributeValueChoice(Interfaces.Repository1.IAttribute attribute, string value)
                {
                    throw new NotImplementedException();
                }

                Interfaces.Repository1.IOrderLine Interfaces.Repository1.IOnlineStoreRepository.NewOrderLine(Interfaces.Repository1.IOrder order, Interfaces.Repository1.IProduct product, int quantity)
                {
                    throw new NotImplementedException();
                }

                Interfaces.Repository1.IAttributeValue Interfaces.Repository1.IOnlineStoreRepository.NewAttributeValue(string value, int displayOrder)
                {
                    throw new NotImplementedException();
                }

                Interfaces.Repository1.IAttributeValueChoice Interfaces.Repository1.IOnlineStoreRepository.NewAttributeValueChoice(Interfaces.Repository1.IAttribute attribute, string value)
                {
                    throw new NotImplementedException();
                }

                IEntityRepository<Interfaces.Repository1.ICategory> Interfaces.Repository1.IOnlineStoreRepository.Categories
                {
                    get { throw new NotImplementedException(); }
                }

                IEntityRepository<Interfaces.Repository1.IProduct> Interfaces.Repository1.IOnlineStoreRepository.Products
                {
                    get { throw new NotImplementedException(); }
                }

                IEntityRepository<Interfaces.Repository1.IOrder> Interfaces.Repository1.IOnlineStoreRepository.Orders
                {
                    get { throw new NotImplementedException(); }
                }

                IEntityRepository<Interfaces.Repository1.IOrderLine> Interfaces.Repository1.IOnlineStoreRepository.OrdersLines
                {
                    get { throw new NotImplementedException(); }
                }

                IEntityRepository<Interfaces.Repository1.IAttribute> Interfaces.Repository1.IOnlineStoreRepository.Attributes
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

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public class HardCodedEntityFactory : IEntityObjectFactory
            {
                public TEntityContract NewEntity<TEntityContract>() where TEntityContract : class
                {
                    if ( typeof(TEntityContract) == typeof(Interfaces.Repository1.IOrder) )
                    {
                        return (TEntityContract)(object)new EntityObject_Order();
                    }
                    if ( typeof(TEntityContract) == typeof(Interfaces.Repository1.IOrderLine) )
                    {
                        return (TEntityContract)(object)new EntityObject_OrderLine();
                    }
                    if ( typeof(TEntityContract) == typeof(Interfaces.Repository1.IProduct) )
                    {
                        return (TEntityContract)(object)new EntityObject_Product();
                    }
                    if ( typeof(TEntityContract) == typeof(Interfaces.Repository1.ICategory) )
                    {
                        return (TEntityContract)(object)new EntityObject_Category();
                    }
                    if ( typeof(TEntityContract) == typeof(Interfaces.Repository1.IAttribute) )
                    {
                        return (TEntityContract)(object)new EntityObject_Attribute();
                    }
                    if ( typeof(TEntityContract) == typeof(Interfaces.Repository1.IAttributeValue) )
                    {
                        return (TEntityContract)(object)new EntityObject_AttributeValue();
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

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public class EntityObject_Order : Interfaces.Repository1.IOrder, IEntityPartId<int>
            {
                private int m_Id;
                private string m_OrderNo;
                private ICollection<EntityObject_OrderLine> m_OrderLines = new HashSet<EntityObject_OrderLine>();
                private EntityObjectFactory.CollectionAdapter<EntityObject_OrderLine, Interfaces.Repository1.IOrderLine> m_OrderLines_Adapter;
                private DateTime m_PlacedAt;
                private Interfaces.Repository1.OrderStatus m_Status;

                public EntityObject_Order()
                {
                    this.m_OrderLines_Adapter =
                        new EntityObjectFactory.CollectionAdapter<EntityObject_OrderLine, Interfaces.Repository1.IOrderLine>(this.m_OrderLines);
                    this.m_Status = Interfaces.Repository1.OrderStatus.New;
                    this.m_Id = _s_nextIntId++;
                }

                public int Id
                {
                    get { return this.m_Id; }
                    set { this.m_Id = value; }
                }

                public string OrderNo
                {
                    get { return this.m_OrderNo; }
                    set { this.m_OrderNo = value; }
                }

                ICollection<Interfaces.Repository1.IOrderLine> Interfaces.Repository1.IOrder.OrderLines
                {
                    get { return this.m_OrderLines_Adapter; }
                }

                public virtual ICollection<EntityObject_OrderLine> OrderLines
                {
                    get { return this.m_OrderLines; }
                    set
                    {
                        this.m_OrderLines_Adapter = new EntityObjectFactory.CollectionAdapter<EntityObject_OrderLine, Interfaces.Repository1.IOrderLine>(value);
                        this.m_OrderLines = value;
                    }
                }

                public DateTime PlacedAt
                {
                    get { return this.m_PlacedAt; }
                    set { this.m_PlacedAt = value; }
                }

                public Interfaces.Repository1.OrderStatus Status
                {
                    get { return m_Status; }
                    set { m_Status = value; }
                }
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public class EntityObject_OrderLine : Interfaces.Repository1.IOrderLine, IEntityPartId<int>
            {
                private int m_Id;
                private EntityObject_Order m_Order;
                private EntityObject_Product m_Product;
                private int m_Quantity;

                private ICollection<Interfaces.Repository1.IAttributeValueChoice> m_Attributes_ContractValue;
                private string m_Attributes_StorageValue;
                private DualValueStates m_Attributes_ValueState;

                public EntityObject_OrderLine()
                {
                    this.m_Attributes_ContractValue = new List<Interfaces.Repository1.IAttributeValueChoice>();
                    this.m_Attributes_ValueState = DualValueStates.Contract;
                }

                public EntityObject_OrderLine(IComponentContext arg0)
                {
                    this.m_Attributes_ContractValue = new List<Interfaces.Repository1.IAttributeValueChoice>();
                    this.m_Attributes_ValueState = DualValueStates.Contract;
                }

                public int Id
                {
                    get { return m_Id; }
                    set { m_Id = value; }
                }

                Interfaces.Repository1.IOrder Interfaces.Repository1.IOrderLine.Order
                {
                    get { return this.m_Order; }
                    set { this.m_Order = (EntityObject_Order)value; }
                }

                Interfaces.Repository1.IProduct Interfaces.Repository1.IOrderLine.Product
                {
                    get { return this.m_Product; }
                    set { this.m_Product = (EntityObject_Product)value; }
                }

                [Column(name: "OrderId")]
                public virtual EntityObject_Order Order
                {
                    get { return this.m_Order; }
                    set { this.m_Order = value; }
                }

                [Column(name: "ProductId")]
                public virtual EntityObject_Product Product
                {
                    get { return this.m_Product; }
                    set { this.m_Product = value; }
                }

                public int Quantity
                {
                    get { return this.m_Quantity; }
                    set { this.m_Quantity = value; }
                }

                ICollection<Interfaces.Repository1.IAttributeValueChoice> Interfaces.Repository1.IOrderLine.Attributes
                {
                    get
                    {
                        if (this.m_Attributes_ValueState == DualValueStates.Storage)
                        {
                            this.m_Attributes_ContractValue = JsonConvert
                                .DeserializeObject<List<EntityObject_AttributeValueChoice>>(this.m_Attributes_StorageValue)
                                .Cast<Interfaces.Repository1.IAttributeValueChoice>()
                                .ToList();
                            this.m_Attributes_ValueState |= DualValueStates.Contract;
                        }
                        return this.m_Attributes_ContractValue;
                    }
                    set
                    {
                        this.m_Attributes_ContractValue = value;
                        this.m_Attributes_ValueState = DualValueStates.Contract;
                    }
                }

                public virtual string Attributes
                {
                    get
                    {
                        if (this.m_Attributes_ValueState == DualValueStates.Contract)
                        {
                            this.m_Attributes_StorageValue = JsonConvert.SerializeObject(this.m_Attributes_ContractValue.Cast<EntityObject_AttributeValueChoice>().ToList());
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
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public class EntityObject_Product : Interfaces.Repository1.IProduct, IEntityPartId<int>
            {
                private int m_Id;
                private string m_CatalogNo;
                private string m_Name;
                private decimal m_Price;
                
                private ICollection<EntityObject_Category> m_Categories = new HashSet<EntityObject_Category>();
                private EntityObjectFactory.CollectionAdapter<EntityObject_Category, Interfaces.Repository1.ICategory> m_Categories_Adapter;
                
                private ICollection<EntityObject_Attribute> m_Attributes = new HashSet<EntityObject_Attribute>();
                private EntityObjectFactory.CollectionAdapter<EntityObject_Attribute, Interfaces.Repository1.IAttribute> m_Attributes_Adapter;

                public EntityObject_Product()
                {
                    this.m_Categories_Adapter =
                        new EntityObjectFactory.CollectionAdapter<EntityObject_Category, Interfaces.Repository1.ICategory>(this.m_Categories);
                    this.m_Attributes_Adapter =
                        new EntityObjectFactory.CollectionAdapter<EntityObject_Attribute, Interfaces.Repository1.IAttribute>(this.m_Attributes);
                    this.m_Id = _s_nextIntId++;
                }

                public int Id
                {
                    get { return this.m_Id; }
                    set { this.m_Id = value; }
                }

                public string CatalogNo
                {
                    get { return this.m_CatalogNo; }
                    set { this.m_CatalogNo = value; }
                }

                public string Name
                {
                    get { return this.m_Name; }
                    set { this.m_Name = value; }
                }

                public decimal Price
                {
                    get { return this.m_Price; }
                    set { this.m_Price = value; }
                }

                ICollection<Interfaces.Repository1.ICategory> Interfaces.Repository1.IProduct.Categories
                {
                    get { return this.m_Categories_Adapter; }
                }

                public virtual ICollection<EntityObject_Category> Categories
                {
                    get { return this.m_Categories; }
                    set
                    {
                        this.m_Categories_Adapter = new EntityObjectFactory.CollectionAdapter<EntityObject_Category, Interfaces.Repository1.ICategory>(value);
                        this.m_Categories = value;
                    }
                }

                ICollection<Interfaces.Repository1.IAttribute> Interfaces.Repository1.IProduct.Attributes
                {
                    get { return this.m_Attributes_Adapter; }
                }

                public virtual ICollection<EntityObject_Attribute> Attributes
                {
                    get { return this.m_Attributes; }
                    set
                    {
                        this.m_Attributes_Adapter = new EntityObjectFactory.CollectionAdapter<EntityObject_Attribute, Interfaces.Repository1.IAttribute>(value);
                        this.m_Attributes = value;
                    }
                }
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public class EntityObject_Category : Interfaces.Repository1.ICategory, IEntityPartId<int>
            {
                private int m_Id;
                private string m_Name;

                public EntityObject_Category()
                {
                    this.m_Id = _s_nextIntId++;
                }

                public int Id
                {
                    get { return this.m_Id; }
                    set { this.m_Id = value; }
                }

                public string Name
                {
                    get { return this.m_Name; }
                    set { this.m_Name = value; }
                }

                public virtual ICollection<EntityObject_Product> Products { get; set; }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public class EntityObject_Attribute : Interfaces.Repository1.IAttribute, IEntityObject, IEntityPartId<int>
            {
                private int m_Id;
                private string m_Name;
                private string m_TitleForUser;
                private IList<Interfaces.Repository1.IAttributeValue> m_Values_ContractValue;
                private string m_Values_StorageValue;
                private DualValueStates m_Values_ValueState;

                public EntityObject_Attribute()
                {
                    this.m_Values_ContractValue = new List<Interfaces.Repository1.IAttributeValue>();
                    this.m_Values_ValueState = DualValueStates.Contract;
                }

                public EntityObject_Attribute(IComponentContext arg0)
                {
                    this.m_Values_ContractValue = new List<Interfaces.Repository1.IAttributeValue>();
                    this.m_Values_ValueState = DualValueStates.Contract;
                }

                IEntityId IEntityObject.GetId()
                {
                    return new EntityId<Interfaces.Repository1.IAttribute, int>(this.Id);
                }

                void IEntityObject.SetId(object value)
                {
                    this.m_Id = (int)value;
                }

                IList<Interfaces.Repository1.IAttributeValue> Interfaces.Repository1.IAttribute.Values
                {
                    get
                    {
                        if ( this.m_Values_ValueState == DualValueStates.Storage )
                        {
                            this.m_Values_ContractValue = JsonConvert
                                .DeserializeObject<List<EntityObject_AttributeValue>>(this.m_Values_StorageValue)
                                .Cast<Interfaces.Repository1.IAttributeValue>()
                                .ToList();
                            this.m_Values_ValueState |= DualValueStates.Contract;
                        }
                        return this.m_Values_ContractValue;
                    }
                    set
                    {
                        this.m_Values_ContractValue = value;
                        this.m_Values_ValueState = DualValueStates.Contract;
                    }
                }

                public int Id
                {
                    get { return this.m_Id; }
                    set { this.m_Id = value; }
                }

                Type IEntityObject.ContractType
                {
                    get { return typeof(Interfaces.Repository1.IAttribute); }
                }

                public string Name
                {
                    get { return this.m_Name; }
                    set { this.m_Name = value; }
                }

                public string TitleForUser
                {
                    get { return this.m_TitleForUser; }
                    set { this.m_TitleForUser = value; }
                }

                public virtual string Values
                {
                    get
                    {
                        if ( this.m_Values_ValueState == DualValueStates.Contract )
                        {
                            this.m_Values_StorageValue = JsonConvert.SerializeObject(this.m_Values_ContractValue.Cast<EntityObject_AttributeValue>().ToList());
                            this.m_Values_ValueState |= DualValueStates.Storage;
                        }
                        return this.m_Values_StorageValue;
                    }
                    set
                    {
                        this.m_Values_StorageValue = value;
                        this.m_Values_ValueState = DualValueStates.Storage;
                    }
                }

                public virtual ICollection<EntityObject_Product> Products { get; set; }
            }
            public class EntityObject_AttributeValue : Interfaces.Repository1.IAttributeValue
            {
                public virtual int DisplayOrder { get; set; }
                public virtual string Value { get; set; }
            }
            public class EntityObject_AttributeValueChoice : Interfaces.Repository1.IAttributeValueChoice
            {
                private Interfaces.Repository1.IAttribute m_Attribute_ContractValue;
                private int m_Attribute_StorageValue;
                private DualValueStates m_Attribute_ValueState;
                private IFramework _dependency_Framework;

                public EntityObject_AttributeValueChoice()
                {
                    _dependency_Framework = null;
                }
                public EntityObject_AttributeValueChoice(IComponentContext components)
                {
                    _dependency_Framework = components.Resolve<IFramework>();
                }

                Interfaces.Repository1.IAttribute Interfaces.Repository1.IAttributeValueChoice.Attribute
                {
                    get
                    {
                        if (this.m_Attribute_ValueState == DualValueStates.Storage)
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

                public virtual int Attribute
                {
                    get
                    {
                        if (this.m_Attribute_ValueState == DualValueStates.Contract)
                        {
                            this.m_Attribute_StorageValue = ((IEntityPartId<int>)this.m_Attribute_ContractValue).Id;
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

                public string Value { get; set; }

                private Interfaces.Repository1.IAttribute LazyLoad_Attribute(int id)
                {
                    using ( var repo = _dependency_Framework.NewUnitOfWork<Interfaces.Repository1.IOnlineStoreRepository>() )
                    {
                        return repo.Attributes.Where(a => ((IEntityPartId<int>)a).Id == id).Single();
                    }
                }
            }
        }
    }
}
