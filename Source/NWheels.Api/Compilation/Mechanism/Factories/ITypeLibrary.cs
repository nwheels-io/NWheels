using NWheels.Compilation.Mechanism.Syntax.Members;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Compilation.Mechanism.Factories
{
    public interface ITypeLibrary<TArtifact>
    {
        ITypeKey CreateKey<TKeyExtension>(Type primaryContract, Type[] secondaryContracts = null, TKeyExtension extension = default(TKeyExtension));
        ITypeFactoryContext CreateContext<TContextExtension>(ITypeKey key, TypeMember product, TContextExtension extension);
        TypeMember GetOrBuildTypeMember(ITypeKey key);
        ITypeFactoryProduct<TArtifact> GetProduct(ITypeKey key);
        event Action<BuildingNewProductEventArgs> BuildingNewProduct;
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class BuildingNewProductEventArgs : EventArgs
    {
        public BuildingNewProductEventArgs(ITypeKey key)
        {
            this.Key = key;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ITypeKey Key { get; }
        public List<ITypeFactoryConvention> Pipe { get; } = new List<ITypeFactoryConvention>();
        public ITypeFactoryContext Context { get; set; }
    }
}
