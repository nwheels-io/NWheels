#if false

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using NUnit.Framework;
using NWheels.DataObjects;
using NWheels.DataObjects.Core;
using NWheels.DataObjects.Core.Conventions;
using NWheels.Entities.Core;
using NWheels.Entities.Factories;
using NWheels.Testing;
using NWheels.Testing.Entities.Stacks;
using IR1 = NWheels.Testing.Entities.Stacks.Interfaces.Repository1;

namespace NWheels.UnitTests.Entities
{
    [TestFixture]
    public class DomainObjectFactoryTests : DynamicTypeUnitTestBase
    {
        [Test]
        public void CanCreateDomainObject()
        {
            //-- arrange

            var factoryUnderTest = CreateDomainObjectFactory();
            
            Func<IR1.ICustomer, IR1.ICustomer> factoryMethod = e => {
                return factoryUnderTest.CreateInstanceOf<IR1.ICustomer>().UsingConstructor<IR1.ICustomer>(e);
            };

            //-- act

            var entity = new TestCustomerEntity(factoryMethod);

            //-- assert

            Assert.That(entity.DomainObject, Is.Not.Null);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private DomainObjectConvention CreateDomainObjectFactory()
        {
            var metadataCache = TestFramework.CreateMetadataCacheWithDefaultConventions(new IMetadataConvention[] {
                new DefaultIdMetadataConvention(typeof(int))
            });

            var updater = new ContainerBuilder();
            updater.RegisterInstance(metadataCache).As<ITypeMetadataCache, TypeMetadataCache>();
            updater.Update(Framework.Components.ComponentRegistry);

            metadataCache.GetTypeMetadata(typeof(IR1.IAttribute));
            metadataCache.GetTypeMetadata(typeof(IR1.ICategory));
            metadataCache.GetTypeMetadata(typeof(IR1.IProduct));
            metadataCache.GetTypeMetadata(typeof(IR1.IOrder));
            metadataCache.GetTypeMetadata(typeof(IR1.IOrderLine));
            metadataCache.GetTypeMetadata(typeof(IR1.ICustomer));
            metadataCache.GetTypeMetadata(typeof(IR1.IContactDetail));
            metadataCache.GetTypeMetadata(typeof(IR1.IEmailContactDetail));
            metadataCache.GetTypeMetadata(typeof(IR1.IPhoneContactDetail));
            metadataCache.GetTypeMetadata(typeof(IR1.IPostContactDetail));

            metadataCache.AcceptVisitor(new CrossTypeFixupMetadataVisitor(metadataCache));

            var factory = new DomainObjectConvention(base.DyamicModule);
            return factory;
        }

        public interface ILicenseContract
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class TestCustomerEntity : IR1.ICustomer
        {
            private readonly IR1.ICustomer _domainObject;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public TestCustomerEntity(Func<IR1.ICustomer, IR1.ICustomer> domainObjectFactory)
            {
                _domainObject = domainObjectFactory(this);
                ContactDetails = new List<IR1.IContactDetail>();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public bool QualifiesAsValuableCustomer()
            {
                return _domainObject.QualifiesAsValuableCustomer();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public bool IsInteredtedIn(IR1.IProduct product)
            {
                return _domainObject.IsInteredtedIn(product);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public string FullName { get; set; }
            public ICollection<IR1.IContactDetail> ContactDetails { get; private set; }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public IR1.ICustomer DomainObject
            {
                get { return _domainObject; }
            }
        }
    }
}


#endif