using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace NWheels.Globalization.Core
{
    public class DbBasedLocalizationProvider : LocalizationProviderBase
    {
        private readonly IFramework _framework;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public DbBasedLocalizationProvider(IFramework framework)
        {
            _framework = framework;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of LocalizationProviderBase

        protected override void Initialize(out ImmutableDictionary<string, ILocale> localeByCultureName)
        {
            using (var context = _framework.NewUnitOfWork<IApplicationLocalizationContext>())
            {
                var allLocales = context.Locales.AsQueryable().ToList();

                localeByCultureName = allLocales
                    .Select(entity => new DbBasedLocale(entity))
                    .ToImmutableDictionary(
                        locale => locale.Culture.Name, 
                        locale => (ILocale)locale);
            }
        }

        #endregion
    }
}
