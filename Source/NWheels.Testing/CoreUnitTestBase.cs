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

namespace NWheels.Testing
{
    [TestFixture]
    public abstract class CoreUnitTestBase : UnitTestBase
    {
        private TypeMetadataCache _metadataCache;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [SetUp]
        public void CoreBaseSetUp()
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [TearDown]
        public void CoreBaseTearDown()
        {
            _metadataCache = null;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected TypeMetadataCache CreateMetadataCache(params MixinRegistration[] mixinRegistrations)
        {
            _metadataCache = TestFramework.CreateMetadataCacheWithDefaultConventions(mixinRegistrations);
            return _metadataCache;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected TypeMetadataCache MetadataCache
        {
            get { return _metadataCache; }
        }
    }
}