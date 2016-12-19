using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using NWheels.Entities.Migrations;

namespace NWheels.Stacks.MongoDb
{
    public class MongoScriptMigration : StorageScriptMigration
    {
        public MongoScriptMigration(string script)
            : base(script)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of MigrationBase

        public override void Execute(string connectionString, object connectionObject)
        {
            var db = (MongoDatabase)connectionObject;

            var args = new EvalArgs();
            args.Code = this.Script;

            var resultValue = db.Eval(args);

            //TODO: should check resultValue for errors?
            //TODO: better understand result values from script execution
            //if (resultValue.AsBoolean == true)
            //{
            //}
            //else
            //{
            //    throw new Exception("Failure while executing migration script on the DB.");
            //}
        }

        #endregion
    }
}
