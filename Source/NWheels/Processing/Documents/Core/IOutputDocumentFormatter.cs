using NWheels.DataObjects.Core;
using NWheels.Entities.Core;
using NWheels.UI;

namespace NWheels.Processing.Documents.Core
{
    public interface IOutputDocumentFormatter
    {
        FormattedDocument FormatReportDocument(IObject criteria, ApplicationEntityService.EntityCursor queryResults, DocumentDesign design);
        FormattedDocument FormatFixedDocument(IObject model, DocumentDesign design);
        DocumentFormat MetaFormat { get; }
    }
}
