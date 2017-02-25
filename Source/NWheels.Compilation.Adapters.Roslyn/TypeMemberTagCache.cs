using System;
using System.Collections.Generic;
using System.Text;
using NWheels.Compilation.Mechanism.Syntax.Members;

namespace NWheels.Compilation.Adapters.Roslyn
{
    public class TypeMemberTagCache : IDisposable
    {
        private readonly Dictionary<TypeMember, RoslynTypeFactoryBackend.BackendTag> _backendTagByType = 
            new Dictionary<TypeMember, RoslynTypeFactoryBackend.BackendTag>();

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private TypeMemberTagCache()
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal RoslynTypeFactoryBackend.BackendTag GetOrAddBackendTagFor(TypeMember type)
        {
            if (_backendTagByType.TryGetValue(type, out RoslynTypeFactoryBackend.BackendTag existingTag))
            {
                return existingTag;
            }

            var newTag = new RoslynTypeFactoryBackend.BackendTag();
            _backendTagByType[type] = newTag;
            return newTag;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        void IDisposable.Dispose()
        {
            _s_current = null;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static TypeMemberTagCache CreateOnCurrentThread()
        {
            if (_s_current != null)
            {
                throw new InvalidOperationException("Current thread already has an associated instance of TypeMemberTagCache.");
            }

            var cache = new TypeMemberTagCache();
            _s_current = cache;
            return cache;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [ThreadStatic]
        private static TypeMemberTagCache _s_current;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static TypeMemberTagCache Current
        {
            get
            {
                var cache = _s_current;

                if (cache != null)
                {
                    return cache;
                }

                throw new InvalidOperationException("Current thread has no associated instance of TypeMemberTagCache.");
            }
        }
    }
}
