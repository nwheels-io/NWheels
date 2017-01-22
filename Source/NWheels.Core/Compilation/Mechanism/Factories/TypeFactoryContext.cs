using NWheels.Compilation.Mechanism.Factories;
using System;
using System.Collections.Generic;
using System.Text;
using NWheels.Compilation.Mechanism.Syntax.Members;

namespace NWheels.Compilation.Mechanism.Factories
{
    public class TypeFactoryContext<TKeyExtension, TContextExtension> : ITypeFactoryContext<TKeyExtension, TContextExtension>, ITypeFactoryContext
        where TKeyExtension : ITypeKeyExtension, new()
    {
        public TypeFactoryContext(ITypeKey<TKeyExtension> key, TypeMember product, TContextExtension extension)
        {
            this.Key = key;
            this.Product = product;
            this.Extension = extension;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        ITypeKey ITypeFactoryContext.Key => (ITypeKey)this.Key;
        object ITypeFactoryContext.Extension => this.Extension;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ITypeKey<TKeyExtension> Key { get; }
        public TypeMember Product { get; }
        public TContextExtension Extension { get; }
    }
}
