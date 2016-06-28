using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Concurrency;
using NWheels.Exceptions;
using NWheels.Processing.Documents;
using NWheels.Processing.Documents.Core;
using NWheels.UI;
using NWheels.UI.Impl;
using OfficeOpenXml;

namespace NWheels.Stacks.Formats.EPPlus
{
    public class ExcelInputDocumentParser : IInputDocumentParser
    {
        private readonly IFramework _framework;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ExcelInputDocumentParser(IFramework framework)
        {
            _framework = framework;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void ImportDataFromReportDocument(
            FormattedDocument document, 
            DocumentDesign design, 
            ApplicationEntityService entityService, 
            IWriteOnlyCollection<DocumentImportIssue> issues)
        {
            var importOperation = new ExcelDataImportOperation(document, design, entityService, issues);
            importOperation.Execute();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public DocumentFormat MetaFormat
        {
            get
            {
                return ExcelMetaFormat.GetFormat();
            }
        }
    }
}
