using NWheels.DataObjects;
using NWheels.UI;
using NWheels.UI.Toolbox;
using NWheels.UI.Uidl;

namespace NWheels.Globalization.Core
{
    public class LocaleCrudScreenPart : CrudScreenPart<IApplicationLocaleEntity>
    {
        public LocaleCrudScreenPart(string idName, UidlApplication parent)
            : base(idName, parent)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public EntityMethodForm<IApplicationLocaleEntity, IUpdateEntriesInput> UpateEntriesMethod { get; set; }
        public UidlCommand UpdateEntries { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of CrudScreenPart<IApplicationLocaleEntity>

        protected override void DescribePresenter(PresenterBuilder<CrudScreenPart<IApplicationLocaleEntity>, Empty.Data, IState> presenter)
        {
            ImportExportEnabled = true;

            base.DescribePresenter(presenter);

            Crud.UpdateCommandsOnSelection = true;

            presenter.On(Crud.SelectedEntityChanged)
                .Broadcast(UpateEntriesMethod.EntitySetter).WithPayload(vm => vm.Input).TunnelDown()
                .Then(b => b.Broadcast(UpateEntriesMethod.EntitySetter).WithPayload(vm => vm.Input).TunnelDown());

            Crud.Grid
                .Column(x => x.IsoCode)
                .Column(x => x.EnglishName)
                .Column(x => x.CultureCode)
                .Column(x => x.EntryCount, size: FieldSize.Small)
                .Column(x => x.LastEntriesUpdate, format: "d")
                .Column(x => x.LastTranslationUpload, format: "d")
                .Column(x => x.LastBulkTranstale, format: "d");

            Crud.Form
                .UseSectionsInsteadOfTabs()
                .ShowFields(
                    x => x.IsoCode, 
                    x => x.EnglishName, 
                    x => x.CultureCode, 
                    x => x.EntryCount, 
                    x => x.LastEntriesUpdate, 
                    x => x.LastTranslationUpload,
                    x => x.LastBulkTranstale)
                .Field(x => x.LastEntriesUpdate, type: FormFieldType.Label)
                .Field(x => x.LastTranslationUpload, type: FormFieldType.Label);

            UpateEntriesMethod.InputForm.TemplateName = "FormAlert";
            UpateEntriesMethod.InputForm.Field(x => x.Warning, type: FormFieldType.Alert, setup: f => {
                f.AlertType = UserAlertType.Warning;
                f.Label = "You are about to update locale entries. Do you want to proceed?";
            });

            UpateEntriesMethod.AttachTo(
                presenter,
                command: UpdateEntries,
                onExecute: (locale, vm) => locale.UpdateEntries());
            
            presenter.On(UpateEntriesMethod.OperationCompleted).Broadcast(Crud.RefreshRequested).TunnelDown();

            Crud.AddEntityCommands(UpdateEntries);
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [ViewModelContract]
        public interface IUpdateEntriesInput
        {
            [PropertyContract.ReadOnly]
            string Warning { get; set; }
        }
    }
}
