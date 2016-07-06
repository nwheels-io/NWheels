#if false

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.DataObjects;
using NWheels.Processing;
using NWheels.UI;

namespace NWheels.Globalization.Locales
{
    [TransactionScript(SupportsInitializeInput = false, SupportsPreview = false)]
    [SecurityCheck.AllowAnonymous]
    public class LocaleEntryExportTx : TransactionScript<Empty.Context, LocaleEntryExportTx.IInput, IQueryable<IApplicationLocaleEntryEntityPart>>
    {
        private readonly IFramework _framework;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public LocaleEntryExportTx(IFramework framework)
        {
            _framework = framework;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of TransactionScript<Context,IInput,IQueryable<IApplicationLocaleEntryEntity>>

        public override IQueryable<IApplicationLocaleEntryEntityPart> Execute(IInput input)
        {
            if (input == null || input.Locale == null)
            {
                return ListLocaleEntriesForUIApplication(localeIsoCode: "EN");
            }

            using (var context = _framework.NewUnitOfWork<IApplicationLocalizationContext>())
            {
                var localeIsoCode = input.Locale.IsoCode;
                
                var resultSet = context.LocaleEntries.AsQueryable()
                    .Where(x => x.LocaleIsoCode == localeIsoCode)
                    .OrderBy(x => x.EntryId)
                    .ToList();

                if (resultSet.Count == 0 && UIOperationContext.Current != null)
                {
                    return ListLocaleEntriesForUIApplication(localeIsoCode);
                }

                return resultSet.AsQueryable();
            }
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private IQueryable<IApplicationLocaleEntryEntityPart> ListLocaleEntriesForUIApplication(string localeIsoCode)
        {
            var locale = UIOperationContext.Current.AppContext.Uidl.Locales.Values.First();
            var entries = locale.Translations
                .Select(kvp => {
                    var entry = _framework.NewDomainObject<IApplicationLocaleEntryEntityPart>();
                    entry.EntryId = kvp.Key;
                    entry.LocaleIsoCode = localeIsoCode;
                    entry.Translation = kvp.Value;
                    return entry;
                })
                .OrderBy(e => e.EntryId)
                .ToList();

            return entries.AsQueryable();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [ViewModelContract]
        public interface IInput
        {
            IApplicationLocaleEntity Locale { get; set; }
        }
    }
}

#endif

