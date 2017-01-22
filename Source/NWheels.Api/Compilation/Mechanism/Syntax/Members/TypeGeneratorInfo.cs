using NWheels.Compilation.Mechanism.Factories;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Compilation.Mechanism.Syntax.Members
{
    public struct TypeGeneratorInfo
    {
        public TypeGeneratorInfo(Type factoryType, ITypeKey typeKey)
        {
            this.FactoryType = factoryType;
            this.TypeKey = typeKey;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Type FactoryType { get; }
        public ITypeKey TypeKey { get; }
    }
}
