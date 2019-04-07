using System.Collections;
using System.Collections.Generic;
using NWheels.Composition.Model;
using NWheels.Composition.Model.Impl;
using NWheels.Composition.Model.Impl.Metadata;
using NWheels.DevOps.Model.Impl.Parsers;

namespace NWheels.DevOps.Model
{
    [ModelParser(typeof(SystemMainParser))]
    public abstract class SystemMain : ICanInclude<AnyEnvironment>
    {
    }
}
