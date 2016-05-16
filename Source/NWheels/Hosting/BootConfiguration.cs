using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Text;
using Hapil;
using NWheels.Exceptions;
using System.IO;
using System.Linq;
using NWheels.Configuration.Core;
using NWheels.Extensions;

namespace NWheels.Hosting
{
    [DataContract(Namespace = "NWheels.Hosting", Name = "Boot.Config")]
    public class BootConfiguration : INodeConfiguration
    {
        public const string DefaultBootConfigFileName = "boot.config";
        public const string DefaultModuleConfigFileName = "module.config";

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private EnvironmentConfiguration.LookupResult _environmentLookupLog = EnvironmentConfiguration.LookupResult.NotFound;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Validate()
        {
            LoadEnvironmentConfig();

            if ( string.IsNullOrEmpty(ApplicationName) )
            {
                throw new NodeHostConfigException("ApplicatioName is not specified");
            }

            if ( string.IsNullOrEmpty(NodeName) )
            {
                throw new NodeHostConfigException("NodeName is not specified");
            }

            FrameworkModules = ValidateModuleList(FrameworkModules);
            ApplicationModules = ValidateModuleList(ApplicationModules);
            IntegrationModules = ValidateModuleList(IntegrationModules);
            
            ValidateConfigFiles();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string ToLogString()
        {
            var text = new StringBuilder();

            text.AppendLine();
            text.AppendFormat("Boot configuration   - {0}", this.LoadedFromFile ?? "(not from a file source)");
            text.AppendLine();
            text.AppendFormat("Application Name     - {0}", this.ApplicationName);
            text.AppendLine();
            text.AppendFormat("Node Name            - {0}", this.NodeName);
            text.AppendLine();
            text.AppendFormat("Environment Lookup   - {0}", _environmentLookupLog);
            text.AppendLine();
            text.AppendFormat("Environment Name     - {0}", this.EnvironmentName);
            text.AppendLine();
            text.AppendFormat("Environment Type     - {0}", string.IsNullOrEmpty(this.EnvironmentType) ? "(unspecified)" : this.EnvironmentType);
            text.AppendLine();
            text.AppendFormat("MachineName          - {0}", _s_machineName);
            text.AppendLine();
            text.AppendFormat("Process ID           - {0}", Process.GetCurrentProcess().Id);
            text.AppendLine();

            ModuleListToLogString(this.FrameworkModules, "Framework Module", text);
            ModuleListToLogString(this.ApplicationModules, "Application Module", text);
            ModuleListToLogString(this.IntegrationModules, "Integration Module", text);

            foreach ( var configFile in this.ConfigFiles )
            {
                text.AppendFormat("+ Configuration File - {0}", configFile.Path);
                text.AppendLine();
            }

            return text.ToString();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataMember(Order = 1, Name = "Application", IsRequired = true)]
        public string ApplicationName { get; set; }

        [DataMember(Order = 2, Name = "Node", IsRequired = true)]
        public string NodeName { get; set; }

        [DataMember(Order = 3, Name = "Environment", IsRequired = true)]
        public string EnvironmentName { get; set; }

        [DataMember(Order = 4, IsRequired = false, EmitDefaultValue = false)]
        public string EnvironmentType { get; set; }

        [DataMember(Order = 5, IsRequired = false, EmitDefaultValue = false)]
        public List<ModuleConfig> FrameworkModules { get; set; }

        [DataMember(Order = 6, IsRequired = false, EmitDefaultValue = false)]
        public List<ModuleConfig> ApplicationModules { get; set; }

        [DataMember(Order = 7, IsRequired = false, EmitDefaultValue = false)]
        public List<ModuleConfig> IntegrationModules { get; set; }

        [DataMember(Order = 8, IsRequired = false, EmitDefaultValue = false)]
        public List<ConfigFile> ConfigFiles { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string MachineName
        {
            get { return _s_machineName; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string InstanceId { get; set; }
        public string LoadedFromDirectory { get; set; }
        public string LoadedFromFile { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void LoadEnvironmentConfig()
        {
            var envConfigFilePath = Path.Combine(LoadedFromDirectory, EnvironmentConfiguration.DefaultEnvironmentConfigFileName);
            var envConfigFile = EnvironmentConfiguration.LoadFromFile(envConfigFilePath);

            if (envConfigFile != null)
            {
                EnvironmentConfiguration.Environment environment;
                _environmentLookupLog = envConfigFile.TryGetEnvironment(_s_machineName, this.LoadedFromDirectory, out environment);

                if (environment != null && _environmentLookupLog != EnvironmentConfiguration.LookupResult.NotFound)
                {
                    this.EnvironmentName = environment.Name;
                    this.EnvironmentType = environment.Type;
                }
            }
            else
            {
                _environmentLookupLog = EnvironmentConfiguration.LookupResult.EnvironmentConfigFileDoesNotExist;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private List<ModuleConfig> ValidateModuleList(List<ModuleConfig> configuredList)
        {
            List<ModuleConfig> validatedList;

            if (configuredList == null)
            {
                validatedList = new List<ModuleConfig>();
            }
            else
            {
                validatedList = configuredList.Where(module => module.ShouldLoad(node: this)).ToList();
                validatedList.ForEach(ValidateModule);
            }

            return validatedList;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void ValidateConfigFiles()
        {
            if (ConfigFiles == null)
            {
                ConfigFiles = new List<ConfigFile>();
            }
            else
            {
                ConfigFiles.ForEach(f => ValidateConfigFile(this.LoadedFromDirectory, f));
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void ModuleListToLogString(List<ModuleConfig> moduleList, string title, StringBuilder text)
        {
            foreach (var module in moduleList)
            {
                text.AppendFormat("+ {0,-18} - {1}", title, module.Assembly);
                FeatureListToLogString(module.Features, text);
                text.AppendLine();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void FeatureListToLogString(List<FeatureConfig> featureList, StringBuilder text)
        {
            foreach (var feature in featureList)
            {
                text.AppendLine();
                text.AppendFormat("  + Feature            + {0}", feature.Name);
            }
        }
        
        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static readonly string _s_machineName = System.Environment.MachineName;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static BootConfiguration LoadFromFile(string filePath)
        {
            using ( var file = File.OpenRead(filePath) )
            {
                var serializer = new DataContractSerializer(typeof(BootConfiguration));
                var config = (BootConfiguration)serializer.ReadObject(file);

                config.LoadedFromFile = filePath;
                config.LoadedFromDirectory = Path.GetDirectoryName(filePath);

                return config;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static void ValidateModule(ModuleConfig module)
        {
            if ( string.IsNullOrEmpty(module.Assembly) )
            {
                throw new NodeHostConfigException("Each module must have Assembly specified.");
            }

            if ( string.IsNullOrEmpty(module.Name) )
            {
                module.Name = Path.GetFileNameWithoutExtension(module.Assembly);
            }

            if ( string.IsNullOrEmpty(module.LoaderClass) )
            {
                module.LoaderClass = string.Format("{0}.ModuleLoader", Path.GetFileNameWithoutExtension(module.Assembly));
            }

            if ( module.Features == null )
            {
                module.Features = new List<FeatureConfig>();
            }
            else
            {
                module.Features.ForEach(feature => ValidateFeature(module, feature));
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static void ValidateFeature(ModuleConfig module, FeatureConfig feature)
        {
            if ( string.IsNullOrWhiteSpace(feature.Name) && string.IsNullOrWhiteSpace(feature.LoaderClass) )
            {
                throw new NodeHostConfigException("At least one of Name or LoaderClass must be specified in Feature element.");
            }

            if ( string.IsNullOrWhiteSpace(feature.LoaderClass) )
            {
                feature.LoaderClass = string.Format("{0}.{1}FeatureLoader", Path.GetFileNameWithoutExtension(module.Assembly), feature.Name);
            }

            if ( string.IsNullOrWhiteSpace(feature.Name) )
            {
                feature.Name = feature.LoaderClass.TrimPrefix(Path.GetFileNameWithoutExtension(module.Assembly) + ".").TrimSuffix("FeatureLoader");
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static void ValidateConfigFile(string baseDirectory, ConfigFile file)
        {
            if ( string.IsNullOrEmpty(file.Path) )
            {
                throw new NodeHostConfigException("ConfigFile element must specify path to config file in the Path element.");
            }

            if ( !Path.IsPathRooted(file.Path) )
            {
                file.Path = Path.Combine(baseDirectory, file.Path);
            }

            if ( !File.Exists(file.Path) )
            {
                if ( file.IsOptional )
                {
                    file.IsOptionalAndMissing = true;
                }
                else
                {
                    throw new NodeHostConfigException("Config file does not exist: " + file.Path);
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataContract(Namespace = "NWheels.Hosting", Name = "Module")]
        public class ModuleConfig
        {
            public bool ShouldLoad(INodeConfiguration node)
            {
                if (this.Condition != null)
                {
                    return this.Condition.Evaluate(node);
                }

                return true;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            [DataMember(Order = 1, IsRequired = false, EmitDefaultValue = false)]
            public string Name { get; set; }

            [DataMember(Order = 2, IsRequired = false, EmitDefaultValue = false)]
            public ConditionConfig Condition { get; set; }

            [DataMember(Order = 3, IsRequired = true)]
            public string Assembly { get; set; }

            [DataMember(Order = 4, IsRequired = false, EmitDefaultValue = false)]
            public string LoaderClass { get; set; }

            [DataMember(Order = 5, IsRequired = false, EmitDefaultValue = false)]
            public List<FeatureConfig> Features { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataContract(Namespace = "NWheels.Hosting", Name = "Condition")]
        public class ConditionConfig
        {
            public bool Evaluate(INodeConfiguration node)
            {
                if (!XmlConfigurationLoader.Match(node.EnvironmentName, this.EnvironmentNames))
                {
                    return false;
                }

                if (!XmlConfigurationLoader.Match(node.EnvironmentType, this.EnvironmentTypes))
                {
                    return false;
                }

                return true;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            [DataMember(Order = 1, IsRequired = false, EmitDefaultValue = false)]
            public string EnvironmentTypes { get; set; }

            [DataMember(Order = 2, IsRequired = false, EmitDefaultValue = false)]
            public string EnvironmentNames { get; set; }
        }


        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataContract(Namespace = "NWheels.Hosting", Name = "Feature")]
        public class FeatureConfig
        {
            [DataMember(Order = 1, IsRequired = false, EmitDefaultValue = false)]
            public string Name { get; set; }

            [DataMember(Order = 2, IsRequired = false, EmitDefaultValue = false)]
            public string LoaderClass { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataContract(Namespace = "NWheels.Hosting", Name = "File")]
        public class ConfigFile
        {
            [DataMember(Order = 1, IsRequired = false, EmitDefaultValue = false)]
            public string Path { get; set; }

            [DataMember(Order = 2, IsRequired = false, EmitDefaultValue = false)]
            public bool IsOptional { get; set; }

            public bool IsOptionalAndMissing { get; set; }
        }
    }
}
