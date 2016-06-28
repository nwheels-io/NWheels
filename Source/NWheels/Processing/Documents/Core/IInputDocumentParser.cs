using NWheels.Concurrency;
using NWheels.UI;

namespace NWheels.Processing.Documents.Core
{
    public interface IInputDocumentParser
    {
        void ImportDataFromReportDocument(
            FormattedDocument document, 
            DocumentDesign design, 
            ApplicationEntityService entityService, 
            IWriteOnlyCollection<DocumentImportIssue> issues);

        DocumentFormat MetaFormat { get; }
    }
}
