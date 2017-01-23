using System;
using System.Collections.Generic;
using System.Text;
using NWheels.Compilation.Mechanism.Syntax.Members;

namespace NWheels.Compilation.Mechanism.Factories
{
    public class TypeLibrary<TArtifact> : ITypeLibrary<TArtifact>
    {
        private readonly ITypeFactoryBackend<TArtifact> _backend;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TypeLibrary(ITypeFactoryBackend<TArtifact> backend)
        {
            _backend = backend;
            backend.ArtifactsLoaded += OnBackendArtifactsLoaded;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TypeKey CreateKey<TKeyExtension>(
            TypeMember primaryContract, 
            TypeMember[] secondaryContracts = null, 
            TKeyExtension extension = default(TKeyExtension))
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ITypeFactoryContext CreateContext<TContextExtension>(TypeKey key, TypeMember product, TContextExtension extension)
        {
            var keyInternals = (ITypeKeyInternals)key;
            return keyInternals.CreateContext<TContextExtension>(product, extension);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ITypeFactoryProduct<TArtifact> GetProduct(TypeKey key)
        {
            BuildingNewProduct?.Invoke(null);
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TypeMember GetOrBuildTypeMember(TypeKey key)
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public event Action<BuildingNewProductEventArgs> BuildingNewProduct;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void OnBackendArtifactsLoaded(TArtifact[] artifacts)
        {
            throw new NotImplementedException();
        }
    }
}
