using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.DataObjects;

namespace NWheels.Entities
{
    [EntityPartContract]
    public interface IEntityPartUniqueDisplayName
    {
        [PropertyContract.Required(AllowEmpty = false),
            PropertyContract.Unique,
            PropertyContract.Validation.MaxLength(100),
            PropertyContract.Semantic.DisplayName]
        string Name { get; set; }
    }
}
