using NWheels.Entities;

namespace NWheels.Globalization.Core
{
    public interface IApplicationLocalizationContext : IApplicationDataRepository
    {
        IEntityRepository<IApplicationLocaleEntity> Locales { get; }
    }
}
