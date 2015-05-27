using System.Collections.Concurrent;
using System.Threading;
using NWheels.DataObjects.Core;

namespace NWheels.Testing.Entities.Impl
{
    public class TestIntIdValueGenerator : IPropertyValueGenerator<int>
    {
        private readonly ConcurrentDictionary<string, Counter> _countersByQualifiedName = new ConcurrentDictionary<string, Counter>();

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public int GenerateValue(string qualifiedPropertyName)
        {
            var counter = _countersByQualifiedName.GetOrAdd(qualifiedPropertyName ?? "", new Counter());
            return Interlocked.Increment(ref counter.Value);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class Counter
        {
            public int Value;
        }
    }
}
