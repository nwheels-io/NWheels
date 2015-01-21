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
                FrameworkModules = new string[0];
            }

            if ( ApplicationModules == null )
            {
                ApplicationModules = new string[0];
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataMember]
        public string ApplicationName { get; set; }
        [DataMember]
        public string NodeName { get; set; }
        [DataMember]
        public IList<string> FrameworkModules { get; set; }
        [DataMember]
        public IList<string> ApplicationModules { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static NodeHostConfig LoadFromFile(string filePath)
        {
            using ( var file = File.OpenRead(filePath) )
            {
                var serializer = new DataContractSerializer(typeof(NodeHostConfig));
                return (NodeHostConfig)serializer.ReadObject(file);
            }
        }
    }
}
