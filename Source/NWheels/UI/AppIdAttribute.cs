using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.UI
{
    [AttributeUsage(AttributeTargets.Class)]
    public class AppIdAttribute : Attribute
    {
        public AppIdAttribute(string @namespace, string idName)
            : this(@namespace, idName, version: null)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public AppIdAttribute(string @namespace, string idName, string version)
        {
            this.Namespace = @namespace;
            this.IdName = idName;
            this.Version = version;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string Namespace { get; private set; }
        public string IdName { get; private set; }
        public string Version { get; private set; }
    }
}
