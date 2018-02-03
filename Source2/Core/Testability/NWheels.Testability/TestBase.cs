using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace NWheels.Testability
{
    public static class TestBase
    {
        private static readonly string _s_testBinaryFolderPath = 
            Path.GetDirectoryName(typeof(TestBase).Assembly.Location);

        private static readonly string _s_testFilesFolderPath = 
            Path.Combine(Path.GetDirectoryName(typeof(TestBase).Assembly.Location), "TestFiles");

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static class Traits
        {
            public const string NamePurpose = "Purpose";
            public const string ValuePurposeUnitTest = "UnitTest";
            public const string ValuePurposeIntegrationTest = "IntegrationTest";
            public const string ValuePurposeSystemApiTest = "SystemApiTest";
            public const string ValuePurposeSystemUITest = "SystemUITest";
            public const string ValuePurposeStressLoadTest = "StressLoadTest";
            public const string ValuePurposeManualTest = "ManualTest";
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public abstract class AbstractTest
        {
            protected string TestBinaryFolderPath => _s_testBinaryFolderPath;            
            protected string TestFilesFolderPath => _s_testFilesFolderPath;            
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Trait(Traits.NamePurpose, Traits.ValuePurposeUnitTest)]
        public abstract class UnitTest : AbstractTest
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Trait(Traits.NamePurpose, Traits.ValuePurposeIntegrationTest)]
        public abstract class IntegrationTest : AbstractTest
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Trait(Traits.NamePurpose, Traits.ValuePurposeSystemApiTest)]
        public abstract class SystemApiTest : AbstractTest
        {
            protected void AssertMicroserviceOutput(MicroserviceProcess microservice, Action assertions)
            {
                try
                {
                    microservice.ExitCode.Should().Be(0, "microservice must exit with code 0");
                    assertions();
                }
                catch (Exception e)
                {
                    throw new AggregateException(e, new MicroserviceProcessException(microservice));
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Trait(Traits.NamePurpose, Traits.ValuePurposeSystemUITest)]
        public abstract class SystemUITest : AbstractTest
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Trait(Traits.NamePurpose, Traits.ValuePurposeStressLoadTest)]
        public abstract class StressLoadTest : AbstractTest
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Trait(Traits.NamePurpose, Traits.ValuePurposeManualTest)]
        public abstract class ManualTest : AbstractTest
        {
        }
    }
}
