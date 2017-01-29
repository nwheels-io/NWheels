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
                return (ReferenceEquals(obj, null) || obj is KeyExtension);
            }
            public override int GetHashCode()
            {
                return 0;
            }
            public static bool operator ==(KeyExtension x, KeyExtension y)
            {
                return true;
            }
            public static bool operator !=(KeyExtension x, KeyExtension y)
            {
                return false;
            }

        }

        public class ContextExtension
        {
        }
    }
}
