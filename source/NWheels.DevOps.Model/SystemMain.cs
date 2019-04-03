using System.Collections;
using System.Collections.Generic;
using NWheels.Composition.Model;
using NWheels.Composition.Model.Metadata;

namespace NWheels.DevOps.Model
{
    [ModelParser]
    public abstract class SystemMain : ICanInclude<AnyEnvironment>
    {
    }
}
