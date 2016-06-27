using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using NWheels.Core;
using NWheels.DataObjects;
using NWheels.Entities;
using NWheels.Extensions;
using NWheels.Processing.Documents;
using NWheels.UI;
using NWheels.UI.Uidl;

namespace NWheels.Globalization.Locales
{
    [EntityContract]
    public interface IApplicationLocaleEntity
    {
        [PropertyContract.EntityId, PropertyContract.Validation.Length(2, 5)]
        string IsoCode { get; set; }

        [PropertyContract.Required, PropertyContract.Semantic.Culture]
        string CultureCode { get; set; }

        [PropertyContract.Required, PropertyContract.Semantic.DisplayName]
        string EnglishName { get; set; }

        [PropertyContract.Calculated]
        int EntryCount { get; }

        DateTime LastTranslationUpload { get; set; }
        DateTime LastEntriesUpdate { get; set; }

        Dictionary<string, string> Entries { get; set; }

        void UpdateEntries();
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public abstract class ApplicationLocaleEntity : IApplicationLocaleEntity
    {
        #region Implementation of IApplicationLocaleEntity

        public abstract string IsoCode { get; set; }
        public abstract string CultureCode { get; set; }
        public abstract string EnglishName { get; set; }
        public abstract DateTime LastTranslationUpload { get; set; }
        public abstract DateTime LastEntriesUpdate { get; set; }
        public abstract Dictionary<string, string> Entries { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        public virtual void UpdateEntries()
        {
            var components = Framework.As<ICoreFramework>().Components;
            var entrySources = components.Resolve<IEnumerable<ApplicationLocaleEntrySource>>();

            var allEntryIdsInUse = new HashSet<string>();

            foreach (var source in entrySources)
            {
                var application = (UidlApplication)components.Resolve(source.UidlApplicationType);
                var uidl = UidlBuilder.GetApplicationDocument(application, MetadataCache, LocalizationProvider, components);

                var defaultLocale = uidl.Locales.Values.First();
                allEntryIdsInUse.UnionWith(defaultLocale.Translations.Keys);

                HandleRemovedEntries(defaultLocale);
                HandleAddedEntries(defaultLocale);
            }

            this.LastEntriesUpdate = Framework.UtcNow;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [EntityImplementation.CalculatedProperty]
        public virtual int EntryCount
        {
            get
            {
                return this.Entries.Count;
            }
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [EntityImplementation.TriggerOnNew]
        protected virtual void TriggerOnNew()
        {
            Entries = new Dictionary<string, string>();
            UpdateEntries();
        }
        
        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        [EntityImplementation.DependencyProperty]
        protected IFramework Framework { get; set; }

        [EntityImplementation.DependencyProperty]
        protected ITypeMetadataCache MetadataCache { get; set; }

        [EntityImplementation.DependencyProperty]
        protected ILocalizationProvider LocalizationProvider { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void HandleAddedEntries(UidlLocale defaultLocale)
        {
            var addedEntryIds = new HashSet<string>(defaultLocale.Translations.Keys);
            addedEntryIds.ExceptWith(this.Entries.Keys);

            foreach (var addedId in addedEntryIds.OrderBy(id => id))
            {
                this.Entries.Add(addedId, addedId.SplitPascalCase());
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void HandleRemovedEntries(UidlLocale defaultLocale)
        {
            //var entryById = this.Entries.Distinct(new EntryIdEqualityComparer()).ToDictionary(e => e.EntryId);
            
            var removedEntryIds = new HashSet<string>(this.Entries.Keys);
            removedEntryIds.ExceptWith(defaultLocale.Translations.Keys);

            foreach (var removedId in removedEntryIds)
            {
                this.Entries.Remove(removedId);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class EntryIdEqualityComparer : IEqualityComparer<IApplicationLocaleEntryEntityPart>
        {
            #region Implementation of IEqualityComparer<in IApplicationLocaleEntryEntityPart>

            public bool Equals(IApplicationLocaleEntryEntityPart x, IApplicationLocaleEntryEntityPart y)
            {
                return x.EntryId.Equals(y.EntryId);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public int GetHashCode(IApplicationLocaleEntryEntityPart obj)
            {
                return obj.EntryId.GetHashCode();
            }

            #endregion
        }
    }
}
