using System.Collections;
using System.Collections.Generic;
using NWheels.Composition.Model;
using NWheels.Composition.Model.Metadata;
using NWheels.DevOps.Model.Metadata;

namespace NWheels.DevOps.Model
{
    [ModelParser(typeof(SystemMainParser))]
    public abstract class SystemMain : ICanInclude<AnyEnvironment>
    {
    }
}
