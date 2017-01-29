using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using System.IO;
using NWheels.Compilation.Mechanism.Factories;

namespace NWheels.Compilation.Adapters.Roslyn
{
    public class ReferenceCache
    {
        private readonly object _syncRoot = new object();
        private ImmutableDictionary<string, MetadataReference> _referenceByAssemblyName =
            ImmutableDictionary.Create<string, MetadataReference>(StringComparer.OrdinalIgnoreCase);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MetadataReference EnsureReferenceCached(string assemblyFilePath)
        {
            MetadataReference reference;

            if (!_referenceByAssemblyName.TryGetValue(assemblyFilePath, out reference))
            {
                lock (_syncRoot)
                {
                    if (!_referenceByAssemblyName.TryGetValue(assemblyFilePath, out reference))
                    {
                        reference = MetadataReference.CreateFromFile(assemblyFilePath);
                        _referenceByAssemblyName = _referenceByAssemblyName.Add(assemblyFilePath, reference);
                    }
                }
            }

            return reference;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void IncludePrerequisiteAssemblyReferences()
        {
            var systemObjectAssemblyLocation = typeof(object).GetTypeInfo().Assembly.Location;
            var systemAssemblyFolder = Path.GetDirectoryName(systemObjectAssemblyLocation);

            EnsureReferenceCached(systemObjectAssemblyLocation);
            EnsureReferenceCached(Path.Combine(systemAssemblyFolder, "mscorlib.dll"));
            EnsureReferenceCached(Path.Combine(systemAssemblyFolder, "System.Runtime.dll"));
            EnsureReferenceCached(typeof(TypeKeyAttribute).GetTypeInfo().Assembly.Location);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IEnumerable<MetadataReference> GetAllCachedReferences()
        {
            return _referenceByAssemblyName.Values;
        }
    }
}
