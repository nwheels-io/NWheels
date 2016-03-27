using System;
using System.Reflection;
using NWheels.Exceptions;

namespace NWheels.Entities.Migrations
{
    public abstract class MigrationBase
    {
        protected MigrationBase()
        {
            var attribute = this.GetType().GetCustomAttribute<SchemaVersionAttribute>();

            if (attribute != null)
            {
                this.SchemaVersion = attribute.Version;
            }
            else
            {
                throw new ConventionException("Class '{0}' cannot be used as a migration because SchemaVersion attribute was not applied on it.");
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public int SchemaVersion { get; private set; }
    }
}
