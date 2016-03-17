namespace NWheels.Processing.Documents.Core
{
    public interface IInputDocumentParser
    {
        void ImportReportDocument(FormattedDocument document, DocumentDesign design);
        DocumentFormat MetaFormat { get; }
    }
}
