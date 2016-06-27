using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Entities;
using NWheels.Entities.Core;
using NWheels.Exceptions;
using NWheels.Extensions;
using NWheels.Processing.Documents;
using NWheels.UI;
using NWheels.UI.Impl;
using OfficeOpenXml;

namespace NWheels.Stacks.Formats.EPPlus
{
    public class ExcelDataImportOperation
    {
        private readonly FormattedDocument _document;
        private readonly DocumentDesign _design;
        private readonly DocumentDesign.TableElement _tableDesign;
        private readonly int[] _keyColumnIndex;
        private readonly ApplicationEntityService _entityService;
        private readonly ApplicationEntityService.EntityHandler _entityHandler;
        private IApplicationDataRepository _domainContext;
        private ApplicationEntityService.EntityCursorRow[] _cursorBuffer;
        private ApplicationEntityService.EntityCursorMetadata _metaCursor;
        //private int[] _cursorColumnIndex;
        private ExcelPackage _package;
        private ExcelWorksheet _worksheet;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ExcelDataImportOperation(FormattedDocument document, DocumentDesign design, ApplicationEntityService entityService)
        {
            if (document == null)
            {
                throw new ArgumentNullException("document");
            }
            if (design == null)
            {
                throw new ArgumentNullException("design");
            }
            if (entityService == null)
            {
                throw new ArgumentNullException("entityService");
            }

            _entityService = entityService;
            _document = document;
            _design = design;
            _tableDesign = (design.Contents as DocumentDesign.TableElement);

            if (_tableDesign == null)
            {
                throw new NotSupportedException("Import from excel is only supported for table document design.");
            }

            _entityHandler = _entityService.GetEntityHandler(_tableDesign.BoundEntityName);
            _keyColumnIndex = _tableDesign.Columns.Select((column, index) => column.Binding.IsKey ? index : -1).Where(n => n >= 0).ToArray();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Execute()
        {
            var inputStream = new MemoryStream(_document.Contents);

            using (_package = new ExcelPackage())
            {
                _package.Load(inputStream);

                ExcelImportExportFault? formatValidationFault;
                if (!ValidateFormatSignature(_package, _design, out formatValidationFault))
                {
                    throw new DomainFaultException<EntityImportExportFault, ExcelImportExportFault>(
                        EntityImportExportFault.BadInputDocumentFormat,
                        formatValidationFault.Value);
                }

                using (_domainContext = (IApplicationDataRepository)_entityHandler.NewUnitOfWork())
                {
                    RetrieveExistingEntities();
                    ImportDataRows();
                    
                    _domainContext.CommitChanges();
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void RetrieveExistingEntities()
        {
            var queryOptions = new ApplicationEntityService.QueryOptions(
                _entityHandler.MetaType.QualifiedName, 
                queryParams: new Dictionary<string, string>());

            foreach (var column in _tableDesign.Columns.Where(c => c.Binding.Expression != null))
            {
                queryOptions.SelectPropertyNames.Add(new ApplicationEntityService.QuerySelectItem(column.Binding.Expression)); 
            }

            using (ApplicationEntityService.QueryContext.NewQuery(_entityService, queryOptions))
            {
                var cursor = _entityHandler.QueryCursor(queryOptions);
                _metaCursor = cursor.Metadata;
                _cursorBuffer = cursor.ToArray();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void ImportDataRows()
        {
            if (_design.CustomImport != null)
            {
                _design.CustomImport(new CustomImportContext(
                    _design,
                    _package,
                    _worksheet,
                    _entityService,
                    _entityHandler,
                    _domainContext,
                    _metaCursor,
                    _cursorBuffer
                ));
                return;
            }

            for (int rowNumber = 3 ; !RowIsEmpty(rowNumber) ; rowNumber++)
            {
                ApplicationEntityService.EntityCursorRow existingCursorRow;
                IDomainObject entity;

                if (TryLocateExistingCursorRow(rowNumber, out existingCursorRow))
                {
                    entity = existingCursorRow.Record;
                }
                else
                {
                    entity = _entityHandler.CreateNew();
                }

                PopulateEntityFromWorksheetRow(entity, rowNumber, isNew: existingCursorRow == null);
                _domainContext.GetEntityRepository(entity).Save(entity);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void PopulateEntityFromWorksheetRow(IDomainObject entity, int rowNumber, bool isNew)
        {
            for (int i = 0 ; i < _tableDesign.Columns.Count ; i++)
            {
                var column = _tableDesign.Columns[i];

                if (column.Binding.IsKey && !isNew)
                {
                    continue;
                }

                if (_metaCursor.Columns[i].PropertyPath.Count != 1)
                {
                    continue;
                }

                var metaProperty = _metaCursor.Columns[i].MetaProperty;

                var cellValueString = _worksheet.Cells[rowNumber, i + 1].Value.ToStringOrDefault();
                var parsedValue = metaProperty.ParseStringValue(cellValueString);
                
                _metaCursor.Columns[i].MetaProperty.WriteValue(entity, parsedValue);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private bool TryLocateExistingCursorRow(int rowNumber, out ApplicationEntityService.EntityCursorRow row)
        {
            for (int cursorIndex = 0 ; cursorIndex < _cursorBuffer.Length ; cursorIndex++)
            {
                if (MatchRowsByKey(sheetRowNumber: rowNumber, cursorRow: _cursorBuffer[cursorIndex]))
                {
                    row = _cursorBuffer[cursorIndex];
                    return true;
                }
            }

            row = null;
            return false;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private bool MatchRowsByKey(int sheetRowNumber, ApplicationEntityService.EntityCursorRow cursorRow)
        {
            for (int keyCol = 0 ; keyCol < _keyColumnIndex.Length ; keyCol++)
            {
                var keyColIndex = _keyColumnIndex[keyCol];
                var keyColumn = _tableDesign.Columns[keyColIndex];
                var sheetCell = _worksheet.Cells[sheetRowNumber, keyColIndex + 1];
                var sheetCellValue = sheetCell.Value;
                var cursorKeyValue = keyColumn.Binding.ReadValueFromCursor(cursorRow, keyColIndex, applyFormat: false);

                if (cursorKeyValue == null)
                {
                    if (sheetCellValue != null)
                    {
                        return false;
                    }
                }
                else if (!cursorKeyValue.Equals(sheetCellValue))
                {
                    return false;
                }
            }

            return true;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        //private string GetEntityLocatorText(Dictionary<string, string> filter)
        //{
        //    var filterText = string.Join(" & ", filter.Select(kvp => kvp.Key + "=" + kvp.Value));
        //    var locatorText = string.Format("{0}[{1}]", _entityHandler.MetaType.QualifiedName, filterText);
        //    return locatorText;
        //}

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private bool RowIsEmpty(int rowNumber)
        {
            for (int col = 0 ; col < _tableDesign.Columns.Count ; col++)
            {
                if (_worksheet.Cells[rowNumber, col + 1].Value != null)
                {
                    return false;
                }
            }

            return true;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private bool ValidateFormatSignature(ExcelPackage package, DocumentDesign design, out ExcelImportExportFault? faultSubCode)
        {
            if (package.Workbook.Worksheets.Count != 1)
            {
                faultSubCode = ExcelImportExportFault.WrongNumberOfWorksheets;
                return false;
            }

            _worksheet = package.Workbook.Worksheets[1];

            var formatIdName = package.Workbook.Worksheets[1].Cells[1, 2].Value as string;

            if (string.IsNullOrEmpty(formatIdName))
            {
                faultSubCode = ExcelImportExportFault.MissingFormatSignature;
                return false;
            }

            if (formatIdName != design.IdName)
            {
                faultSubCode = ExcelImportExportFault.FormatSignatureMismatch;
                return false;
            }

            faultSubCode = null;
            return true;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class CustomImportContext
        {
            public CustomImportContext(
                DocumentDesign design, 
                ExcelPackage package, 
                ExcelWorksheet worksheet, 
                ApplicationEntityService entityService, 
                ApplicationEntityService.EntityHandler entityHandler, 
                IApplicationDataRepository domainContext, 
                ApplicationEntityService.EntityCursorMetadata metaCursor, 
                ApplicationEntityService.EntityCursorRow[] cursorBuffer)
            {
                Design = design;
                Package = package;
                Worksheet = worksheet;
                EntityService = entityService;
                EntityHandler = entityHandler;
                DomainContext = domainContext;
                MetaCursor = metaCursor;
                CursorBuffer = cursorBuffer;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public DocumentDesign Design { get; private set; }
            public ExcelPackage Package { get; private set; }
            public ExcelWorksheet Worksheet { get; private set; }
            public ApplicationEntityService EntityService { get; private set; }
            public ApplicationEntityService.EntityHandler EntityHandler { get; private set; }
            public IApplicationDataRepository DomainContext { get; private set; }
            public ApplicationEntityService.EntityCursorMetadata MetaCursor { get; private set; }
            public ApplicationEntityService.EntityCursorRow[] CursorBuffer { get; private set; }
        }
    }
}
