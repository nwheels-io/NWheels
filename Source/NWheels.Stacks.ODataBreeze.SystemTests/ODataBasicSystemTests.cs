#if false

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Autofac;
using MongoDB.Driver;
using NUnit.Framework;
using NWheels.Conventions.Core;
using NWheels.DataObjects.Core.Conventions;
using NWheels.Domains.Security;
using NWheels.Domains.Security.UI;
using NWheels.Entities;
using NWheels.Extensions;
using NWheels.Hosting;
using NWheels.Hosting.Core;
using NWheels.Testing;
using NWheels.Testing.Entities.Impl;
using NWheels.Utilities;

namespace NWheels.Stacks.ODataBreeze.SystemTests
{
    [TestFixture, Category("System")]
    public class ODataBasicSystemTests : SystemTestBase
    {
        public const string TestDatabaseName = "NWheelsTest";
        public const string TestEndpointUrl = "http://localhost:8901/";

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        static ODataBasicSystemTests()
        {
            AssemblyUtility.Redirect("System.Web.Http", new Version("5.2.3.0"), publicKeyToken: "31bf3856ad364e35");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test, Category("Manual")]
        public void ManualRestEndpointTest()
        {
            System.Windows.Forms.MessageBox.Show("SERVER RUNNING. PRESS OK TO STOP.");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanDownloadMetadata()
        {
            using ( var client = new WebClient() )
            {
                var metadata = client.DownloadString(TestEndpointUrl + "Metadata");
                Console.WriteLine(metadata);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnBuildingBootConfiguration(BootConfiguration configuration)
        {
            configuration
                .AddStackModule("NWheels.Stacks.MongoDb")
                .AddStackModule("NWheels.Stacks.ODataBreeze")
                .AddDomainModule("NWheels.Domains.Security");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnRegisteringModuleComponents(ContainerBuilder builder)
        {
            base.OnRegisteringModuleComponents(builder);
            builder.NWheelsFeatures().Entities().RegisterDataRepository<ITestDataRepository>().WithRestEndpoint(defaultListenUrl: TestEndpointUrl);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnInitializedStorage()
        {
            TestData.SecurityDomain.InsertBasic(base.Framework);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override string StorageConnectionString
        {
            get
            {
                return "server=localhost;database=" + TestDatabaseName;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface ITestDataRepository : IApplicationDataRepository
        {
            IEntityRepository<IUserAccountEntity> AllUsers { get; }
            IEntityRepository<IFrontEndUserAccountEntity> FrontEndUsers { get; }
            IEntityRepository<IBackEndUserAccountEntity> BackEndUsers { get; }
        }
    }
}

#endif