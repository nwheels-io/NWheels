using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Compilation.Mechanism.Factories
{
    public interface ITypeFactory
    {
        Type ArtifactType { get; }
    }
}
