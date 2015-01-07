using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Entities;

namespace NWheels.Modules.Geo
{
    public interface ICityZipCodeEntity : IEntityPartId<int>
    {
        ICityEntity City { get; set; }
        IZipCodeEntity ZipCode { get; set; }
        bool IsPrimary { get; set; }
    }
}
