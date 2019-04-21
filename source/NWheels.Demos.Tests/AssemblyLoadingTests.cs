using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.DependencyModel.Resolution;
using NUnit.Framework;
using System.Collections.Generic;
using NWheels.Build;

namespace NWheels.Demos.Tests
{
    public class AssemblyLoadingTests
    {
        [Test]
        public void CanLoadPluginAssembly()
        {
            const string assemblyPath = @"/Users/felixb/oss/NWheels/source/NWheels.DevOps.Adapters/bin/Debug/netstandard2.0/NWheels.DevOps.Adapters.dll";
            
            var context = new PluginAssemblyLoadContext();
            var assembly = context.LoadWithDependencyResolver(assemblyPath);
            var serviceType = assembly.GetType("NWheels.DevOps.Adapters.Common.K8sYaml.K8sService");
            var serviceInstance = Activator.CreateInstance(serviceType);

            var toYamlStringMethod = serviceType.GetMethod("ToYamlString");
            var yaml = (string) toYamlStringMethod.Invoke(serviceInstance, new object[0]);

            Console.WriteLine(yaml);
        }

        #if fasle
        public class PluginLoadContext : AssemblyLoadContext
        {
            private readonly string _assemblyPath;
            private readonly DependencyContext _dependencyContext;
            private readonly ICompilationAssemblyResolver _assemblyResolver;

            public PluginLoadContext(string assemblyPath)
            {
                _assemblyPath = assemblyPath;
                _assemblyResolver = new CompositeCompilationAssemblyResolver(
                    new ICompilationAssemblyResolver[] {
                        new AppBaseCompilationAssemblyResolver(Path.GetDirectoryName(assemblyPath)),
                        new ReferenceAssemblyPathResolver(),
                        new PackageCompilationAssemblyResolver()
                    }
                );

                this.Assembly = this.LoadFromAssemblyPath(assemblyPath);
                _dependencyContext = DependencyContext.Load(this.Assembly);

                this.Resolving += OnResolving;
            }

            public Assembly Assembly { get; }
            
            protected override Assembly Load(AssemblyName assemblyName)
            {
                return null;
            }

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
                        return this.LoadFromAssemblyPath(assemblies[0]);
                    }
                }

                return null;                

                bool NamesMatch(RuntimeLibrary runtime)
                {
                    return string.Equals(runtime.Name, name.Name, StringComparison.OrdinalIgnoreCase);
                }
            }
        }
        #endif
    }
}
