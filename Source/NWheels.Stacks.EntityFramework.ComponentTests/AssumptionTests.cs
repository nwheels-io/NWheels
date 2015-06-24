using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using HR1 = NWheels.Stacks.EntityFramework.ComponentTests.HardCodedImplementations.Repository1;
using System.Diagnostics;
using Hapil.Members;

namespace NWheels.Stacks.EntityFramework.ComponentTests
{
    [TestFixture]
    public class AssumptionTests
    {
        [Test]
        public void CanEnumerateDbProviders()
        {
            var table = DbProviderFactories.GetFactoryClasses();

            Console.WriteLine(string.Join("/", table.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToArray()));

            foreach ( var row in table.Rows.Cast<DataRow>() )
            {
                Console.WriteLine(row["InvariantName"] + " ----> " + string.Join("/", row.ItemArray.Select(v => v != null ? v.ToString() : "(NULL)").ToArray()));
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanCreateAnyDbConnectionFromConnectionStiring()
        {
            //-- Act

            var factory = DbProviderFactories.GetFactory("System.Data.SqlClient");
            var connection = factory.CreateConnection();

            //-- Assert

            Assert.That(connection, Is.InstanceOf<SqlConnection>());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanBuildCompiledDbModelProgrammatically()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<HR1.EntityObject_Product>().HasEntitySetName("Product");
            modelBuilder.Entity<HR1.EntityObject_Order>().HasEntitySetName("Order");
            modelBuilder.Entity<HR1.EntityObject_OrderLine>().HasEntitySetName("OrderLine");

            var providerInfo = new DbProviderInfo(providerInvariantName: "System.Data.SqlClient", providerManifestToken: "2012");
            var model = modelBuilder.Build(providerInfo);
            var compiledModel = model.Compile();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanFindCorrectPrimitivePropertyMethodOfEntityTypeConfiguration()
        {
            //-- Arrange

            var propertyMethods =
                typeof(EntityTypeConfiguration<>)
                .MakeGenericType(typeof(HR1.EntityObject_Product))
                .GetMethods()
                .Where(m => m.Name == "Property" && m.IsGenericMethod)
                .ToArray();

            //-- Act

            var nonNullablePrimitivePropertyMethod = propertyMethods.Single(m => !m.GetParameters()[0].ParameterType.ToString().Contains("System.Nullable"));
            var nullablePrimitivePropertyMethod = propertyMethods.Single(m => m.GetParameters()[0].ParameterType.ToString().Contains("System.Nullable"));
        }
    }
}
