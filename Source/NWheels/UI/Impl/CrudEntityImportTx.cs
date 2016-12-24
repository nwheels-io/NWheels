using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.DataObjects;
using NWheels.Extensions;
using NWheels.Logging;
using NWheels.Processing;
using NWheels.Processing.Documents;
using NWheels.Processing.Documents.Core;
using NWheels.UI.Factories;

namespace NWheels.UI.Impl
{
    [TransactionScript(SupportsInitializeInput = true)]
    public class CrudEntityImportTx : TransactionScript<CrudEntityImportTx.IContext, CrudEntityImportTx.IInput, CrudEntityImportTx.IOutput>
    {
        private readonly IFramework _framework;
        private readonly IViewModelObjectFactory _viewModelFactory;
        private readonly EntityImportExportFormatSet _formatSet;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public CrudEntityImportTx(
            IFramework framework,
            IViewModelObjectFactory viewModelFactory, 
            IEnumerable<IEntityExportFormat> exportFormats,
            IEnumerable<IInputDocumentParser> inputParsers)
        {
            _framework = framework;
            _viewModelFactory = viewModelFactory;
            _formatSet = new EntityImportExportFormatSet(exportFormats, inputParsers: inputParsers);
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

        public override IOutput Preview(IInput input)
        {
            throw new NotSupportedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override IOutput Execute(IInput input)
        {
            IEntityExportFormat importFormat;

            var uiContext = UIOperationContext.Current;
            var inputParser = _formatSet.GetInputParser(input.Format, out importFormat);
            var document = new FormattedDocument(new DocumentMetadata(inputParser.MetaFormat), input.File);
            var issues = new List<DocumentImportIssue>();

            inputParser.ImportDataFromReportDocument(document, importFormat.DocumentDesign, uiContext.EntityService, issues.AsWriteOnly());

            if (issues.Count == 0)
            {
                issues.Add(DocumentImportIssue.NoneReported());
            }
            else
            {
                var errorCount = issues.Count(x => x.Severity >= SeverityLevel.Error);
                var warningCount = issues.Count(x => x.Severity == SeverityLevel.Warning);
                
                issues.Insert(0, DocumentImportIssue.Done(errorCount, warningCount));
            }

            var output = _viewModelFactory.NewEntity<IOutput>();
            output.ImportIssues = issues.Cast<IDocumentImportIssue>().ToList();
            return output;
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

            [PropertyContract.Required, PropertyContract.Semantic.FileUpload]
            byte[] File { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [ViewModelContract]
        public interface IOutput
        {
            ICollection<IDocumentImportIssue> ImportIssues { get; set; }
        }
    }
}
