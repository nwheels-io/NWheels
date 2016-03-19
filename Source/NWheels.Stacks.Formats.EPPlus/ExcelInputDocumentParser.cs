using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Processing.Documents;
using NWheels.Processing.Documents.Core;
using NWheels.UI;

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

        public void ImportDataFromReportDocument(FormattedDocument document, DocumentDesign design, ApplicationEntityService entityService)
        {
            var tableDesign = (DocumentDesign.TableElement)design.Contents;
            var entityHandler = entityService.GetEntityHandler(tableDesign.BoundEntityName);



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
