using System;
using System.Collections.Generic;
using System.Linq;
using MetaPrograms.Members;

namespace NWheels.Build
{
    public class ClrTypeLoaderSession
    {
        private readonly ClrTypeLoader _loader;

        private readonly Dictionary<TypeMember, Action<Type>> _typeRequests = 
            new Dictionary<TypeMember, Action<Type>>();

        public ClrTypeLoaderSession(ClrTypeLoader loader)
        {
            _loader = loader;
        }
        
        public void AddRequest(TypeMember type, Action<Type> callback)
        {
            Action<Type> existingCallback;
            
            if (_typeRequests.TryGetValue(type, out existingCallback))
            {
                existingCallback += callback;
            }
            else
            {
                existingCallback = callback;
            }

            _typeRequests[type] = existingCallback;
        }

        public IDictionary<Type, TBase> LoadRequestedTypes<TBase>()
        {
            var loadedTypes = _loader.LoadClrTypes(_typeRequests.Keys);

            foreach (var entry in _typeRequests)
            {
                var clrType = loadedTypes[entry.Key];
                entry.Value(clrType);
            }
            
            var result = loadedTypes.ToDictionary(
                entry => loadedTypes[entry.Key],
                entry => (TBase)Activator.CreateInstance(entry.Value));

            return result;
        }
    }
}