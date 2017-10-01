using System.Collections.Generic;
using System.Collections.Immutable;

namespace NWheels.Microservices.Api
{
    public interface IAssemblyLocationMap
    {
        IEnumerable<string> Directories { get; }
        ImmutableDictionary<string, string> FilePathByAssemblyName { get; }
    }
}