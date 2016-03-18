using System;
using System.Collections.Generic;
using System.Linq;
using NWheels.Extensions;
using NWheels.Processing.Documents.Core;
using NWheels.UI.Factories;

namespace NWheels.UI.Impl
{
    public class EntityImportExportFormatSet
    {
        private readonly IReadOnlyList<IEntityExportFormat> _exportFormats;
        private readonly IReadOnlyList<IInputDocumentParser> _inputParsers;
        private readonly IReadOnlyList<IOutputDocumentFormatter> _outputFormatters;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public EntityImportExportFormatSet(
            IEnumerable<IEntityExportFormat> exportFormats,
            IEnumerable<IInputDocumentParser> inputParsers = null,
            IEnumerable<IOutputDocumentFormatter> outputFormatters = null)
        {
            _exportFormats = exportFormats.ToArray();
            _inputParsers = (inputParsers ?? new IInputDocumentParser[0]).ToArray();
            _outputFormatters = (outputFormatters ?? new IOutputDocumentFormatter[0]).ToArray();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IEnumerable<IEntityExportFormat> GetAvailableFormats(UIOperationContext uiContext, string entityName = null)
        {
            var entityHandler = uiContext.EntityService.GetEntityHandler(entityName.OrDefaultIfNullOrWhitespace(uiContext.EntityName));
            var entityContract = entityHandler.MetaType.ContractType;
            return GetAvailableFormats(entityContract);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IEnumerable<IEntityExportFormat> GetAvailableFormats(Type entityContract)
        {
            var availableFormats = _exportFormats.Where(f => f.EntityContract.IsAssignableFrom(entityContract));
            return availableFormats;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IEntityExportFormat GetFormatByName(string formatName)
        {
            return _exportFormats.First(f => f.FormatName == formatName);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IInputDocumentParser GetInputParser(string formatName, out IEntityExportFormat format)
        {
            format = _exportFormats.First(f => f.FormatName == formatName);
            var documentFormatIdName = format.DocumentFormatIdName;
            return _inputParsers.First(p => p.MetaFormat.IdName == documentFormatIdName);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IOutputDocumentFormatter GetOutputFormatter(string formatName, out IEntityExportFormat format)
        {
            format = _exportFormats.First(f => f.FormatName == formatName);
            var documentFormatIdName = format.DocumentFormatIdName;
            return _outputFormatters.First(p => p.MetaFormat.IdName == documentFormatIdName);
        }
    }
}
