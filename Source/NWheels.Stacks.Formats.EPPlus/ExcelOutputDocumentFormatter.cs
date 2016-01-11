using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.DataObjects;
using NWheels.Entities.Core;
using NWheels.Processing.Documents;
using NWheels.Processing.Documents.Core;
using NWheels.UI;

namespace NWheels.Stacks.Formats.EPPlus
{
    public class ExcelOutputDocumentFormatter : IOutputDocumentFormatter
    {
        #region Implementation of IOutputDocumentFormatter

        public FormattedDocument FormatReportDocument(IDomainObject criteria, ApplicationEntityService.EntityCursor queryResults, DocumentDesign design)
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public FormattedDocument FormatFixedDocument(IDomainObject model, DocumentDesign design)
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public DocumentFormat MetaFormat
        {
            get
            {
                return _s_metaFormat;
            }
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static readonly DocumentFormat _s_metaFormat = new DocumentFormat(
            idName: "EXCEL", 
            contentType: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileExtension: "xslx",
            defaultFileName: "workbook.xlsx");
    }
}
