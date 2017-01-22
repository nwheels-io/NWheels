using System;
using System.Collections.Generic;
using System.Text;
using NWheels.Compilation.Mechanism.Syntax.Members;

namespace NWheels.Compilation.Mechanism.Factories
{
    public class TypeKey<TExtension> : ITypeKey<TExtension>, ITypeKey, ITypeKeyInternals
        where TExtension : ITypeKeyExtension, new()
    {
        public TypeKey(TypeMember primaryContract, TypeMember[] secondaryContracts, TExtension extension)
        {
            this.PrimaryContract = primaryContract;
            this.SecondaryContracts = secondaryContracts;
            this.Extension = extension;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        ITypeFactoryContext ITypeKeyInternals.CreateContext<TContextExtension>(TypeMember product, TContextExtension contextExtension)
        {
            return new TypeFactoryContext<TExtension, TContextExtension>(this, product, contextExtension);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TypeMember PrimaryContract { get; }
        public IReadOnlyList<TypeMember> SecondaryContracts { get; }
        public TExtension Extension { get; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        object ITypeKey.Extension => this.Extension;
    }
    
    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface ITypeKeyInternals
    {
        ITypeFactoryContext CreateContext<TContextExtension>(TypeMember product, TContextExtension contextExtension);
    }
}
