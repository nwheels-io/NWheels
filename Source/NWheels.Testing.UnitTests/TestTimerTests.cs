using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace NWheels.Testing.UnitTests
{
    [TestFixture]
    public class TestTimerTests : UnitTestBase
    {
        [Test]
        public void CanSetupNewTimer()
        {
            Framework.NewTimer<int>(
                "T1", "I1", TimeSpan.Zero, TimeSpan.FromSeconds(1), 
                parameter => {
                    Console.WriteLine(parameter);    
                }, 
                parameter: 123);
        }
    }
}
