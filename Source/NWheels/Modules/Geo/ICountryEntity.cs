using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Entities;

namespace NWheels.Modules.Geo
{
    public interface ICountryEntity : IEntityPartId<int>
    {
        string Name { get; set; }
        string Abbreviation { get; set; }
        IList<ICountryStateEntity> States { get; }
        IList<IStateCountyEntity> Counties { get; }
        IList<ICityEntity> Cities { get; }
        IList<IZipCodeEntity> ZipCodes { get; }
    }
}
