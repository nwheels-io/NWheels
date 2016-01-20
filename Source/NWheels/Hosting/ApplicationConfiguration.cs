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
                //Environments.ForEach(ValidateEnvironment);
            }

            if ( Nodes == null )
            {
                Nodes = new List<NodeConfig>();
            }
            else
            {
                //Nodes.ForEach(ValidateNode);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string ToLogString()
        {
            return ApplicationName;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataMember(Order = 101, IsRequired = true)]
        public string ApplicationName { get; set; }

        [DataMember(Order = 102, IsRequired = true)]
        public List<NodeConfig> Nodes { get; set; }

        [DataMember(Order = 103, IsRequired = true)]
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

        [DataContract(Namespace = "NWheels.Hosting", Name = "Environment")]
        public class EnvironmentConfig
        {
            [DataMember(Order = 101, IsRequired = true)]
            public string Name { get; set; }

            [DataMember(Order = 102, IsRequired = true)]
            public string Type { get; set; }

            [DataMember(Order = 103, IsRequired = true)]
            public List<MachineConfig> Machines { get; set; }

            [DataMember(Order = 104, IsRequired = true)]
            public List<EnvironmentNodeConstraint> Constraints { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataContract(Namespace = "NWheels.Hosting", Name = "Node")]
        public class NodeConfig
        {
            [DataMember(Order = 101, IsRequired = true)]
            public string Name { get; set; }

            [DataMember(Order = 102, IsRequired = true)]
            public BootConfiguration Boot { get; set; }

            [DataMember(Order = 103, IsRequired = true)]
            public List<string> MachineRoles { get; set; } // look for intersection with MachineConfig.Roles
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataContract(Namespace = "NWheels.Hosting", Name = "Machine")]
        public class MachineConfig
        {
            [DataMember(Order = 1, IsRequired = true)]
            public string DnsName { get; set; }

            [DataMember(Order = 2, IsRequired = true)]
            public string IPAddress { get; set; }

            [DataMember(Order = 3, IsRequired = true)]
            public List<string> Roles { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataContract(Namespace = "NWheels.Hosting", Name = "Constraint")]
        public class EnvironmentNodeConstraint
        {
            [DataMember(Order = 101, IsRequired = true)]
            public string NodeName { get; set; }

            [DataMember(Order = 102, IsRequired = true)]
            public int? InstanceCount { get; set; } // mutually exclusive with InstanceIds

            [DataMember(Order = 103, IsRequired = true)]
            public List<string> InstanceIds { get; set; } // mutually exclusive with InstanceCount

            [DataMember(Order = 104, IsRequired = true)]
            public int? StandbyCountPerActiveInstance { get; set; } // number of failover standby instances per active instance

            [DataMember(Order = 106, IsRequired = true)]
            public int? MaxActiveInstancesPerMachine { get; set; }

            [DataMember(Order = 105, IsRequired = true)]
            public int? MaxTotalInstancesPerMachine { get; set; }
        }
    }
}
