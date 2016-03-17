using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Authorization.Core;
using NWheels.DataObjects;
using NWheels.Entities.Core;
using NWheels.Processing;
using NWheels.Processing.Documents;
using NWheels.Processing.Documents.Core;
using NWheels.UI.Factories;

namespace NWheels.UI.Impl
{
    [TransactionScript(SupportsInitializeInput = true)]
    public class CrudEntityExportTx : ITransactionScript<Empty.Context, CrudEntityExportTx.IInput, DocumentFormatReplyMessage>
    {
        private readonly IFramework _framework;
        private readonly IViewModelObjectFactory _viewModelFactory;
        private readonly IReadOnlyDictionary<string, IEntityExportFormat> _exportFormatByName;
        private readonly IReadOnlyDictionary<string, IOutputDocumentFormatter> _formatterByFormatIdName;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public CrudEntityExportTx(
            IFramework framework,
            IViewModelObjectFactory viewModelFactory, 
            IEnumerable<IEntityExportFormat> exportFormats, 
            IEnumerable<IOutputDocumentFormatter> outputFormatters)
        {
            _framework = framework;
            _viewModelFactory = viewModelFactory;
            _exportFormatByName = exportFormats.ToDictionary(f => f.FormatTitle);
            _formatterByFormatIdName = outputFormatters.ToDictionary(f => f.MetaFormat.IdName);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of ITransactionScript<Context,IInput,Output>

        public IInput InitializeInput(Empty.Context context)
        {
            var input = _viewModelFactory.NewEntity<IInput>();

            if (_exportFormatByName.Count > 0)
            {
                input.Format = _exportFormatByName.First().Key;
            }

            return input;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public DocumentFormatReplyMessage Preview(IInput input)
        {
            throw new NotSupportedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public DocumentFormatReplyMessage Execute(IInput input)
        {
            var uiContext = UIOperationContext.Current;
            var exportFormat = _exportFormatByName[input.Format];
            var outputFormatter = _formatterByFormatIdName[exportFormat.DocumentFormatIdName];

            var entityHandler = uiContext.EntityService.GetEntityHandler(uiContext.EntityName);
            var cursor = entityHandler.QueryCursor(uiContext.Query);
            var document = outputFormatter.FormatReportDocument((IDomainObject)input, cursor, exportFormat.DocumentDesign);

            return new DocumentFormatReplyMessage(_framework, Session.Current, _framework.NewGuid(), document);
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [ViewModelContract]
        public interface IInput
        {
            [PropertyContract.Required, EntityExportFormatSemantics]
            string Format { get; set; }
        }
    }
}
