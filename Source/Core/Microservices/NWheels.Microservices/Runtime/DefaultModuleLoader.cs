using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using NWheels.Kernel.Api.Extensions;
using NWheels.Kernel.Api.Injection;
using NWheels.Kernel.Runtime.Injection;
using NWheels.Microservices.Api;
using NWheels.Microservices.Api.Exceptions;

namespace NWheels.Microservices.Runtime
{
    public class DefaultModuleLoader : IModuleLoader
    {
        private readonly IBootConfiguration _bootConfig;
        private readonly IComponentContainer _bootComponents;
        private readonly Dictionary<string, Type[]> _publicTypeCache = new Dictionary<string, Type[]>();
        private readonly Dictionary<string, Assembly> _assemblyCache = new Dictionary<string, Assembly>();

        //private readonly IBootConfiguration _bootConfig;
        //private readonly ModuleAssemblyLoadContext _assemblyLoadContext;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public DefaultModuleLoader(IBootConfiguration bootConfig, IComponentContainer bootComponents)
        {
            _bootConfig = bootConfig;
            _bootComponents = bootComponents;
            //_bootConfig = bootConfig;
            //_assemblyLoadContext = new ModuleAssemblyLoadContext(bootConfig.AssemblyLocationMap);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual IEnumerable<IFeatureLoader> GetBootFeatureLoaders()
        {
            var bootConfig = _bootConfig;
            var allModuleConfigs = 
                bootConfig.FrameworkModules
                .Concat(bootConfig.ApplicationModules)
                .Concat(bootConfig.CustomizationModules);

            var bootFeatureLoaderTypes = allModuleConfigs.SelectMany(m => GetModuleBootFeatureLoaderTypes(m)).ToArray();
            var bootFeatureLoaders = new List<IFeatureLoader>();

            RegisterFeatureLoadersInBootContainer(bootFeatureLoaderTypes);

            foreach (var type in bootFeatureLoaderTypes)
            {
                bootFeatureLoaders.Add((IFeatureLoader)_bootComponents.Resolve(type));
            }

            return bootFeatureLoaders;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual IEnumerable<Type> GetModuleBootFeatureLoaderTypes(IModuleConfiguration moduleConfig)
        {
            var allFeatureLoaderTypes = GetModuleAllFeatureLoaderTypes(moduleConfig);
            var namedFeatureLoaderTypeMap = new Dictionary<string, Type>();
            var defaultFeatureLoaderTypes = new List<Type>();
            var namedFeatureLoaderTypes = new List<Type>();

            foreach (var type in allFeatureLoaderTypes)
            {
                if (type.IsDefined(typeof(DefaultFeatureLoaderAttribute)))
                {
                    defaultFeatureLoaderTypes.Add(type);
                }
                else
                {
                    var name = FeatureLoaderAttribute.GetFeatureNameOrThrow(type);
                    namedFeatureLoaderTypeMap.Add(name, type);
                }
            }

            foreach (var feature in moduleConfig.Features)
            {
                if (namedFeatureLoaderTypeMap.TryGetValue(feature.FeatureName, out Type type))
                {
                    if (namedFeatureLoaderTypes.Contains(type))
                    {
                        throw ModuleLoaderException.DuplicateNamedFeature(moduleConfig.ModuleName, feature.FeatureName);
                    }

                    namedFeatureLoaderTypes.Add(type);
                }
                else
                {
                    throw ModuleLoaderException.NamedFeatureDoesNotExist(moduleConfig.ModuleName, feature.FeatureName);
                }
            }

            return defaultFeatureLoaderTypes.Concat(namedFeatureLoaderTypes);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual IEnumerable<Type> GetModulePublicTypes(IModuleConfiguration moduleConfig)
        {
            var moduleName = moduleConfig.ModuleName;

            if (_publicTypeCache.TryGetValue(moduleName, out Type[] cachedAnswer))
            {
                return cachedAnswer;
            }

            var assembly = moduleConfig.RuntimeAssembly ?? GetAssembly(moduleConfig.AssemblyName);
            var publicTypes = assembly.GetExportedTypes();

            _publicTypeCache.Add(moduleName, publicTypes);
            return publicTypes;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual IEnumerable<Type> GetModuleAllFeatureLoaderTypes(IModuleConfiguration moduleConfig)
        {
            var featureLoaderTypes = GetModulePublicTypes(moduleConfig)
                .Where(type =>
                    type.IsClass &&
                    !type.IsAbstract &&
                    typeof(IFeatureLoader).IsAssignableFrom(type));

            return featureLoaderTypes;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual Assembly GetAssembly(string name)
        {
            if (_assemblyCache.TryGetValue(name, out Assembly cachedAssembly))
            {
                return cachedAssembly;
            }

            var foundAssembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(asm => asm.GetName().Name == name);

            if (foundAssembly == null)
            {
                var assemblyFilePath = PathUtility.ExpandPathFromBinary(name + ".dll");
                foundAssembly = Assembly.LoadFrom(assemblyFilePath);
                //foundAssembly = Assembly.Load(new AssemblyName() {
                //    Name = name
                //});
                //foundAssembly = _assemblyLoadContext.LoadFromAssemblyName(new AssemblyName() {
                //    Name = name
                //});
            }

            _assemblyCache.Add(name, foundAssembly);
            return foundAssembly;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void RegisterFeatureLoadersInBootContainer(Type[] featureLoaderTypes)
        {
            var builder = new ComponentContainerBuilder(_bootComponents.AsInternal());

            foreach (var type in featureLoaderTypes)
            {
                builder.RegisterComponentType(type).ForService<IFeatureLoader>();
            }

            _bootComponents.AsInternal().Merge(builder); 
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

#if false
        private class ModuleAssemblyLoadContext : AssemblyLoadContext
        {
            private readonly IAssemblyLocationMap _locationMap;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ModuleAssemblyLoadContext(IAssemblyLocationMap locationMap)
            {
                _locationMap = locationMap;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected override Assembly Load(AssemblyName assemblyName)
            {
                if (_locationMap.FilePathByAssemblyName.TryGetValue(assemblyName.Name, out string mappedFilePath))
                {
                    return AssemblyLoadContext.Default.LoadFromAssemblyPath(mappedFilePath);
                }

                foreach (var directoryProbe in _locationMap.Directories)
                {
                    var filePathProbes = new[] {
                        Path.Combine(directoryProbe, assemblyName.Name + ".dll"),
                        Path.Combine(directoryProbe, assemblyName.Name + ".exe"),
                        Path.Combine(directoryProbe, assemblyName.Name + ".so")
                    };

                    foreach (var filePath in filePathProbes)
                    {
                        if (File.Exists(filePath))
                        {
                            return AssemblyLoadContext.Default.LoadFromAssemblyPath(filePath);
                        }
                    }
                }

                var errorMessage =
                    $"Assembly '{assemblyName.Name}' could not be found in any of probed locations:{Environment.NewLine}" +
                    string.Join(Environment.NewLine, _locationMap.Directories.Select((path, index) => $"[{index + 1}] {path}"));

                throw new FileNotFoundException(errorMessage);
            }
        }
#endif
    }
}
