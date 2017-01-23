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
        public TypeFactoryContext(TypeKey<TKeyExtension> key, TypeMember type, TContextExtension extension)
        {
            this.Key = key;
            this.Type = type;
            this.Extension = extension;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        TypeKey ITypeFactoryContext.Key => (TypeKey)this.Key;
        object ITypeFactoryContext.Extension => this.Extension;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TypeKey<TKeyExtension> Key { get; }
        public TypeMember Type { get; }
        public TContextExtension Extension { get; }
    }
}
