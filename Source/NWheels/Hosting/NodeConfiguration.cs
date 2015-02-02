using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using NWheels.Exceptions;
using NWheels.Utilities;
using System.IO;

namespace NWheels.Hosting
{
    [DataContract(Namespace = "NWheels.Hosting")]
    public class NodeConfiguration : INodeConfiguration
    {
        public const string DefaultNodeConfigFileName = "node.config";
        public const string DefaultModuleConfigFileName = "module.config";

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Validate()
        {
            if ( string.IsNullOrEmpty(ApplicationName) )
            {
                throw new NodeHostConfigException("ApplicatioName is not specified");
            }

            if ( string.IsNullOrEmpty(NodeName) )
            {
                throw new NodeHostConfigException("NodeName is not specified");
            }

            if ( FrameworkModules == null )
            {
                FrameworkModules = new List<ModuleConfig>();
            }
            else
            {
                FrameworkModules.ForEach(ValidateModule);
            }

            if ( ApplicationModules == null )
            {
                ApplicationModules = new List<ModuleConfig>();
            }
            else
            {
                ApplicationModules.ForEach(ValidateModule);
            }

            if ( ConfigFiles == null )
            {
                ConfigFiles = new List<ConfigFile>();
            }
            else
            {
                ConfigFiles.ForEach(f => ValidateConfigFile(this.LoadedFromDirectory, f));
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string ToLogString()
        {
            var text = new StringBuilder();

            text.AppendFormat("Application Name   - {0}", this.ApplicationName);
            text.AppendLine();
            text.AppendFormat("Node Name          - {0}", this.NodeName);
            text.AppendLine();
            text.AppendFormat("Environment Name   - {0}", this.EnvironmentName);
            text.AppendLine();
            text.AppendFormat("Environment Type   - {0}", string.IsNullOrEmpty(this.EnvironmentType) ? "(unspecified)" : this.EnvironmentType);
            text.AppendLine();

            foreach ( var module in this.FrameworkModules )
            {
                text.AppendFormat("+ Framework Module   - {0}", module.Assembly);
                text.AppendLine();
            }

            foreach ( var module in this.ApplicationModules )
            {
                text.AppendFormat("+ Application Module - {0}", module.Assembly);
                text.AppendLine();
            }

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
        public List<ConfigFile> ConfigFiles { get; set; }


        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string InstanceId { get; set; }
        public string LoadedFromDirectory { get; private set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static NodeConfiguration LoadFromFile(string filePath)
        {
            using ( var file = File.OpenRead(filePath) )
            {
                var serializer = new DataContractSerializer(typeof(NodeConfiguration));
                var config = (NodeConfiguration)serializer.ReadObject(file);
                
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
                throw new NodeHostConfigException("Config file does not exist: " + file.Path);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataContract(Namespace = "NWheels.Hosting", Name = "Module")]
        public class ModuleConfig
        {
            [DataMember(Order = 1, IsRequired = false, EmitDefaultValue = false)]
            public string Name { get; set; }
            [DataMember(Order = 2, IsRequired = true)]
            public string Assembly { get; set; }
            [DataMember(Order = 2, IsRequired = false, EmitDefaultValue = false)]
            public string LoaderClass { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataContract(Namespace = "NWheels.Hosting", Name = "File")]
        public class ConfigFile
        {
            [DataMember(Order = 1, IsRequired = false, EmitDefaultValue = false)]
            public string Path { get; set; }
        }

    }
}
