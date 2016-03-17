using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using NWheels.Processing.Documents;
using NWheels.UI.Impl;
using NWheels.UI.Uidl;

namespace NWheels.UI.Toolbox
{
    public class CrudScreenPart<TEntity> : ScreenPartBase<CrudScreenPart<TEntity>, Empty.Input, Empty.Data, CrudScreenPart<TEntity>.IState>
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
                ImportForm.Execute.Text = "Import";
                ImportForm.Reset.Text = "Cancel";
                ImportForm.InputForm.Field(x => x.Format);
                ImportForm.InputForm.Field(x => x.File, type: FormFieldType.FileUpload);
                
                ExportForm.AttachAsPopupTo(presenter, Export);
                ExportForm.Execute.Text = "Export";
                ExportForm.Reset.Text = "Cancel";
            }
            else
            {
                ImportForm = null;
                ExportForm = null;
            }

            var metaType = base.MetadataCache.GetTypeMetadata(typeof(TEntity));
            this.Text = metaType.Name + "Management";

            presenter.On(Crud.SelectedEntityChanged).AlterModel(alt => alt.Copy(vm => vm.Input).To(vm => vm.State.Entity));
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public bool ImportExportEnabled { get; set; }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public Crud<TEntity> Crud { get; set; }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public TransactionForm<CrudEntityImportTx.IInput, CrudEntityImportTx, Empty.Output> ImportForm { get; set; }
        public TransactionForm<CrudEntityExportTx.IInput, CrudEntityExportTx, DocumentFormatReplyMessage> ExportForm { get; set; }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public UidlCommand Import { get; set; }
        public UidlCommand Export { get; set; }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        [ViewModelContract]
        public interface IState
        {
            TEntity Entity { get; set; }
        }
    }
}
