using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using NWheels.Authorization;
using NWheels.Authorization.Core;
using NWheels.Hosting;

namespace NWheels.Globalization.Core
{
    public class DbBasedLocalizationProvider : LocalizationProviderBase
    {
        private readonly IFramework _framework;
        private readonly ISessionManager _sessionManager;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public DbBasedLocalizationProvider(IFramework framework, ISessionManager sessionManager)
        {
            _framework = framework;
            _sessionManager = sessionManager;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of LocalizationProviderBase

        protected override void Initialize(out ImmutableDictionary<string, ILocale> localeByCultureName)
        {
            using (_sessionManager.JoinGlobalSystem())
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
        }

        #endregion
    }
}
