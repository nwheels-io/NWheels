using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;
using System.Xml;
using Hapil;
using Hapil.Testing.NUnit;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Csdl;
using Microsoft.OData.Edm.Library;
using NUnit.Framework;

namespace LinqPadODataV4Driver.Tests
{
    [TestFixture]
    public class EntityObjectFactoryTests : NUnitEmittedTypesTestBase
    {
        [Test]
        public void CanGenerateEntityClass()
        {
            //-- Arrange

            var model = ParseTestModelEdmx();
            var entityEdmType = model.SchemaElements.OfType<IEdmEntityType>().First(t => t.Name == "Product");
            var clrTypeCache = new EntityClrTypeCache(base.Module, model);

            //-- Act

            var entityClrType = clrTypeCache.GetEntityClrType(entityEdmType);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private IEdmModel ParseTestModelEdmx()
        {
            using ( var xmlReader = XmlReader.Create(new StringReader(TestModelEdmx)) )
            {
                return EdmxReader.Parse(xmlReader);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private const string TestModelEdmx = @"<edmx:Edmx Version=""4.0"" xmlns:edmx=""http://docs.oasis-open.org/odata/ns/edmx"">
            <edmx:DataServices>
            <Schema Namespace=""NWheels.Samples.RestService"" xmlns=""http://docs.oasis-open.org/odata/ns/edm"">
                <EntityType Name=""Product"">
                <Key>
                    <PropertyRef Name=""Id"" />
                </Key>
                <Property Name=""Id"" Type=""Edm.Int32"" Nullable=""false"" />
                <Property Name=""Name"" Type=""Edm.String"" />
                <Property Name=""Price"" Type=""Edm.Decimal"" Nullable=""false"" />
                <NavigationProperty Name=""OrderLines"" Type=""Collection(NWheels.Samples.RestService.OrderLine)"" />
                </EntityType>
                <EntityType Name=""Order"">
                <Key>
                    <PropertyRef Name=""Id"" />
                </Key>
                <Property Name=""Id"" Type=""Edm.Int32"" Nullable=""false"" />
                <Property Name=""DateTime"" Type=""Edm.DateTimeOffset"" Nullable=""false"" />
                <Property Name=""CustomerEmail"" Type=""Edm.String"" />
                <NavigationProperty Name=""OrderLines"" Type=""Collection(NWheels.Samples.RestService.OrderLine)"" />
                </EntityType>
                <EntityType Name=""OrderLine"">
                <Key>
                    <PropertyRef Name=""Id"" />
                </Key>
                <Property Name=""Id"" Type=""Edm.Int32"" Nullable=""false"" />
                <Property Name=""Quantity"" Type=""Edm.Int32"" Nullable=""false"" />
                <NavigationProperty Name=""Product"" Type=""NWheels.Samples.RestService.Product"" />
                <NavigationProperty Name=""Order"" Type=""NWheels.Samples.RestService.Order"" />
                </EntityType>
            </Schema>
            <Schema Namespace=""Default"" xmlns=""http://docs.oasis-open.org/odata/ns/edm"">
                <EntityContainer Name=""Container"">
                <EntitySet Name=""Product"" EntityType=""NWheels.Samples.RestService.Product"" />
                <EntitySet Name=""Order"" EntityType=""NWheels.Samples.RestService.Order"" />
                </EntityContainer>
            </Schema>
            </edmx:DataServices>
        </edmx:Edmx>";
    }
}
