using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Globalization.Locales
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

        protected override void Initialize(out Dictionary<string, ILocale> localeByCultureName)
        {
            using (var context = _framework.NewUnitOfWork<IApplicationLocalizationContext>())
            {
                var allLocales = context.Locales.AsQueryable().ToList();

                localeByCultureName = allLocales
                    .Select(entity => new DbBasedLocale(entity))
                    .ToDictionary(
                        locale => locale.Culture.Name, 
                        locale => (ILocale)locale);
            }
        }

        #endregion
    }
}
