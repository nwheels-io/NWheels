using System;
using System.Collections.Generic;

namespace NWheels.Composition.Model.Impl.Metadata
{
    public class ExtensionCollection
    {
        private readonly Dictionary<Type, object> _extensions = new Dictionary<Type, object>();
        
        public T Get<T>() where T : class
        {
            return (T)_extensions[typeof(T)];
        }

        public object Get(Type type) 
        {
            return _extensions[type];
        }

        public T TryGet<T>() where T : class
        {
            if (_extensions.TryGetValue(typeof(T), out var value))
            {
                return (T)value;
            }

            return default;
        }

        public object TryGet(Type type) 
        {
            if (_extensions.TryGetValue(type, out var value))
            {
                return value;
            }

            return default;
        }

        public T GetOrAdd<T>(Func<T> factory) where T : class
        {
            if (_extensions.TryGetValue(typeof(T), out var existingValue))
            {
                return (T)existingValue;
            }

            var newValue = factory();
            _extensions[typeof(T)] = newValue;
            return newValue;
        }

        public object GetOrAdd(Type type, Func<object> factory)
        {
            if (_extensions.TryGetValue(type, out var existingValue))
            {
                return existingValue;
            }

            var newValue = factory();
            _extensions[type] = newValue;
            return newValue;
        }

        public void Set<T>(T value)
        {
            _extensions[typeof(T)] = value;
        }

        public bool Contains<T>()
        {
            return _extensions.ContainsKey(typeof(T));
        }
        
        public IEnumerable<Type> Keys => _extensions.Keys;
    }
}
