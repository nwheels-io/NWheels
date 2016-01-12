using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Authorization.Core;
using NWheels.DataObjects;
using NWheels.Entities.Core;
using NWheels.Processing.Documents;
using NWheels.Processing.Documents.Core;
using NWheels.UI;
using OfficeOpenXml;

namespace NWheels.Stacks.Formats.EPPlus
{
    public class ExcelOutputDocumentFormatter : IOutputDocumentFormatter
    {
        #region Implementation of IOutputDocumentFormatter

        public FormattedDocument FormatReportDocument(IDomainObject criteria, ApplicationEntityService.EntityCursor queryResults, DocumentDesign design)
        {
            using ( ExcelPackage package = new ExcelPackage() )
            {
                var worksheet = package.Workbook.Worksheets.Add("Report");

                for ( int i = 0; i < queryResults.ColumnCount; i++ )
                {
                    worksheet.Cells[1, i + 1].Value = queryResults.Columns[i].AliasName;
                    worksheet.Cells[1, i + 1].Style.Font.Bold = true;
                }

                int rowNumber = 2;

                foreach ( var row in queryResults )
                {
                    for ( int i = 0; i < queryResults.ColumnCount; i++ )
                    {
                        worksheet.Cells[rowNumber, i + 1].Value = row[i].ToString();
                    }

                    rowNumber++;
                }

                return new FormattedDocument(
                    new DocumentMetadata(_s_metaFormat, "report.xlsx"),
                    package.GetAsByteArray());
            }            
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
