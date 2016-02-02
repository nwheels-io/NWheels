using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Entities.Core
{
    public class DatabaseInitializationCheckRegistration
    {
        public DatabaseInitializationCheckRegistration(Type contextType)
        {
            this.ContextType = contextType;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Type ContextType { get; private set; }
    }
}
