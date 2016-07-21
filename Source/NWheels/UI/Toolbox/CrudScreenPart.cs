using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using NWheels.Processing.Documents;
using NWheels.Processing.Documents.Core;
using NWheels.UI.Impl;
using NWheels.UI.Uidl;

namespace NWheels.UI.Toolbox
{
    public class CrudScreenPart<TEntity> : ScreenPartBase<CrudScreenPart<TEntity>, object, Empty.Data, CrudScreenPart<TEntity>.IState>
        where TEntity : class
    {
        public CrudScreenPart(string idName, UidlApplication parent)
            : base(idName, parent)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void GridColumns(params Expression<Func<TEntity, object>>[] propertySelectors)
        {
            foreach ( var property in propertySelectors )
            {
                Crud.Grid.Column<object>(property);
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void DescribePresenter(PresenterBuilder<CrudScreenPart<TEntity>, Empty.Data, IState> presenter)
        {
            ContentRoot = Crud;

            if (ImportExportEnabled)
            {
                PopupContents.Add(ImportForm);
                PopupContents.Add(ExportForm);

                Crud.StaticCommands.Add(Import);
                Crud.StaticCommands.Add(Export);

                Import.Kind = CommandKind.Submit;
                Import.Severity = CommandSeverity.Change;
                Import.Icon = "upload";
                Export.Kind = CommandKind.Navigate;
                Export.Severity = CommandSeverity.Read;
                Export.Icon = "download";

                ImportForm.AttachAsPopupTo(presenter, Import);
                ImportForm.UseOutputForm();
                ImportForm.Execute.Text = "Import";
                ImportForm.Reset.Text = "Cancel";
                ImportForm.InputForm.Field(x => x.Format);
                ImportForm.InputForm.Field(x => x.File, type: FormFieldType.FileUpload);
                ImportForm.InputForm.LookupSource(x => x.Format, x => x.AvailableFormats);
                ImportForm.ContextEntityType = typeof(TEntity);

                if (ImportForm.OutputForm != null)
                {
                    ImportForm.OutputForm.Field(
                        x => x.ImportIssues, label: "MessagesAndIssues", type: FormFieldType.InlineGrid, modifiers: FormFieldModifiers.Tab | FormFieldModifiers.FlatStyle, 
                        setup: f => {
                            var issuesCrud = (Crud<IDocumentImportIssue>)f.NestedWidget;
                            issuesCrud.DisableAuthorizationChecks = true;
                            issuesCrud.DisableToolBar = true;
                            issuesCrud.DisableForm = true;
                            //issuesCrud.Grid.EnablePaging = false;
                            issuesCrud.Grid
                                .Column(x => x.Severity, size: FieldSize.Small)
                                .Column(x => x.Text, size: FieldSize.Large, title: "Message")
                                .Column(x => x.Location, size: FieldSize.Medium);
                        });
                }
                
                ExportForm.AttachAsPopupTo(presenter, Export);
                ExportForm.Execute.Text = "Export";
                ExportForm.Reset.Text = "Cancel";
                ExportForm.InputForm.LookupSource(x => x.Format, x => x.AvailableFormats);
                ExportForm.ContextEntityType = typeof(TEntity);
                ExportForm.OutputDownloadFormat = "EXCEL";

                var entityName = MetadataCache.GetTypeMetadata(typeof(TEntity)).QualifiedName;

                presenter.On(NavigatedHere)
                    .AlterModel(
                        alt => alt.Copy(entityName).To(vm => vm.State.ImportContext.EntityName),
                        alt => alt.Copy(entityName).To(vm => vm.State.ExportContext.EntityName))
                    .Then(b => b.Broadcast(ImportForm.ContextSetter).WithPayload(vm => vm.State.ImportContext).TunnelDown()
                    .Then(bb => bb.Broadcast(ExportForm.ContextSetter).WithPayload(vm => vm.State.ExportContext).TunnelDown()
                    .ThenIf(NavigateToFormEnabled, bbb => bbb.Broadcast(Crud.ContextSetter).WithPayload(vm => vm.Input).TunnelDown())));

                presenter.On(ImportForm.OutputReady).Broadcast(Crud.RefreshRequested).TunnelDown();
            }
            else
            {
                ImportForm = null;
                ExportForm = null;

                if (NavigateToFormEnabled)
                {
                    presenter.On(NavigatedHere).Broadcast(Crud.ContextSetter).WithPayload(vm => vm.Input).TunnelDown();
                }
            }

            var metaType = base.MetadataCache.GetTypeMetadata(typeof(TEntity));
            this.Text = metaType.Name + "Management";

            presenter.On(Crud.SelectedEntityChanged).AlterModel(alt => alt.Copy(vm => vm.Input).To(vm => vm.State.Entity));
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public bool ImportExportEnabled { get; set; }
        public bool NavigateToFormEnabled { get; set; }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public Crud<TEntity> Crud { get; set; }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public TransactionForm<CrudEntityImportTx.IContext, CrudEntityImportTx.IInput, CrudEntityImportTx, CrudEntityImportTx.IOutput> ImportForm { get; set; }
        public TransactionForm<CrudEntityExportTx.IContext, CrudEntityExportTx.IInput, CrudEntityExportTx, DocumentFormatReplyMessage> ExportForm { get; set; }
        public DataGrid<IDocumentImportIssue> ImportIssues { get; set; }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public UidlCommand Import { get; set; }
        public UidlCommand Export { get; set; }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        [ViewModelContract]
        public interface IState
        {
            TEntity Entity { get; set; }
            CrudEntityImportTx.IContext ImportContext { get; set; }
            CrudEntityExportTx.IContext ExportContext { get; set; }
        }
    }
}
