using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Authorization.Core;
using NWheels.DataObjects;
using NWheels.DataObjects.Core;
using NWheels.Entities.Core;
using NWheels.Processing;
using NWheels.Processing.Documents;
using NWheels.Processing.Documents.Core;
using NWheels.UI.Factories;

namespace NWheels.UI.Impl
{
    [TransactionScript(SupportsInitializeInput = true)]
    public class CrudEntityExportTx : TransactionScript<CrudEntityExportTx.IContext, CrudEntityExportTx.IInput, DocumentFormatReplyMessage>
    {
        private readonly IFramework _framework;
        private readonly IViewModelObjectFactory _viewModelFactory;
        private readonly EntityImportExportFormatSet _formatSet;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public CrudEntityExportTx(
            IFramework framework,
            IViewModelObjectFactory viewModelFactory, 
            IEnumerable<IEntityExportFormat> exportFormats, 
            IEnumerable<IOutputDocumentFormatter> outputFormatters)
        {
            _framework = framework;
            _viewModelFactory = viewModelFactory;
            _formatSet = new EntityImportExportFormatSet(exportFormats, outputFormatters: outputFormatters);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of ITransactionScript<Context,IInput,Output>

        public override IInput InitializeInput(IContext context)
        {
            var availableFormats = _formatSet.GetAvailableFormats(UIOperationContext.Current, context.EntityName);
            var input = _viewModelFactory.NewEntity<IInput>();
            
            input.AvailableFormats = availableFormats.Select(f => f.FormatName).ToArray();
            
            return input;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override DocumentFormatReplyMessage Preview(IInput input)
        {
            throw new NotSupportedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override DocumentFormatReplyMessage Execute(IInput input)
        {
            IEntityExportFormat exportFormat;

            var uiContext = UIOperationContext.Current;
            var outputFormatter = _formatSet.GetOutputFormatter(input.Format, out exportFormat);
            var entityHandler = uiContext.EntityService.GetEntityHandler(uiContext.EntityName);
            var queryOptions = new ApplicationEntityService.QueryOptions(uiContext.EntityName, new Dictionary<string, string>());
            
            var tableDesign = ((DocumentDesign.TableElement)exportFormat.DocumentDesign.Contents);
            
            foreach (var column in tableDesign.Columns.Where(c => c.Binding.Expression != null))
            {
                queryOptions.SelectPropertyNames.Add(new ApplicationEntityService.QuerySelectItem(column.Binding.Expression));
            }
            
            using (ApplicationEntityService.QueryContext.NewQuery(uiContext.EntityService, queryOptions))
            {
                using (entityHandler.NewUnitOfWork())
                {
                    var cursor = entityHandler.QueryCursor(queryOptions);
                    var document = outputFormatter.FormatReportDocument((IObject)input, cursor, exportFormat.DocumentDesign);
                    return new DocumentFormatReplyMessage(_framework, Session.Current, _framework.NewGuid(), document);
                }
            }
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [ViewModelContract]
        public interface IContext
        {
            string EntityName { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [ViewModelContract]
        public interface IInput
        {
            [PropertyContract.Required]
            string Format { get; set; }

            ICollection<string> AvailableFormats { get; set; }
        }
    }
}
