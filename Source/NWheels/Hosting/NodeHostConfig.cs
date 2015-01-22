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
    public class NodeHostConfig
    {
        public const string DefaultFileName = "nodehost.config";

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
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataMember(Order = 1, IsRequired = true)]
        public string ApplicationName { get; set; }
        [DataMember(Order = 2, IsRequired = true)]
        public string NodeName { get; set; }
        [DataMember(Order = 3, IsRequired = false, EmitDefaultValue = false)]
        public List<ModuleConfig> FrameworkModules { get; set; }
        [DataMember(Order = 4, IsRequired = false, EmitDefaultValue = false)]
        public List<ModuleConfig> ApplicationModules { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static NodeHostConfig LoadFromFile(string filePath)
        {
            using ( var file = File.OpenRead(filePath) )
            {
                var serializer = new DataContractSerializer(typeof(NodeHostConfig));
                return (NodeHostConfig)serializer.ReadObject(file);
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
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataContract(Namespace = "NWheels.Hosting", Name = "Module")]
        public class ModuleConfig
        {
            [DataMember(Order = 1, IsRequired = false, EmitDefaultValue = false)]
            public string Name { get; set; }
            [DataMember(Order = 2, IsRequired = true)]
            public string Assembly { get; set; }
        }
    }
}
