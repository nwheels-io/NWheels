using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Entities.Migrations
{
    public abstract class CodeMigration : MigrationBase
    {
        #region Overrides of MigrationBase

        public override void Execute(string connectionString, object connectionObject)
        {

        }

        #endregion
    }
}
