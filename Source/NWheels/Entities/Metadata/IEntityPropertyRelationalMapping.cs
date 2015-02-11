using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Entities.Metadata
{
    public interface IEntityPropertyRelationalMapping
    {
        string TableName { get; }
        string ColumnName { get; }
        string DataTypeName { get; }
    }
}
