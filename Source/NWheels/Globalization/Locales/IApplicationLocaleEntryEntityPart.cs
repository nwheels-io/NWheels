using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.DataObjects;
using NWheels.Entities;

namespace NWheels.Globalization.Locales
{
    [EntityPartContract]
    public interface IApplicationLocaleEntryEntityPart
    {
        [PropertyContract.Required]
        string EntryId { get; set; }

        [PropertyContract.Required]
        string Translation { get; set; }
    }
}
