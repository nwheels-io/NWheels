using System;
using System.Collections.Generic;

namespace NWheels.Kernel.Api.Extensions
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

        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue) 
        {
            TValue value;

            if ( dictionary.TryGetValue(key, out value) )
            {
                return value;
            }

            return defaultValue;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static TValue GetValueOrCreateDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TKey, TValue> defaultValueFactory)
        {
            TValue value;

            if (dictionary.TryGetValue(key, out value))
            {
                return value;
            }

            return defaultValueFactory(key);
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

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static TValue GetOrThrow<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, string keyNotFoundExceptionFormat)
        {
            TValue value;

            if ( dictionary.TryGetValue(key, out value) )
            {
                return value;
            }

            throw new KeyNotFoundException(string.Format(keyNotFoundExceptionFormat, key));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static TValue ReadOnlyGetValueOrDefault<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue)
        {
            TValue value;

            if ( dictionary.TryGetValue(key, out value) )
            {
                return value;
            }

            return defaultValue;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static TValue ReadOnlyGetOrThrow<TKey, TValue>(
            this IReadOnlyDictionary<TKey, TValue> dictionary, 
            TKey key, 
            Func<TKey, Exception> keyNotFoundExceptionFactory)
        {
            TValue value;

            if ( dictionary.TryGetValue(key, out value) )
            {
                return value;
            }

            var exception = keyNotFoundExceptionFactory(key);
            throw exception;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static TValue ReadOnlyGetOrThrow<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary, TKey key, string keyNotFoundExceptionFormat)
        {
            TValue value;

            if ( dictionary.TryGetValue(key, out value) )
            {
                return value;
            }

            throw new KeyNotFoundException(string.Format(keyNotFoundExceptionFormat, key));
        }
    }
}
