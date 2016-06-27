using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Entities;

namespace NWheels.Globalization.Locales
{
    public interface IApplicationLocalizationContext : IApplicationDataRepository
    {
        IEntityRepository<IApplicationLocaleEntity> Locales { get; }
        IEntityRepository<IApplicationLocaleEntryEntity> LocaleEntries { get; }
    }
}
