using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Authorization.Core;
using NWheels.DataObjects;
using NWheels.DataObjects.Core;
using NWheels.Entities.Core;
using NWheels.Extensions;
using NWheels.Processing.Documents;
using NWheels.Processing.Documents.Core;
using NWheels.UI;
using OfficeOpenXml;

namespace NWheels.Stacks.Formats.EPPlus
{
    public class ExcelOutputDocumentFormatter : IOutputDocumentFormatter
    {
        #region Implementation of IOutputDocumentFormatter

        public FormattedDocument FormatReportDocument(IObject criteria, ApplicationEntityService.EntityCursor queryResults, DocumentDesign design)
        {
            using ( ExcelPackage package = new ExcelPackage() )
            {
                string fileName;

                if (design != null)
                {
                    FormatReportDocumentByDesign(queryResults, design, package);
                    fileName = design.IdName + ".xlsx";
                }
                else
                {
                    FormatDefaultReportDocument(queryResults, package);
                    fileName = "report.xlsx";
                }

                return new FormattedDocument(
                    new DocumentMetadata(ExcelMetaFormat.GetFormat(), fileName),
                    package.GetAsByteArray());
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public FormattedDocument FormatFixedDocument(IObject model, DocumentDesign design)
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public DocumentFormat MetaFormat
        {
            get
            {
                return ExcelMetaFormat.GetFormat();
            }
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void FormatReportDocumentByDesign(ApplicationEntityService.EntityCursor cursor, DocumentDesign design, ExcelPackage package)
        {
            var table = (DocumentDesign.TableElement)design.Contents;
            var worksheet = package.Workbook.Worksheets.Add(cursor.PrimaryEntity.QualifiedName);
            var cursorColumnIndex = new int[table.Columns.Count];

            worksheet.Row(2).Style.Font.Bold = true;

            for (int i = 0; i < table.Columns.Count; i++)
            {
                cursorColumnIndex[i] = TryFindCursorColumnIndex(cursor, table.Columns[i]);

                if (table.Columns[i].Width.HasValue)
                {
                    worksheet.Column(i + 1).Width = table.Columns[i].Width.Value;
                }

                if (table.Columns[i].Binding.Format != null)
                {
                    worksheet.Column(i + 1).Style.Numberformat.Format = table.Columns[i].Binding.Format;
                }

                if (table.Columns[i].Binding.IsKey)
                {
                    worksheet.Column(i + 1).Style.Font.Color.SetColor(Color.Blue);
                }

                worksheet.Cells[2, i + 1].Value = table.Columns[i].Title;
            }

            worksheet.Row(1).Style.Font.Color.SetColor(Color.Maroon);
            worksheet.Cells[1, 1].Value = "FORMAT";
            worksheet.Cells[1, 2].Value = design.IdName;

            int rowNumber = 3;

            foreach (var row in cursor)
            {
                for (int col = 0; col < table.Columns.Count; col++)
                {
                    worksheet.Cells[rowNumber, col + 1].Value = table.Columns[col].Binding.ReadValueFromCursor(row, cursorColumnIndex[col], applyFormat: false);
                }

                rowNumber++;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void FormatDefaultReportDocument(ApplicationEntityService.EntityCursor queryResults, ExcelPackage package)
        {
            var worksheet = package.Workbook.Worksheets.Add("Report");

            for (int i = 0; i < queryResults.ColumnCount; i++)
            {
                worksheet.Cells[1, i + 1].Value = queryResults.Columns[i].AliasName;
                worksheet.Cells[1, i + 1].Style.Font.Bold = true;
            }

            int rowNumber = 2;

            foreach (var row in queryResults)
            {
                for (int i = 0; i < queryResults.ColumnCount; i++)
                {
                    worksheet.Cells[rowNumber, i + 1].Value = row[i].ToStringOrDefault("N/A");
                }

                rowNumber++;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private int TryFindCursorColumnIndex(ApplicationEntityService.EntityCursor cursor, DocumentDesign.TableElement.Column column)
        {
            for (int cursorColumnIndex = 0; cursorColumnIndex < cursor.ColumnCount; cursorColumnIndex++)
            {
                var cursorColumnExpression = string.Join(".", cursor.Columns[cursorColumnIndex].PropertyPath);
                if (cursorColumnExpression == column.Binding.Expression)
                {
                    return cursorColumnIndex;
                }
            }

            return -1;
        }
    }
}
