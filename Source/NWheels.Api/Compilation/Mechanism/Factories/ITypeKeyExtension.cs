using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Compilation.Mechanism.Factories
{
    public interface ITypeKeyExtension
    {
        object[] Serialize();
        void Deserialize(object[] values);
    }
}
