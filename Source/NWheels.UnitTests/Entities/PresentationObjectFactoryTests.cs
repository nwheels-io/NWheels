using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Autofac.Core;
using Newtonsoft.Json;
using NUnit.Framework;
using NWheels.Conventions.Core;
using NWheels.DataObjects;
using NWheels.DataObjects.Core;
using NWheels.DataObjects.Core.Conventions;
using NWheels.Entities;
using NWheels.Entities.Core;
using NWheels.Entities.Factories;
using NWheels.Exceptions;
using NWheels.Extensions;
using NWheels.Logging;
using NWheels.Processing.Messages;
using NWheels.Testing;
using NWheels.TypeModel.Core;
using Shouldly;

namespace NWheels.UnitTests.Entities
{
    [TestFixture]
    public class PresentationObjectFactoryTests : DynamicTypeUnitTestBase
    {
        private DomainObjectFactory _domainObjetFactory;
        private HardCodedEntityObject_ContractEntity _persistableObject;
        private IContractEntity _domainObject;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [SetUp]
        public void SetUp()
        {
            Framework.UpdateComponents(
                builder =>
                {
                    builder.NWheelsFeatures().ObjectContracts().Concretize<IContractEntity>().With<ContractEntity>();
                    builder.NWheelsFeatures().Logging().RegisterLogger<IContractEntityLogger>();
                });
            Framework.RebuildMetadataCache();
            Framework.MetadataCache.GetTypeMetadata(typeof(IContractEntity)).As<TypeMetadataBuilder>().UpdateImplementation(
                typeof(HardCodedEntityObjectFactory),
                typeof(HardCodedEntityObject_ContractEntity));

            _domainObjetFactory = new DomainObjectFactory(Framework.Components, base.DyamicModule, Framework.MetadataCache);
            _persistableObject = new HardCodedEntityObject_ContractEntity(Framework.Components);
            _domainObject = _domainObjetFactory.CreateDomainObjectInstance<IContractEntity>(_persistableObject);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanCreatePresentationObject()
        {
            //- arrange

            var factoryUnderTest = new PresentationObjectFactory(Framework.Components, base.DyamicModule, Framework.MetadataCache);

            //- act

            var presentationObject = factoryUnderTest.CreatePresentationObjectInstance(_domainObject);

            //- assert

            presentationObject.ShouldNotBe(null);
            presentationObject.ShouldBeAssignableTo<IContractEntity>();
            presentationObject.ShouldBeAssignableTo<IPresentationObject>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanGetDomainObjectFromPresentationObject()
        {
            //- arrange

            var factoryUnderTest = new PresentationObjectFactory(Framework.Components, base.DyamicModule, Framework.MetadataCache);

            //- act

            var presentationObject = factoryUnderTest.CreatePresentationObjectInstance(_domainObject);
            var domainObjectFromPresentationObject = presentationObject.As<IPresentationObject>().GetDomainObject();

            //- assert

            domainObjectFromPresentationObject.ShouldBeSameAs(_domainObject);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanSerializePresentationObjectToJson()
        {
            //- arrange

            _persistableObject.WritePropertyValue_Expiration(new DateTime(2015, 5, 30));
            _persistableObject.WritePropertyValue_IsApproved(true);
            _persistableObject.Term = ContractTermType.Monthly;

            var factoryUnderTest = new PresentationObjectFactory(Framework.Components, base.DyamicModule, Framework.MetadataCache);

            //- act

            var presentationObject = factoryUnderTest.CreatePresentationObjectInstance(_domainObject);
            var json = JsonConvert.SerializeObject(presentationObject);

            //- assert

            Console.WriteLine(json);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------


#if false
        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanInvokeDomainObjectOperation()
        {
            //- arrange

            Framework.UpdateComponents(
                builder => {
                    builder.NWheelsFeatures().ObjectContracts().Concretize<IContractEntity>().With<ContractEntity>();
                    builder.NWheelsFeatures().Logging().RegisterLogger<IContractEntityLogger>();
                }
            );
            Framework.RebuildMetadataCache();
            Framework.MetadataCache.GetTypeMetadata(typeof(IContractEntity)).As<TypeMetadataBuilder>().UpdateImplementation(
                typeof(HardCodedEntityObjectFactory),
                typeof(HardCodedEntityObject_ContractEntity));

            var factoryUnderTest = new DomainObjectFactory(Framework.Components, base.DyamicModule, Framework.MetadataCache);
            var entityObject = new HardCodedEntityObject_ContractEntity(Framework.Components);

            entityObject.WritePropertyValue_Expiration(new DateTime(2015, 5, 30));
            entityObject.WritePropertyValue_IsApproved(true);
            entityObject.Term = ContractTermType.Monthly;

            IContractEntity contract = factoryUnderTest.CreateDomainObjectInstance<IContractEntity>(entityObject);

            //- act

            contract.Renew();

            //- assert

            // 2015-05-30 -> renew -> 2015-06-30
            Assert.That(contract.Expiration, Is.EqualTo(new DateTime(2015, 6, 30)));
            Assert.That(entityObject.Expiration, Is.EqualTo(new DateTime(2015, 6, 30)));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanInvokeWiredMethodsOnCommitOfNewEntity()
        {
            //- arrange

            Framework.UpdateComponents(
                builder => {
                    builder.NWheelsFeatures().ObjectContracts().Concretize<IContractEntity>().With<ContractEntity>();
                    builder.NWheelsFeatures().Logging().RegisterLogger<IContractEntityLogger>();
                }
            );
            Framework.RebuildMetadataCache();
            Framework.MetadataCache.GetTypeMetadata(typeof(IContractEntity)).As<TypeMetadataBuilder>().UpdateImplementation(
                typeof(HardCodedEntityObjectFactory),
                typeof(HardCodedEntityObject_ContractEntity));

            var factoryUnderTest = new DomainObjectFactory(Framework.Components, base.DyamicModule, Framework.MetadataCache);
            var persistableObject = new HardCodedEntityObject_ContractEntity(Framework.Components);

            IContractEntity contract = factoryUnderTest.CreateDomainObjectInstance<IContractEntity>(persistableObject);

            //- act

            contract.Term = ContractTermType.Yearly;;
            contract.As<ContractEntity>().Approve();
            
            contract.As<IDomainObject>().BeforeCommit();
            var log1 = Framework.TakeLog();

            contract.As<IDomainObject>().AfterCommit();
            var log2 = Framework.TakeLog();

            //- assert

            LogAssert.That(log1).HasOne<IContractEntityLogger>(x => x.BeforeSave(EntityState.NewModified, true, true, true));
            LogAssert.That(log2).HasOne<IContractEntityLogger>(x => x.AfterSave(EntityState.NewModified, true, true, true));
        }

#endif

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Application Code - coded by application developer

        /// <summary>
        /// Type of contract term (yearly/monthly)
        /// </summary>
        public enum ContractTermType
        {
            /// <summary>
            /// Monthly term
            /// </summary>
            Monthly,
            /// <summary>
            /// Yearly term
            /// </summary>
            Yearly
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface IContractEntityLogger : IApplicationEventLogger
        {
            [LogInfo]
            void BeforeSave(EntityState entityState, bool termWasModified, bool expirationWasModified, bool isApprovedWasModified);

            [LogInfo]
            void AfterSave(EntityState entityState, bool termWasModified, bool expirationWasModified, bool isApprovedWasModified);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Represents a contract with customer.
        /// This interface is exposed to UI layer, external client applications, and B2B intergations
        /// </summary>
        /// <remarks>
        /// Encapsulates both entity data and operations -- because this is what OOP truly is.
        /// </remarks>
        public interface IContractEntity
        {
            /// <summary>
            /// Expands contract expiration 1 time the term.
            /// </summary>
            /// <remarks>
            /// Since this method is part of entity interface, it can be invoked by a request received from outside of our system 
            /// (e.g., from web browser or through a B2B integration).
            /// </remarks>
            void Renew();

            /// <summary>
            /// Gets or sets type of contract term (monthly/yearly)
            /// </summary>
            ContractTermType Term { get; set; }

            /// <summary>
            /// Expiration of the contract according to current term.
            /// </summary>
            /// <remarks>
            /// The value of this property can only be set by the business logic.
            /// </remarks>
            DateTime Expiration { get; }

            /// <summary>
            /// Whether this contract has been approved. If not, the contract is not effective.
            /// </summary>
            /// <remarks>
            /// The value of this property can only be set by the business logic.
            /// </remarks>
            bool IsApproved { get; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// This class is abstract because we abstract from the other layers, e.g. communication, presentation, and persistence.
        /// </summary>
        /// <remarks>
        /// The persistence layer will generate concrete implementations of persistable properties, and properly handle data mechanics required from every 
        /// entity object implementation.
        /// 
        /// The most important, this class allows us encapsulate our business logic and the data together -- which is what OOP truly is.
        /// 
        /// For high throughput systems, the best options is to have at most one instance of every entity object per request, as opposed to having
        /// object per layer, which means copying data between objects when crossing the layers. The latter means added latency and heavier duty for GC, 
        /// which usually results in waveform graph of system throughput, as opposed to straight line when GC is minimized.
        /// </remarks>
        public abstract class ContractEntity : IContractEntity
        {
            #region Implementation of IContractEntity - concrete class will be generated on the fly.

            // All we need here is write abstract property declarations. 
            // The underlying infrastructure will implement proper DTO/persistence mechanics under the hood.

            public abstract ContractTermType Term { get; set; }
            public abstract DateTime Expiration { get; protected set; }
            public abstract bool IsApproved { get; protected set; }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            // this method is part of the entity interface - it can be invoked by request received from client applications or through B2B intergation
            public void Renew()
            {
                // since this operation can be requested from outside of our system, proper validation is mandatory
                if (!IsApproved)
                {
                    throw new BusinessRuleException("Contract cannot be renewed because it is not approved.");
                }

                // request is valid, let's do the change
                switch (Term)
                {
                    case ContractTermType.Monthly:
                        Expiration = Expiration.AddMonths(1);
                        break;
                    case ContractTermType.Yearly:
                        Expiration = Expiration.AddYears(1);
                        break;
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            // this method is not part of IContractEntity interface - it is only known within our business logic layer
            public void Approve()
            {
                IsApproved = true;
                Expiration = Framework.UtcNow;
                Renew();
            }

            #endregion

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            #region Dependency Properties

            protected IFramework Framework { get; set; }
            protected IContractEntityLogger Logger { get; set; }

            #endregion

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            #region Auto-Wired Members (by naming convention)

            /// <summary>
            /// By convention, this property will return current state of the entity
            /// </summary>
            protected abstract EntityState EntityState { get; }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            /// <summary>
            /// By convention, this property will return whether the value of Term property was modified
            /// </summary>
            protected abstract bool TermWasModified { get; }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            /// <summary>
            /// By convention, this property will return whether the value of Expiration property was modified
            /// </summary>
            protected abstract bool ExpirationWasModified { get; }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            /// <summary>
            /// By convention, this property will return whether the value of IsApproved property was modified
            /// </summary>
            protected abstract bool IsApprovedWasModified { get; }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            /// <summary>
            /// By convention, this method will be called just prior to committing create or update of the entity
            /// </summary>
            protected virtual void EntityTriggerBeforeSave()
            {
                Logger.BeforeSave(EntityState, TermWasModified, ExpirationWasModified, IsApprovedWasModified);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            /// <summary>
            /// By convention, this method will be called just prior to committing create or update of the entity
            /// </summary>
            protected virtual void EntityTriggerAfterSave()
            {
                Logger.AfterSave(EntityState, TermWasModified, ExpirationWasModified, IsApprovedWasModified);
            }

            #endregion
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Generated Code - dynamic classes emitted on the fly, no human codes it
        // ReSharper disable InconsistentNaming

        public class HardCodedDomainObject_ContractEntity : ContractEntity, IContain<IEntityObject>
        {
            private IContractEntity _underlyingPersistableObject;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public HardCodedDomainObject_ContractEntity(IContractEntity underlyingPersistableObject, IComponentContext components)
            {
                _underlyingPersistableObject = underlyingPersistableObject;
                this.Framework = components.Resolve<IFramework>();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            IEntityObject IContain<IEntityObject>.GetContainedObject()
            {
                return (IEntityObject)_underlyingPersistableObject;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override ContractTermType Term
            {
                get { return _underlyingPersistableObject.Term; }
                set { _underlyingPersistableObject.Term = value; }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override DateTime Expiration
            {
                get
                {
                    return _underlyingPersistableObject.Expiration;
                }
                protected set
                {
                    ((HardCodedEntityObject_ContractEntity)_underlyingPersistableObject).WritePropertyValue_Expiration(value);
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override bool IsApproved
            {
                get
                {
                    return _underlyingPersistableObject.IsApproved;
                }
                protected set
                {
                    ((HardCodedEntityObject_ContractEntity)_underlyingPersistableObject).WritePropertyValue_IsApproved(value);
                }
            }

            protected override EntityState EntityState
            {
                get { throw new NotImplementedException(); }
            }

            protected override bool TermWasModified
            {
                get { throw new NotImplementedException(); }
            }

            protected override bool ExpirationWasModified
            {
                get { throw new NotImplementedException(); }
            }

            protected override bool IsApprovedWasModified
            {
                get { throw new NotImplementedException(); }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        // this class is basically implementation of entity object;
        // while the inherited members of the business logic are here, they are ignored by the persistence.
        public class HardCodedEntityObject_ContractEntity : IContractEntity, IHaveDependencies, IEntityObject
        {
            private Guid _id;
            private DateTime _expiration;
            private bool _isApproved;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private IComponentContext _components;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public HardCodedEntityObject_ContractEntity()
            {
                this.State = EntityState.NewPristine;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public HardCodedEntityObject_ContractEntity(IComponentContext components)
            {
                ((IHaveDependencies)this).InjectDependencies(components);
                _components = components;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            #region Implementation of IContractEntity

            void IContractEntity.Renew()
            {
                throw new NotSupportedException("No domain object provided which is capanle of handling requested operation.");
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ContractTermType Term { get; set; }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            DateTime IContractEntity.Expiration
            {
                get { return _expiration; }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            bool IContractEntity.IsApproved
            {
                get { return _isApproved; }
            }

            #endregion

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            void IHaveDependencies.InjectDependencies(IComponentContext components)
            {
                _components = components;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public DateTime Expiration
            {
                get { return _expiration; }
                set { _expiration = value; }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public bool IsApproved
            {
                get { return _isApproved; }
                set { _isApproved = value; }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public Guid Id
            {
                get { return _id; }
                set { _id = value; }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void WritePropertyValue_Expiration(DateTime value)
            {
                _expiration = value;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void WritePropertyValue_IsApproved(bool value)
            {
                _isApproved = value;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            IEntityId IEntityObject.GetId()
            {
                return new EntityId<IContractEntity, Guid>(_id);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            void IEntityObject.SetId(object value)
            {
                _id = (Guid)value;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            Type IObject.ContractType
            {
                get { return typeof(IContractEntity); }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            Type IObject.FactoryType
            {
                get { return typeof(HardCodedEntityObjectFactory); }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public bool IsModified
            {
                get { throw new NotImplementedException(); }
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

            public void EnsureDomainObject()
            {
                if (_domainObject == null)
                {
                    //RuntimeEntityModelHelpers.EnsureContainerDomainObject<IContractEntity>(this, _components);
                }
            }

            #endregion

            #region Implementation of IEntityObjectBase

            public EntityState State { get; set; }

            #endregion

            #region Implementation of IPersistableObject

            public object[] ExportValues()
            {
                throw new NotImplementedException();
            }

            public void ImportValues(object[] values)
            {
                throw new NotImplementedException();
            }

            #endregion

            #region Implementation of IPersistableObject

            public object[] ExportValues(IEntityRepository entityRepo)
            {
                throw new NotImplementedException();
            }

            public void ImportValues(IEntityRepository entityRepo, object[] values)
            {
                throw new NotImplementedException();
            }

            #endregion
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class HardCodedDomainObjectFactory : IDomainObjectFactory
        {
            private readonly IComponentContext _components;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public HardCodedDomainObjectFactory(IComponentContext components)
            {
                _components = components;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            #region Implementation of IDomainObjectFactory

            public Type GetOrBuildDomainObjectType(Type contractType)
            {
                throw new NotImplementedException();
            }

            public Type GetOrBuildDomainObjectType(Type contractType, Type persistableFactoryType)
            {
                return typeof(HardCodedDomainObject_ContractEntity);
            }

            public TEntityContract CreateDomainObjectInstance<TEntityContract>()
            {
                throw new NotImplementedException();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public TEntityContract CreateDomainObjectInstance<TEntityContract>(TEntityContract underlyingPersistableObject)
            {
                return (TEntityContract)(object)new HardCodedDomainObject_ContractEntity((IContractEntity)(object)underlyingPersistableObject, _components);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public IDomainObject CreateDomainObjectInstance(IPersistableObject underlyingPersistableObject)
            {
                return (IDomainObject)(object)new HardCodedDomainObject_ContractEntity((IContractEntity)(object)underlyingPersistableObject, _components);
            }

            #endregion
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class HardCodedEntityObjectFactory : IEntityObjectFactory
        {
            public TEntityContract NewEntity<TEntityContract>() where TEntityContract : class
            {
                throw new NotImplementedException();
            }

            public TEntityContract NewEntity<TEntityContract>(IComponentContext externalComponents) where TEntityContract : class
            {
                throw new NotImplementedException();
            }

            public object NewEntity(Type entityContractType)
            {
                throw new NotImplementedException();
            }
        }

        // ReSharper restore InconsistentNaming
        #endregion
    }
}
