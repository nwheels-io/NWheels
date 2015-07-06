using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Entities.Core
{
    public interface IStorageInitializer
    {
        bool StorageSchemaExists(string connectionString);
        void MigrateStorageSchema(string connectionString);
        void CreateStorageSchema(string connectionString);
        void DropStorageSchema(string connectionString);
    }
}
