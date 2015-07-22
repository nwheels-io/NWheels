#if false

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Hapil;
using NWheels.Exceptions;
using NWheels.Utilities;
using System.IO;

namespace NWheels.Hosting
{
    [DataContract(Namespace = "NWheels.Hosting", Name = "Application.Config")]
    public class ApplicationConfiguration 
    {
        public const string DefaultApplicationConfigFileName = "application.config";

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Validate()
        {
            if ( string.IsNullOrEmpty(ApplicationName) )
            {
                throw new NodeHostConfigException("ApplicatioName is not specified");
            }

            if ( Environments == null )
            {
                Environments = new List<EnvironmentConfig>();
            }
            else
            {
                Environments.ForEach(ValidateEnvironment);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string ToLogString()
        {
            var text = new StringBuilder();

            text.AppendFormat("Application Name   - {0}", this.ApplicationName);
            text.AppendLine();

            foreach ( var environment in this.Environments )
            {
                text.AppendFormat("+ Environment - {0} : type '{1}'", environment.Name, environment.Type);
                NodeListToLogString(environment.Nodes, text);
                text.AppendLine();
            }

            return text.ToString();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void NodeListToLogString(List<BootConfiguration> nodeList, StringBuilder text)
        {
            foreach ( var node in nodeList )
            {
                text.AppendLine();
                text.AppendFormat(node.ToLogString());
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataMember(Order = 1, Name = "Application", IsRequired = true)]
        public string ApplicationName { get; set; }

        [DataMember(Order = 2, IsRequired = false, EmitDefaultValue = false)]
        public List<EnvironmentConfig> Environments { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string LoadedFromDirectory { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static ApplicationConfiguration LoadFromFile(string filePath)
        {
            using ( var file = File.OpenRead(filePath) )
            {
                var serializer = new DataContractSerializer(typeof(ApplicationConfiguration));
                var config = (ApplicationConfiguration)serializer.ReadObject(file);

                config.LoadedFromDirectory = Path.GetDirectoryName(filePath);

                return config;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static void ValidateEnvironment(EnvironmentConfig environment)
        {
            if ( string.IsNullOrEmpty(environment.Name) )
            {
                throw new NodeHostConfigException("Each Environment must have Name specified.");
            }

            if ( string.IsNullOrEmpty(environment.Type) )
            {
                throw new NodeHostConfigException("Each Environment must have Type specified.");
            }

            if ( environment.Nodes == null )
            {
                environment.Nodes = new List<BootConfiguration>();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataContract(Namespace = "NWheels.Hosting", Name = "Environment")]
        public class EnvironmentConfig
        {
            [DataMember(Order = 1, IsRequired = false, EmitDefaultValue = false)]
            public string Name { get; set; }

            [DataMember(Order = 2, IsRequired = false, EmitDefaultValue = false)]
            public string Type { get; set; }

            [DataMember(Order = 3, IsRequired = false, EmitDefaultValue = false)]
            public List<BootConfiguration> Nodes { get; set; }
        }
    }
}

#endif