using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using NWheels.DataObjects;
using NWheels.UI;
using NWheels.UI.Uidl;

namespace NWheels.Globalization.Locales
{
    public abstract class ApplicationLocaleEntrySource
    {
        public abstract string[] GetAllEntryKeys();
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class UidlApplicationLocaleEntrySource : ApplicationLocaleEntrySource
    {
        private readonly IComponentContext _components;
        private readonly ITypeMetadataCache _metadataCache;
        private readonly ILocalizationProvider _localizationProvider;
        private readonly Type _uidlApplicationType;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public UidlApplicationLocaleEntrySource(
            IComponentContext components, 
            ITypeMetadataCache metadataCache, 
            ILocalizationProvider localizationProvider,
            Type uidlApplicationType)
        {
            _components = components;
            _metadataCache = metadataCache;
            _localizationProvider = localizationProvider;
            _uidlApplicationType = uidlApplicationType;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of ApplicationLocaleEntrySource

        public override string[] GetAllEntryKeys()
        {
            var application = (UidlApplication)_components.Resolve(_uidlApplicationType);
            var uidl = UidlBuilder.GetApplicationDocument(application, _metadataCache, _localizationProvider, _components);

            var firstLocale = uidl.Locales.Values.First();
            return firstLocale.Translations.Keys.ToArray();
        }

        #endregion
    }
}
