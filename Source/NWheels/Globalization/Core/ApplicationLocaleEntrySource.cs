using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using NWheels.Core;
using NWheels.DataObjects;
using NWheels.Extensions;
using NWheels.UI;
using NWheels.UI.Uidl;

namespace NWheels.Globalization.Core
{
    public abstract class ApplicationLocaleEntrySource
    {
        public abstract LocaleEntryKey[] GetAllEntryKeys();

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static HashSet<LocaleEntryKey> GetKeysFromAllRegisteredSources(IFramework framework)
        {
            var components = framework.As<ICoreFramework>().Components;
            var allSources = components.Resolve<IEnumerable<ApplicationLocaleEntrySource>>();
            var allKeys = new HashSet<LocaleEntryKey>();

            foreach (var source in allSources)
            {
                allKeys.UnionWith(source.GetAllEntryKeys());
            }

            return allKeys;
        }
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

        public override LocaleEntryKey[] GetAllEntryKeys()
        {
            var application = (UidlApplication)_components.Resolve(_uidlApplicationType);
            var uidl = UidlBuilder.GetApplicationDocument(application, _metadataCache, _localizationProvider, _components);

            return uidl.GetTranslatables().Where(t => !string.IsNullOrEmpty(t.StringId)).ToArray();
        }

        #endregion
    }
}
