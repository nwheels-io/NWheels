using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Entities.Migrations
{
    public abstract class StorageScriptMigration : MigrationBase
    {
        protected StorageScriptMigration(string script)
        {
            this.Script = script;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string Script { get; private set; }
    }
}
