using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using NWheels.Core.DataObjects;
using NWheels.Core.DataObjects.Conventions;
using NWheels.Core.Entities;
using NWheels.DataObjects;
using NWheels.Entities;
using NWheels.Testing;

namespace NWheels.Core.UnitTests.DataObjects
{
    [TestFixture]
    public class TypeMetadataBuilderConstructorTests : UnitTestBase
    {
        [Test]
        public void CanDetermineDataObjectContract()
        {
            Assert.That(
                DataObjectContractAttribute.IsDataObjectContract(typeof(TestDataObjects.Repository1.IProduct)),
                Is.True);

            Assert.That(
                DataObjectContractAttribute.IsDataObjectContract(typeof(TestDataObjects.Repository1.OrderStatus)),
                Is.False);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CreateScalarProperties()
        {
            //-- Arrange & Act

            var productMetadata = ConstructMetadataOf<TestDataObjects.Repository1.IProduct>();

            //-- Assert

            Assert.That(productMetadata.Properties.Count, Is.EqualTo(4));

            Assert.That(productMetadata.GetPropertyByName("Id").ClrType, Is.EqualTo(typeof(int)));
            Assert.That(productMetadata.GetPropertyByName("Name").ClrType, Is.EqualTo(typeof(string)));
            Assert.That(productMetadata.GetPropertyByName("Price").ClrType, Is.EqualTo(typeof(decimal)));
            Assert.That(productMetadata.GetPropertyByName("Description").ClrType, Is.EqualTo(typeof(string)));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void NonNullableValueTypesAreAlwaysRequired()
        {
            //-- Arrange & Act

            var productMetadata = ConstructMetadataOf<TestDataObjects.Repository1.IProduct>();

            //-- Assert

            Assert.That(productMetadata.GetPropertyByName("Id").Validation.IsRequired);
            Assert.That(productMetadata.GetPropertyByName("Price").Validation.IsRequired);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void ReferenceTypesAreNotRequiredByDefault()
        {
            //-- Arrange & Act

            var productMetadata = ConstructMetadataOf<TestDataObjects.Repository1.IProduct>();

            //-- Assert

            Assert.That(productMetadata.GetPropertyByName("Description").Validation.IsRequired, Is.False);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private TypeMetadataBuilder ConstructMetadataOf<T>()
        {
            TypeMetadataCache metadataCache;
            var metadataConstructor = CreateMetadataConstructor(out metadataCache);
            var typeMetadata = new TypeMetadataBuilder();

            metadataConstructor.ConstructMetadata(
                primaryContract: typeof(T),
                mixinContracts: Type.EmptyTypes,
                builder: typeMetadata,
                cache: metadataCache);

            return typeMetadata;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private TypeMetadataBuilderConstructor CreateMetadataConstructor(out TypeMetadataCache metadataCache, params MixinRegistration[] mixinRegistrations)
        {
            var conventions = new MetadataConventionSet(
                new IMetadataConvention[] { new ContractMetadataConvention(), new AttributeMetadataConvention(), new RelationMetadataConvention() },
                new IRelationalMappingConvention[] { new PascalCaseRelationalMappingConvention(usePluralTableNames: true) });

            metadataCache = new TypeMetadataCache(conventions, mixinRegistrations, new ConcretizationRegistration[0]);
            var metadataConstructor = new TypeMetadataBuilderConstructor(conventions);
            
            return metadataConstructor;
        }
    }
}
