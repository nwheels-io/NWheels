using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using NWheels.Processing.Commands;
using NWheels.Testing;

namespace NWheels.UnitTests.Processing.Commands.Factories
{
    [TestFixture]
    public class MethodCallSerializerObjectTests : DynamicTypeUnitTestBase
    {

        public static SerializationTestCase[] ListTestCases()
        {
            return new SerializationTestCase[] {
                
            };
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class SerializationTestCase
        {
            public SerializationTestCase(string name, Action<TestTarget> )
            {
                this.Name = name;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public string Name { get; private set; }
            public 
            public object[] Arguments { get; set; }
            public object ReturnValue { get; set; }
            public bool? InputWireEmptyShouldBeEmpty { get; set; }
            public bool? InputWireEmptyShouldNotBeEmpty { get; set; }
            public bool? OutputWireEmptyShouldBeEmpty { get; set; }
            public bool? OutputWireEmptyShouldNotBeEmpty { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class TestTarget
        {
            
        }
    }
}
