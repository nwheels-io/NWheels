using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Entities.Migrations
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class SchemaVersionAttribute : Attribute
    {
        public SchemaVersionAttribute(int version)
        {
            this.Version = version;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public int Version { get; private set; }
    }
}
