using System;
using System.Collections.Generic;
using System.Text;
using NWheels.Compilation.Mechanism.Syntax.Members;
using System.Collections.Immutable;
using System.Linq;
using NWheels.Exceptions;

namespace NWheels.Compilation.Mechanism.Factories
{
    public class TypeLibrary<TArtifact> : ITypeLibrary<TArtifact>
    {
        private readonly ITypeFactoryBackend<TArtifact> _backend;
        private readonly Dictionary<TypeKey, TypeMember> _declaredTypeMemberByKey;
        private ImmutableDictionary<TypeKey, TypeFactoryProduct<TArtifact>> _productByKey;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TypeLibrary(ITypeFactoryBackend<TArtifact> backend)
        {
            _backend = backend;
            _declaredTypeMemberByKey = new Dictionary<TypeKey, TypeMember>();
            _productByKey = ImmutableDictionary<TypeKey, TypeFactoryProduct<TArtifact>>.Empty;

            backend.ProductsLoaded += OnBackendProductsLoaded;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ITypeFactoryContext CreateFactoryContext<TExtension>(TypeKey key, TypeMember type, TExtension extension)
        {
            return new TypeFactoryContext<TExtension>(key, type, extension);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void DeclareTypeMember(TypeKey key, TypeMember type)
        {
            _declaredTypeMemberByKey.Add(key, type);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void CompileDeclaredTypeMembers()
        {
            if (_declaredTypeMemberByKey.Count > 0)
            {
                var result = _backend.Compile(_declaredTypeMemberByKey.Values);

                _declaredTypeMemberByKey.Clear();

                if (!result.Success)
                {
                    throw new CompilationErrorsException(result.Failed);
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TypeFactoryProduct<TArtifact> GetProduct(ref TypeKey key)
        {
            return _productByKey[key];
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TypeMember GetOrBuildTypeMember(TypeKey key, Func<TypeKey, TypeMember> factory)
        {
            if (_declaredTypeMemberByKey.TryGetValue(key, out TypeMember existingType))
            {
                return existingType;
            }

            if (_productByKey.TryGetValue(key, out TypeFactoryProduct<TArtifact> existingProduct))
            {
                return _backend.GetBoundTypeMember(existingProduct);
            }

            var newType = factory(key);
            _declaredTypeMemberByKey.Add(key, newType);
            return newType;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public void PopulateProducts(params TypeFactoryProduct<TArtifact>[] products)
        {
            OnBackendProductsLoaded(products);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void OnBackendProductsLoaded(TypeFactoryProduct<TArtifact>[] products)
        {
            var newKeyValuePairs = products.Select(p => new KeyValuePair<TypeKey, TypeFactoryProduct<TArtifact>>(p.Key, p));
            _productByKey = _productByKey.AddRange(newKeyValuePairs);
        }
    }
}
