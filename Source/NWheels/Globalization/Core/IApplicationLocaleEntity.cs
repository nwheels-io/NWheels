using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using NWheels.Core;
using NWheels.DataObjects;
using NWheels.Entities;
using NWheels.Extensions;

namespace NWheels.Globalization.Core
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
        DateTime LastBulkTranstale { get; set; }

        IList<IApplicationLocaleEntryEntityPart> Entries { get; }

        void UpdateEntries();
        void BulkTranslate(IList<LocaleEntryKey> keys, IList<string> translations);
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
        public abstract DateTime LastBulkTranstale { get; set; }
        public abstract IList<IApplicationLocaleEntryEntityPart> Entries { get; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        public virtual void UpdateEntries()
        {
            var allKeysInUse = ApplicationLocaleEntrySource.GetKeysFromAllRegisteredSources(Framework);
            var currentEntries = this.Entries.ToDictionary(e => new LocaleEntryKey(e.StringId, e.Origin));

            HandleRemovedEntries(currentEntries, allKeysInUse);
            HandleAddedEntries(currentEntries, allKeysInUse);
            RebuildEntriesList(currentEntries);

            this.LastEntriesUpdate = Framework.UtcNow;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual void BulkTranslate(IList<LocaleEntryKey> keys, IList<string> translations)
        {
            var currentEntries = this.Entries.ToDictionary(e => new LocaleEntryKey(e.StringId, e.Origin));

            for (int i = 0 ; i < keys.Count ; i++)
            {
                var entry = currentEntries.GetOrAdd(keys[i], CreateNewLocaleEntry);
                entry.Translation = translations[i];
            }

            RebuildEntriesList(currentEntries);

            this.LastBulkTranstale = Framework.UtcNow;
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

        private void HandleAddedEntries(
            Dictionary<LocaleEntryKey, IApplicationLocaleEntryEntityPart> currentEntries,
            IEnumerable<LocaleEntryKey> allKeysInUse)
        {
            var addedKeys = new HashSet<LocaleEntryKey>(allKeysInUse);
            addedKeys.ExceptWith(currentEntries.Keys);

            var originFallbackAddedKeys = addedKeys
                .GroupBy(key => key.StringId)
                .Select(g => new LocaleEntryKey(g.Key, origin: null))
                .ToArray();

            foreach (var key in originFallbackAddedKeys)
            {
                var newEntry = CreateNewLocaleEntry(key);
                currentEntries.Add(key, newEntry);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private IApplicationLocaleEntryEntityPart CreateNewLocaleEntry(LocaleEntryKey key)
        {
            var newEntry = Framework.NewDomainObject<IApplicationLocaleEntryEntityPart>();

            newEntry.StringId = key.StringId;
            newEntry.Origin = key.Origin;
            newEntry.Translation = key.StringId.SplitPascalCase();
            
            return newEntry;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void RebuildEntriesList(Dictionary<LocaleEntryKey, IApplicationLocaleEntryEntityPart> currentEntries)
        {
            this.Entries.Clear();

            foreach (var entryPair in currentEntries.OrderBy(e => e.Key.ToString()))
            {
                this.Entries.Add(entryPair.Value);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void HandleRemovedEntries(
            Dictionary<LocaleEntryKey, IApplicationLocaleEntryEntityPart> currentEntries, 
            IEnumerable<LocaleEntryKey> allKeysInUse)
        {
            var removedKeys = new HashSet<LocaleEntryKey>(currentEntries.Keys);
            removedKeys.ExceptWith(allKeysInUse);

            foreach (var key in removedKeys)
            {
                currentEntries.Remove(key);
            }
        }
    }
}
