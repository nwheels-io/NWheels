using System;
using System.Collections.Generic;
using System.Text;
using NWheels.Compilation.Mechanism.Syntax.Members;

namespace NWheels.Compilation.Mechanism.Factories
{
    public class TypeFactoryMechanism<TArtifact> : ITypeFactoryMechanism<TArtifact>
    {
        private readonly ITypeFactoryBackend<TArtifact> _backend;

        public TypeFactoryMechanism(ITypeFactoryBackend<TArtifact> backend)
        {
            _backend = backend;
        }

        public ITypeFactoryContext CreateContext<TContextExtension>(ITypeKey key, TContextExtension extension)
        {
            throw new NotImplementedException();
        }

        public ITypeKey CreateKey<TKeyExtension>(Type primaryContract, Type[] secondaryContracts = null, TKeyExtension extension = default(TKeyExtension))
        {
            throw new NotImplementedException();
        }

        public ITypeFactoryProduct<TArtifact> GetOrBuildProduct(ITypeKey key)
        {
            BuildingNewProduct?.Invoke(null);
            throw new NotImplementedException();
        }

        public bool TryGetSiblingType(ITypeKey key, out TypeMember type)
        {
            throw new NotImplementedException();
        }

        public event Action<BuildingNewTypeEventArgs> BuildingNewProduct;
    }
}
