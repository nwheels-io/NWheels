using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Entities;

namespace NWheels.Modules.Geo
{
    public interface IZipCodeEntity : IEntityPartId<int>
    {
        ICountryEntity Country { get; set; }
        ICountryStateEntity State { get; set; }
        IStateCountyEntity County { get; set; }
        IList<ICityZipCodeEntity> Cities { get; }
    }
}
