using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.DataObjects;
using NWheels.Processing;
using NWheels.Processing.Documents.Core;
using NWheels.UI.Factories;

namespace NWheels.UI.Impl
{
    [TransactionScript(SupportsInitializeInput = true)]
    public class CrudEntityImportTx : ITransactionScript<Empty.Context, CrudEntityImportTx.IInput, Empty.Output>
    {
        private readonly IFramework _framework;
        private readonly IViewModelObjectFactory _viewModelFactory;
        private readonly IReadOnlyDictionary<string, IEntityExportFormat> _exportFormatByName;
        private readonly IReadOnlyDictionary<string, IInputDocumentParser> _parserByFormatIdName;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public CrudEntityImportTx(
            IFramework framework,
            IViewModelObjectFactory viewModelFactory, 
            IEnumerable<IEntityExportFormat> exportFormats,
            IEnumerable<IInputDocumentParser> inputParsers)
        {
            _framework = framework;
            _viewModelFactory = viewModelFactory;
            _exportFormatByName = exportFormats.ToDictionary(f => f.FormatTitle);
            _parserByFormatIdName = inputParsers.ToDictionary(f => f.MetaFormat.IdName);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of ITransactionScript<Context,IInput,Output>

        public IInput InitializeInput(Empty.Context context)
        {
            var input = _viewModelFactory.NewEntity<IInput>();

            if (_exportFormatByName.Count > 0)
            {
                input.Format = _exportFormatByName.First().Key;
            }

            return input;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        public Empty.Output Preview(IInput input)
        {
            throw new NotSupportedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Empty.Output Execute(IInput input)
        {
            var uiContext = UIOperationContext.Current;
            var exportFormat = _exportFormatByName[input.Format];
            var inputParser = _parserByFormatIdName[exportFormat.DocumentFormatIdName];
            var entityHandler = uiContext.EntityService.GetEntityHandler(uiContext.EntityName);

            return new Empty.Output();
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [ViewModelContract]
        public interface IInput
        {
            [PropertyContract.Required, EntityExportFormatSemantics]
            string Format { get; set; }

            [PropertyContract.Required, PropertyContract.Semantic.FileUpload]
            byte[] File { get; set; }
        }
    }
}
