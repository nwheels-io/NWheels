using System;
using System.Collections.Generic;
using System.Text;
using NWheels.Compilation.Mechanism.Syntax.Members;
using System.Collections.Immutable;
using System.Linq;

namespace NWheels.Compilation.Mechanism.Factories
{
    public class TypeLibrary<TArtifact> : ITypeLibrary<TArtifact>
    {
        private readonly ITypeFactoryBackend<TArtifact> _backend;
        private readonly Dictionary<TypeKey, TypeMember> _pendingTypeByKey;
        private ImmutableDictionary<TypeKey, TypeFactoryProduct<TArtifact>> _productByKey;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TypeLibrary(ITypeFactoryBackend<TArtifact> backend)
        {
            _backend = backend;
            _pendingTypeByKey = new Dictionary<TypeKey, TypeMember>();
            _productByKey = ImmutableDictionary<TypeKey, TypeFactoryProduct<TArtifact>>.Empty;

            backend.ProductsLoaded += OnBackendProductsLoaded;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TypeKey<TKeyExtension> CreateKey<TKeyExtension>(
            TypeMember primaryContract, 
            TypeMember[] secondaryContracts = null, 
            TKeyExtension extension = default(TKeyExtension))
            where TKeyExtension : ITypeKeyExtension, new()
        {
            return new RealTypeKey<TKeyExtension>(primaryContract, secondaryContracts, extension);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ITypeFactoryContext CreateFactoryContext<TContextExtension>(TypeKey key, TypeMember type, TContextExtension extension)
        {
            var keyInternals = (ITypeKeyInternals)key;
            return keyInternals.CreateContext<TContextExtension>(type, extension);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void DeclareTypeMember(TypeKey key, TypeMember type)
        {
            _pendingTypeByKey.Add(key, type);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void CompilePendingTypeMembers()
        {
            if (_pendingTypeByKey.Count > 0)
            {
                _backend.Compile(_pendingTypeByKey.Values);
                _pendingTypeByKey.Clear();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TypeFactoryProduct<TArtifact> GetProduct(TypeKey key)
        {
            return _productByKey[key];
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TypeMember GetOrBuildTypeMember(TypeKey key)
        {
            TypeMemberMissing?.Invoke(null);
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public event Action<TypeMemberMissingEventArgs> TypeMemberMissing;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void OnBackendProductsLoaded(TypeFactoryProduct<TArtifact>[] products)
        {
            var newKeyValuePairs = products.Select(p => new KeyValuePair<TypeKey, TypeFactoryProduct<TArtifact>>(p.Key, p));
            _productByKey = _productByKey.AddRange(newKeyValuePairs);
        }
    }
}
