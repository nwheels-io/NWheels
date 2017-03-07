using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

namespace NWheels.Microservices
{
    public class RealModuleLoader : ModuleLoaderBase
    {
        private readonly BootConfiguration _config;

        public RealModuleLoader(BootConfiguration config)
        {
            _config = config;
        }

        public override List<IFeatureLoader> LoadAllFeatures()
        {
            var featureLoaders = GetFeatureLoadersByModuleConfigs(_config.MicroserviceConfig.ApplicationModules);
            featureLoaders.AddRange(GetFeatureLoadersByModuleConfigs(_config.MicroserviceConfig.FrameworkModules));

            return featureLoaders;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        private List<IFeatureLoader> GetFeatureLoadersByModuleConfigs(MicroserviceConfig.ModuleConfig[] configs)
        {
            var types = new List<Type>();

            foreach (var moduleConfig in configs)
            {
                var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(Path.Combine(
                    _config.ModulesDirectory,
                    $"{moduleConfig.Assembly}.dll"));

                var featureLoaderTypes = assembly.GetTypes().Where(
                    x => x.GetTypeInfo().ImplementedInterfaces.Any(i => i == typeof(IFeatureLoader))).ToList();

                types.AddRange(featureLoaderTypes.Where(x => x.GetTypeInfo().IsDefined(typeof(DefaultFeatureLoaderAttribute))).ToList());

                foreach (var featueConfig in moduleConfig.Features)
                {
                    var type = GetTypeByFeatureLoaderConfig(featureLoaderTypes, featueConfig);
                    if (type == null)
                    {
                        throw new Exception("Feature wasn't found.");
                    }
                    else
                    {
                        types.Add(type);
                    }
                }
            }

            return types.Distinct().Select(x => (IFeatureLoader)Activator.CreateInstance(x)).ToList();
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        private Type GetTypeByFeatureLoaderConfig(List<Type> featureLoaderTypes, MicroserviceConfig.ModuleConfig.FeatureConfig featureConfig)
        {
            var duplicationException = new Exception($"Found more than one FeatureLoader for {featureConfig.Name} config name.");
            Type type = null;
            foreach (var featureType in featureLoaderTypes)
            {
                var typeInfo = featureType.GetTypeInfo();
                if (typeInfo.Name == featureConfig.Name)
                {
                    if (type != null)
                    {
                        throw duplicationException;
                    }
                    type = featureType;
                }
                else
                {
                    var featureAttribute = typeInfo.GetCustomAttribute<FeatureLoaderAttribute>();
                    if (featureAttribute != null && featureAttribute.Name == featureConfig.Name)
                    {
                        if (type != null)
                        {
                            throw duplicationException;
                        }
                        type = featureType;
                    }
                }
            }
            return type;
        }
    }
}