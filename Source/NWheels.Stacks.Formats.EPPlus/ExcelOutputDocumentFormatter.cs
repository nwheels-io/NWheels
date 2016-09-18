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
using NWheels.TypeModel;
using NWheels.UI;
using OfficeOpenXml;
using OfficeOpenXml.FormulaParsing.Utilities;

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
            var worksheet = package.Workbook.Worksheets.Add(cursor.PrimaryEntity.QualifiedName);

            if (design.Options.CustomExport != null)
            {
                design.Options.CustomExport(new CustomExportContext(cursor, design, package, worksheet));
                return;
            }

            var table = (DocumentDesign.TableElement)design.Contents;
            var cursorColumnIndex = new int[table.Columns.Count];

            worksheet.Row(2).Style.Font.Bold = true;
            worksheet.View.FreezePanes(3, 1);

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
            
            FormatDefaultReportDocumentColumns(queryResults, worksheet);
            worksheet.View.FreezePanes(2, 1);
            
            int rowNumber = 2;
            var metadataColumns = queryResults.Metadata.Columns;

            foreach (var row in queryResults)
            {
                for (int i = 0; i < queryResults.ColumnCount; i++)
                {
                    var cellValue = GetDefaultFormatCellValue(queryResults, row[i], metadataColumns[i]);
                    worksheet.Cells[rowNumber, i + 1].Value = cellValue;
                }

                rowNumber++;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private object GetDefaultFormatCellValue(
            ApplicationEntityService.EntityCursor queryResults, 
            object dataValue, 
            ApplicationEntityService.QuerySelectItem column)
        {
            if (dataValue == null)
            {
                return "N/A";
            }
            
            if (dataValue is IDomainObject)
            {
                var relation = column.MetaProperty.Relation;

                if (relation != null && relation.RelatedPartyType != null && relation.RelatedPartyType.EntityIdProperty != null)
                {
                    return relation.RelatedPartyType.EntityIdProperty.ReadValue(dataValue);
                }
            }

            return dataValue;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void FormatDefaultReportDocumentColumns(ApplicationEntityService.EntityCursor queryResults, ExcelWorksheet worksheet)
        {
            for (int i = 0 ; i < queryResults.ColumnCount ; i++)
            {
                var column = queryResults.Columns[i];

                worksheet.Cells[1, i + 1].Value = column.AliasName;
                worksheet.Cells[1, i + 1].Style.Font.Bold = true;

                if (column.MetaProperty != null)
                {
                    worksheet.Column(i + 1).Style.Numberformat.Format = GetColumnDefaultFormat(column.MetaProperty);
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private string GetColumnDefaultFormat(IPropertyMetadata metaProperty)
        {
            if (!string.IsNullOrEmpty(metaProperty.DefaultDisplayFormat))
            {
                return metaProperty.DefaultDisplayFormat;
            }
            else if (metaProperty.ClrType.IsAnyNumericType())
            {
                if (metaProperty.Relation != null)
                {
                    return "0";
                }
                return (metaProperty.ClrType.IsIntegerNumericType() ? "#,##0" : "#,##0.00000");
            }
            else if (metaProperty.ClrType == typeof(DateTime) || metaProperty.ClrType == typeof(DateTimeOffset))
            {
                if (metaProperty.SemanticType != null)
                {
                    switch (metaProperty.SemanticType.WellKnownSemantic)
                    {
                        case WellKnownSemanticType.Date:
                            return "dd MMM yyyy";
                        case WellKnownSemanticType.Time:
                            return "HH:mm:ss";
                    }
                }

                return "yyyy-MM-dd HH:mm:ss";
            }

            return null;
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

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        public class CustomExportContext
        {
            public CustomExportContext(ApplicationEntityService.EntityCursor cursor, DocumentDesign design, ExcelPackage package, ExcelWorksheet worksheet)
            {
                Cursor = cursor;
                Design = design;
                Package = package;
                Worksheet = worksheet;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ApplicationEntityService.EntityCursor Cursor { get; private set; }
            public DocumentDesign Design { get; private set; }
            public ExcelPackage Package { get; private set; }
            public ExcelWorksheet Worksheet { get; private set; }
        }
    }
}
