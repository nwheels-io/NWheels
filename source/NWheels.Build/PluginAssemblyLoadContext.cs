using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.DependencyModel.Resolution;

namespace NWheels.Build
{
    public class PluginAssemblyLoadContext : AssemblyLoadContext
    {
        public Assembly LoadWithDependencyResolver(string assemblyFilePath)
        {
            var resolver = new AssemblyDependencyResolver(this, assemblyFilePath);
            return resolver.Assembly;
        }
        
        protected override Assembly Load(AssemblyName assemblyName)
        {
            return null;
        }
        
        private class AssemblyDependencyResolver
        {
            private readonly PluginAssemblyLoadContext _loadContext;
            private readonly string _assemblyPath;
            private readonly DependencyContext _dependencyContext;
            private readonly ICompilationAssemblyResolver _assemblyResolver;

            public AssemblyDependencyResolver(PluginAssemblyLoadContext loadContext, string assemblyPath)
            {
                _loadContext = loadContext;
                _assemblyPath = assemblyPath;
                _assemblyResolver = new CompositeCompilationAssemblyResolver(
                    new ICompilationAssemblyResolver[] {
                        new AppBaseCompilationAssemblyResolver(Path.GetDirectoryName(assemblyPath)),
                        new ReferenceAssemblyPathResolver(),
                        new PackageCompilationAssemblyResolver()
                    }
                );

                this.Assembly = _loadContext.LoadFromAssemblyPath(assemblyPath);
                _dependencyContext = DependencyContext.Load(this.Assembly);
                _loadContext.Resolving += OnResolving;
            }

            public Assembly Assembly { get; }

            private Assembly OnResolving(AssemblyLoadContext context, AssemblyName name)
            {
                var library = _dependencyContext.RuntimeLibraries.FirstOrDefault(NamesMatch);

                if (library != null)
                {
                    var wrapper = new CompilationLibrary(
                        library.Type,
                        library.Name,
                        library.Version,
                        library.Hash,
                        library.RuntimeAssemblyGroups.SelectMany(g => g.AssetPaths),
                        library.Dependencies,
                        library.Serviceable);

                    var assemblies = new List<string>();
                    _assemblyResolver.TryResolveAssemblyPaths(wrapper, assemblies);

                    if (assemblies.Count > 0)
                    {
                        return _loadContext.LoadFromAssemblyPath(assemblies[0]);
                    }
                }

                return null;

                bool NamesMatch(RuntimeLibrary runtime)
                {
                    return string.Equals(runtime.Name, name.Name, StringComparison.OrdinalIgnoreCase);
                }
            }
        }
    }
}
