using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Xunit;

namespace NWheels.Testability
{
    public static class TestBase
    {
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

        [Trait(Traits.NamePurpose, Traits.ValuePurposeUnitTest)]
        public abstract class UnitTest
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Trait(Traits.NamePurpose, Traits.ValuePurposeIntegrationTest)]
        public abstract class IntegrationTest
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Trait(Traits.NamePurpose, Traits.ValuePurposeSystemApiTest)]
        public abstract class SystemApiTest
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Trait(Traits.NamePurpose, Traits.ValuePurposeSystemUITest)]
        public abstract class SystemUITest
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Trait(Traits.NamePurpose, Traits.ValuePurposeStressLoadTest)]
        public abstract class StressLoadTest
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Trait(Traits.NamePurpose, Traits.ValuePurposeManualTest)]
        public abstract class ManualTest
        {
        }
    }
}
