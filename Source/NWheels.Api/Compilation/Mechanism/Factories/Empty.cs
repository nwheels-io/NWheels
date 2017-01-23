using NWheels.Compilation.Mechanism.Factories;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Compilation.Mechanism.Factories
{
    public static class Empty
    {
        public class KeyExtension : ITypeKeyExtension
        {
            public void Deserialize(object[] values)
            {
            }
            public object[] Serialize()
            {
                return null;
            }
            public override bool Equals(object obj)
            {
                return (obj is KeyExtension);
            }
            public override int GetHashCode()
            {
                return 0;
            }
        }

        public class ContextExtension
        {
        }
    }
}
