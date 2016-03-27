using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Entities.Migrations;
using NWheels.Exceptions;

namespace NWheels.Entities.Core
{
    public abstract class SchemaMigrationCollection
    {
        public abstract IEnumerable<MigrationBase> GetMigrations();
        public abstract Type DomainContextType { get; }
        public abstract int SchemaVersion { get; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public abstract class SchemaMigrationCollection<TContext> : SchemaMigrationCollection
    {
        private readonly IReadOnlyList<MigrationBase> _migrations;
        private readonly int _schemaVersion;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected SchemaMigrationCollection()
        {
            // ReSharper disable once DoNotCallOverridableMethodsInConstructor
            _migrations = BuildMigrationList();

            ValidateMigrationList(out _schemaVersion);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of SchemaMigrationCollection

        public override IEnumerable<MigrationBase> GetMigrations()
        {
            return _migrations;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override int SchemaVersion
        {
            get
            {
                return _schemaVersion;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override Type DomainContextType
        {
            get { return typeof(TContext); }
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected abstract MigrationBase[] BuildMigrationList();

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        private void ValidateMigrationList(out int lastVersion)
        {
            lastVersion = 0;

            for (int i = 0; i < _migrations.Count; i++)
            {
                var migration = _migrations[i];

                if (migration.SchemaVersion <= lastVersion)
                {
                    throw new ConventionException(
                        "Migration class '{0}' at index '{1}' specifies invalid version '{2}'.",
                        migration.GetType().Name, i, migration.SchemaVersion);
                }

                lastVersion = migration.SchemaVersion;
            }
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class EmptySchemaMigrationCollection : SchemaMigrationCollection
    {
        #region Overrides of SchemaMigrationCollection

        public override IEnumerable<MigrationBase> GetMigrations()
        {
            return new MigrationBase[0];
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override Type DomainContextType
        {
            get { return null; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override int SchemaVersion
        {
            get { return 1; }
        }

        #endregion
    }
}
