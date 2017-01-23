using NWheels.Compilation.Mechanism.Syntax.Members;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Compilation.Mechanism.Factories
{
    public interface ITypeLibrary<TArtifact>
    {
        TypeKey CreateKey<TKeyExtension>(TypeMember primaryContract, TypeMember[] secondaryContracts = null, TKeyExtension extension = default(TKeyExtension));
        ITypeFactoryContext CreateContext<TContextExtension>(TypeKey key, TypeMember product, TContextExtension extension);
        TypeMember GetOrBuildTypeMember(TypeKey key);
        ITypeFactoryProduct<TArtifact> GetProduct(TypeKey key);
        event Action<BuildingNewProductEventArgs> BuildingNewProduct;
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class BuildingNewProductEventArgs : EventArgs
    {
        public BuildingNewProductEventArgs(TypeKey key)
        {
            this.Key = key;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TypeKey Key { get; }
        public List<ITypeFactoryConvention> Pipe { get; } = new List<ITypeFactoryConvention>();
        public ITypeFactoryContext Context { get; set; }
    }
}
