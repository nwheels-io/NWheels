#if false

//??? not sure how much this type is useful

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace NWheels.DataStructures
{
    public class ReadThroughDictionary<TKey, TValue>
    {
        private readonly object _syncRoot;
        private ImmutableDictionary<TKey, TValue> _values;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ReadThroughDictionary()
            : this(ImmutableDictionary<TKey, TValue>.Empty)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ReadThroughDictionary(ImmutableDictionary<TKey, TValue> initialValues)
        {
            _values = initialValues;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TValue GetOrAdd(TKey key, Func<TKey, TValue> factory)
        {
            if (_values.TryGetValue(key, out TValue existingValue))
            {
                return existingValue;
            }

            lock (_syncRoot)
            {
                if (_values.TryGetValue(key, out existingValue))
                {
                    return existingValue;
                }

                var newValue = factory(key);
                _values = _values.Add(key, newValue);
                return newValue;
            }
        }
    }
}

#endif