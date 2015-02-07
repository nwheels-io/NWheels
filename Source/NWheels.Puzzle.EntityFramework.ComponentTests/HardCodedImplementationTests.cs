using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using HR1 = NWheels.Puzzle.EntityFramework.ComponentTests.HardCodedImplementations.Repository1;

namespace NWheels.Puzzle.EntityFramework.ComponentTests
{
    [TestFixture]
    public class HardCodedImplementationTests : DatabaseTestBase
    {
        [Test]
        public void CanBuildModelAndInitializeObjectContext()
        {
            var modelBuilder = new DbModelBuilder();
            
            modelBuilder.Entity<HR1.EntityObject_Product>().HasEntitySetName("Product");
            modelBuilder.Entity<HR1.EntityObject_Order>().HasEntitySetName("Order");
            modelBuilder.Entity<HR1.EntityObject_OrderLine>().HasEntitySetName("OrderLine");

            //using ( var connection = new SqlConnection(base.ConnectionString) )
            //{
            //    var invariantName = "System.Data.SqlClient";
            //    var manifestToken = DbProviderServices.GetProviderServices(connection).GetProviderManifestToken(connection);
            //    providerInfo = new DbProviderInfo(invariantName, manifestToken);
            //}

            var providerInfo = new DbProviderInfo(providerInvariantName: "System.Data.SqlClient", providerManifestToken: "2012");
            var model = modelBuilder.Build(providerInfo);
            var compiledModel = model.Compile();

            using ( var connection = new SqlConnection(base.ConnectionString) )
            {
                var objectContext = compiledModel.CreateObjectContext<ObjectContext>(connection);

                var productObjectSet = objectContext.CreateObjectSet<HR1.EntityObject_Product>();
                var orderObjectSet = objectContext.CreateObjectSet<HR1.EntityObject_Order>();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
    }
}
