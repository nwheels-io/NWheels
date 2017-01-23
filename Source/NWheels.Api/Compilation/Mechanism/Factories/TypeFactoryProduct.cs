using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Compilation.Mechanism.Factories
{
    public abstract class TypeFactoryProduct
    {
        public abstract TypeKey Key { get; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class TypeFactoryProduct<TArtifact> : TypeFactoryProduct
    {
        public TypeFactoryProduct(TypeKey key, TArtifact artifact)
        {
            this.Key = key;
            this.Artifact = artifact;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override TypeKey Key { get; }
        public TArtifact Artifact { get; }
    }
}
