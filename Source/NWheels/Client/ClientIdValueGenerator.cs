using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NWheels.DataObjects.Core;

namespace NWheels.Client
{
    public class ClientIdValueGenerator : IPropertyValueGenerator<int>
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
