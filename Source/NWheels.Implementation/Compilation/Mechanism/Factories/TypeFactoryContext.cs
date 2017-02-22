using NWheels.Compilation.Mechanism.Factories;
using System;
using System.Collections.Generic;
using System.Text;
using NWheels.Compilation.Mechanism.Syntax.Members;

namespace NWheels.Compilation.Mechanism.Factories
{
    public class TypeFactoryContext<TContextExtension> : ITypeFactoryContext<TContextExtension>, ITypeFactoryContext
    {
        public TypeFactoryContext(TypeKey key, TypeMember type, TContextExtension extension)
        {
            this.Key = key;
            this.Type = type;
            this.Extension = extension;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        TypeKey ITypeFactoryContext.Key => (TypeKey)this.Key;
        object ITypeFactoryContext.Extension => this.Extension;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TypeKey Key { get; }
        public TypeMember Type { get; }
        public TContextExtension Extension { get; }
    }
}
