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

            foreach (var source in entrySources)
            {
                var allEntryKeys = source.GetAllEntryKeys();

                HandleRemovedEntries(allEntryKeys);
                HandleAddedEntries(allEntryKeys);
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

        private void HandleAddedEntries(string[] entryKeysInUse)
        {
            var addedEntryIds = new HashSet<string>(entryKeysInUse);
            addedEntryIds.ExceptWith(this.Entries.Keys);

            foreach (var addedId in addedEntryIds.OrderBy(id => id))
            {
                this.Entries.Add(addedId, addedId.SplitPascalCase());
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void HandleRemovedEntries(string[] entryKeysInUse)
        {
            //var entryById = this.Entries.Distinct(new EntryIdEqualityComparer()).ToDictionary(e => e.EntryId);
            
            var removedEntryIds = new HashSet<string>(this.Entries.Keys);
            removedEntryIds.ExceptWith(entryKeysInUse);

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
