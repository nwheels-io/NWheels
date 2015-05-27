using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using NWheels.Extensions;

namespace NWheels.UnitTests.Extensions
{
    [TestFixture]
    public class TypeExtensionTests
    {
        [TestCase(typeof(List<int>), typeof(List<>), true)]
        [TestCase(typeof(IList<int>), typeof(IList<>), true)]
        [TestCase(typeof(IDictionary<int, string>), typeof(IDictionary<,>), true)]
        [TestCase(typeof(IDictionary<int, string>), typeof(Tuple<,>), false)]
        [TestCase(typeof(List<int>), typeof(LinkedList<int>), false)]
        [TestCase(typeof(Queue<int>), typeof(Stack<>), false)]
        [TestCase(typeof(DayOfWeek), typeof(Stack<>), false)]
        [TestCase(typeof(Stack<int>), typeof(DayOfWeek), false)]
        public void TestIsConstructedGenericTypeOf(Type typeUnderTest, Type genericTypeDefinition, bool expectedResult)
        {
            var actualResult = typeUnderTest.IsConstructedGenericTypeOf(genericTypeDefinition);
            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }
    }
}
