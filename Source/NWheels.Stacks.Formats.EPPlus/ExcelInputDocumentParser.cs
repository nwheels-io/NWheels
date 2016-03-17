using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Processing.Documents;
using NWheels.Processing.Documents.Core;

namespace NWheels.Stacks.Formats.EPPlus
{
    public class ExcelInputDocumentParser : IInputDocumentParser
    {


        #region Implementation of IInputDocumentParser

        public void ImportReportDocument(FormattedDocument document, DocumentDesign design)
        {
            throw new NotImplementedException();
        }

        public DocumentFormat MetaFormat { get; private set; }

        #endregion
    }
}
