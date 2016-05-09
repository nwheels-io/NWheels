using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using NWheels.Extensions;
using NWheels.Utilities;

namespace NWheels.Hosting
{
    [DataContract(Namespace = "NWheels.Hosting", Name = "Environment.Config")]
    public class EnvironmentConfiguration
    {
        public const string DefaultEnvironmentConfigFileName = "environment.config";
        public const string DefaultMachineName = "default";

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public LookupResult TryGetEnvironment(string machineName, string folderPath, out Environment environment)
        {
            foreach (var env in this.Environments)
            {
                var result = env.Match(machineName, folderPath);

                if (result != LookupResult.NotFound)
                {
                    environment = env;
                    return result;
                }
            }

            environment = null;
            return LookupResult.NotFound;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataMember(Order = 1, IsRequired = true)]
        public List<Environment> Environments { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static EnvironmentConfiguration LoadFromFile(string filePath)
        {
            if (File.Exists(filePath) == false)
            {
                return null;
            }

            using (var file = File.OpenRead(filePath))
            {
                var serializer = new DataContractSerializer(typeof(EnvironmentConfiguration));
                var config = (EnvironmentConfiguration)serializer.ReadObject(file);

                return config;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static LookupResult MatchEnvironment(
            string inputMachineName, 
            string inputFolderPath, 
            string environmentMachineName, 
            string environmentDeploymentFolder, 
            IList<DeploymentFolder> environmentDeploymentFolders)
        {
            LookupResult result;

            if (environmentMachineName != null)
            {
                if (!environmentMachineName.EqualsIgnoreCase(inputMachineName))
                {
                    return LookupResult.NotFound;
                }

                result = LookupResult.MatchedByMachine;

                if (environmentDeploymentFolder != null)
                {
                    if (!environmentDeploymentFolder.PathEquals(inputFolderPath))
                    {
                        return LookupResult.NotFound;
                    }

                    result = LookupResult.MatchedByMachineAndFolder;
                }

                if (environmentDeploymentFolders != null && environmentDeploymentFolders.Count > 0)
                {
                    if (!environmentDeploymentFolders.Any(f => f.Path.PathEquals(inputFolderPath)))
                    {
                        return LookupResult.NotFound;
                    }

                    result = LookupResult.MatchedByMachineAndFolder;
                }

                return result;
            }

            return LookupResult.NotFound;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public enum LookupResult
        {
            NotFound,
            MatchedByMachine,
            MatchedByMachineAndFolder,
            MatchedDefaultFallback,
            EnvironmentConfigFileDoesNotExist
        }
        
        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataContract(Namespace = "NWheels.Hosting", Name = "Environment")]
        public class Environment
        {
            public LookupResult Match(string machineName, string folderPath)
            {
                var result = MatchEnvironment(machineName, folderPath, this.MachineName, this.DeploymentFolder, this.DeploymentFolders);

                if (result != LookupResult.NotFound)
                {
                    return result;
                }

                if (this.Machines != null)
                {
                    foreach (var machine in this.Machines)
                    {
                        result = machine.Match(machineName, folderPath, this.DeploymentFolders);
                        
                        if (result != LookupResult.NotFound)
                        {
                            return result;
                        }
                    }
                }

                if (this.MachineName.EqualsIgnoreCase(DefaultMachineName))
                {
                    return LookupResult.MatchedDefaultFallback;
                }

                return LookupResult.NotFound;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            [DataMember(Order = 1, IsRequired = false, EmitDefaultValue = false)]
            public string MachineName { get; set; }

            [DataMember(Order = 2, IsRequired = true)]
            public string Name { get; set; }

            [DataMember(Order = 3, IsRequired = true)]
            public string Type { get; set; }

            [DataMember(Order = 4, IsRequired = false, EmitDefaultValue = false)]
            public IList<Machine> Machines { get; set; }

            [DataMember(Order = 5, IsRequired = false, EmitDefaultValue = false)]
            public string DeploymentFolder { get; set; }

            [DataMember(Order = 6, IsRequired = false, EmitDefaultValue = false)]
            public IList<DeploymentFolder> DeploymentFolders { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataContract(Namespace = "NWheels.Hosting", Name = "Machine")]
        public class Machine
        {
            public LookupResult Match(string machine, string folder, IList<DeploymentFolder> commonDeploymentFolders)
            {
                return MatchEnvironment(
                    machine, 
                    folder, 
                    this.MachineName, 
                    this.DeploymentFolder, 
                    this.DeploymentFolders ?? commonDeploymentFolders);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            [DataMember(Order = 1, IsRequired = true)]
            public string MachineName { get; set; }

            [DataMember(Order = 2, IsRequired = false, EmitDefaultValue = false)]
            public string MachineRole { get; set; }

            [DataMember(Order = 3, IsRequired = false, EmitDefaultValue = false)]
            public bool IsPrimaryInRole { get; set; }

            [DataMember(Order = 4, IsRequired = false, EmitDefaultValue = false)]
            public string DeploymentFolder { get; set; }

            [DataMember(Order = 5, IsRequired = false, EmitDefaultValue = false)]
            public IList<DeploymentFolder> DeploymentFolders { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataContract(Namespace = "NWheels.Hosting", Name = "Folder")]
        public class DeploymentFolder
        {
            [DataMember(Order = 1, IsRequired = true)]
            public string Path { get; set; }
        }
    }
}
