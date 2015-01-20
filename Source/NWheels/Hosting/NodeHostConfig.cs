using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Hosting
{
    [DataContract(Namespace = "NWheels.Hosting")]
    public class NodeHostConfig
    {
        [DataMember]
        public string ApplicationName { get; set; }
        [DataMember]
        public string NodeName { get; set; }
        [DataMember]
        public IList<string> FrameworkModules { get; set; }
        [DataMember]
        public IList<string> ApplicationModules { get; set; }
    }
}
