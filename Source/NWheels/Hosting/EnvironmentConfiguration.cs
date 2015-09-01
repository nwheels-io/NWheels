using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Hosting
{
    [DataContract(Namespace = "NWheels.Hosting", Name = "Environment.Config")]
    public class EnvironmentConfiguration
    {
        public const string DefaultEnvironmentConfigFileName = "environment.config";

        public static EnvironmentConfiguration LoadFromFile(string filePath)
        {
            if ( File.Exists(filePath) == false )
            {
                return null;
            }

            using ( var file = File.OpenRead(filePath) )
            {
                var serializer = new DataContractSerializer(typeof(EnvironmentConfiguration));
                var config = (EnvironmentConfiguration)serializer.ReadObject(file);

                return config;
            }
        }

        [DataMember(Order = 1, IsRequired = true)]
        public List<Environment> Environments { get; set; }

        [DataContract(Namespace = "NWheels.Hosting", Name = "Environment")]
        public class Environment
        {
            [DataMember(Order = 1, IsRequired = true)]
            public string MachineName { get; set; }

            [DataMember(Order = 2, IsRequired = true)]
            public string Name { get; set; }

            [DataMember(Order = 3, IsRequired = true)]
            public string Type { get; set; }
        }
    }
}
