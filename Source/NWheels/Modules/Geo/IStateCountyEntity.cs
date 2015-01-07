using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Entities;

namespace NWheels.Modules.Geo
{
    public interface IStateCountyEntity : IEntityPartId<int>
    {
        ICountryEntity Country { get; }
        ICountryStateEntity State { get; }
        IList<ICityEntity> Cities { get; }
        IList<IZipCodeEntity> ZipCodes { get; }
    }
}
