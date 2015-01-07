using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Entities;

namespace NWheels.Modules.Geo
{
    public interface ICountryStateEntity : IEntityPartId<int>
    {
        ICountryEntity Country { get; }
        IList<IStateCountyEntity> Counties { get; }
        string Name { get; set; }
        string Abbreviation { get; set; }
        IList<ICityEntity> Cities { get; }
        IList<IZipCodeEntity> ZipCodes { get; }
    }
}
