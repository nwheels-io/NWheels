using NWheels.Entities.Core;
using NWheels.UI;

namespace NWheels.Processing.Documents.Core
{
    public interface IOutputDocumentFormatter
    {
        FormattedDocument FormatReportDocument(IDomainObject criteria, ApplicationEntityService.EntityCursor queryResults, DocumentDesign design);
        FormattedDocument FormatFixedDocument(IDomainObject model, DocumentDesign design);
        DocumentFormat MetaFormat { get; }
    }
}
