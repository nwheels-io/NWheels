using NWheels.Compilation.Mechanism.Syntax.Members;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Compilation.Mechanism.Factories
{
    public interface ITypeFactoryMechanism<TArtifact>
    {
        ITypeKey CreateKey<TKeyExtension>(Type primaryContract, Type[] secondaryContracts = null, TKeyExtension extension = default(TKeyExtension));
        ITypeFactoryContext CreateContext<TContextExtension>(ITypeKey key, TContextExtension extension);
        bool TryGetSiblingType(ITypeKey key, out TypeMember type);
        ITypeFactoryProduct<TArtifact> GetOrBuildProduct(ITypeKey key);
        event Action<BuildingNewTypeEventArgs> BuildingNewProduct;
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class BuildingNewTypeEventArgs : EventArgs
    {
        public BuildingNewTypeEventArgs(ITypeKey key)
        {
            this.Key = key;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ITypeKey Key { get; }
        public List<ITypeFactoryConvention> Pipe { get; } = new List<ITypeFactoryConvention>();
        public ITypeFactoryContext Context { get; set; }
    }
}
