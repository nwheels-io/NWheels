using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Extensions
{
    public static class DictionaryExtensions
    {
        public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TKey, TValue> valueFactory)
        {
            TValue value;

            if ( !dictionary.TryGetValue(key, out value) )
            {
                value = valueFactory(key);
                dictionary.Add(key, value);
            }

            return value;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key) where TValue : new()
        {
            TValue value;

            if ( !dictionary.TryGetValue(key, out value) )
            {
                value = new TValue();
                dictionary.Add(key, value);
            }

            return value;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        public static TValue GetOrThrow<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TKey, Exception> keyNotFoundExceptionFactory)
        {
            TValue value;

            if ( dictionary.TryGetValue(key, out value) )
            {
                return value;
            }

            var exception = keyNotFoundExceptionFactory(key);
            throw exception;
        }
    }
}
