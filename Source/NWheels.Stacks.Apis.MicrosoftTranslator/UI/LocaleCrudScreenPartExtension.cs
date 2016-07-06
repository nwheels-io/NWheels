using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Globalization.Core;
using NWheels.Stacks.Apis.MicrosoftTranslator.Domain;
using NWheels.UI;
using NWheels.UI.Toolbox;
using NWheels.UI.Uidl;

namespace NWheels.Stacks.Apis.MicrosoftTranslator.UI
{
    public class LocaleCrudScreenPartExtension : 
        UidlExtension<LocaleCrudScreenPart, Empty.Data, LocaleCrudScreenPart.IState>
    {
        #region Overrides of UidlScreenPartExtension<LocaleCrudScreenPart,Data,CrudScreenPart<IApplicationLocaleEntity>.IState>

        protected override void ExtendPresenter(
            LocaleCrudScreenPart screenPart, 
            PresenterBuilder<LocaleCrudScreenPart, Empty.Data, CrudScreenPart<IApplicationLocaleEntity>.IState> presenter)
        {
            screenPart.Crud.StaticCommands.Add(BulkTranslate);
            screenPart.PopupContents.Add(BulkTranslateTx);

            BulkTranslateTx.AttachAsPopupTo(presenter, BulkTranslate);
            BulkTranslateTx.UseOutputForm();
            BulkTranslateTx.UseOKCancelNotation(withIcons: true);

            BulkTranslate.Severity = CommandSeverity.Change;
            BulkTranslate.Icon = "gear";

            BulkTranslateTx.InputForm.UseSectionsInsteadOfTabs();
            BulkTranslateTx.InputForm.Field(x => x.TranslateFrom, 
                type: FormFieldType.Lookup, 
                modifiers: FormFieldModifiers.DropDown | FormFieldModifiers.LookupShowSelectNone);
            BulkTranslateTx.InputForm.Field(x => x.TranslateTo, 
                type: FormFieldType.Lookup,
                modifiers: FormFieldModifiers.DropDown | FormFieldModifiers.LookupShowSelectNone);

            BulkTranslateTx.InputForm.Field(x => x.MicrosoftTranslatorServiceAuthentication, setup: f => {
                var authForm = (Form<LocaleBulkTranslateTx.IMicrosoftTranslatorAuthenticationEntityPart>)f.NestedWidget;
                authForm.IsModalPopup = true;
            });

            BulkTranslateTx.OutputForm.IsModalPopup = true;
            BulkTranslateTx.OutputForm.Field(x => x.Details,
                type: FormFieldType.Label,
                modifiers: FormFieldModifiers.Memo | FormFieldModifiers.FlatStyle);
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public UidlCommand BulkTranslate { get; set; }
        
        public TransactionForm<
            LocaleBulkTranslateTx.IInput, 
            LocaleBulkTranslateTx, 
            LocaleBulkTranslateTx.IOutput> BulkTranslateTx { get; set; }
    }
}
