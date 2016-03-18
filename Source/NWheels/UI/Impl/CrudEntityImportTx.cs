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
    public class CrudEntityImportTx : ITransactionScript<CrudEntityImportTx.IContext, CrudEntityImportTx.IInput, Empty.Output>
    {
        private readonly IFramework _framework;
        private readonly IViewModelObjectFactory _viewModelFactory;
        private readonly EntityImportExportFormatSet _formatSet;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public CrudEntityImportTx(
            IFramework framework,
            IViewModelObjectFactory viewModelFactory, 
            IEnumerable<IEntityExportFormat> exportFormats,
            IEnumerable<IInputDocumentParser> inputParsers)
        {
            _framework = framework;
            _viewModelFactory = viewModelFactory;
            _formatSet = new EntityImportExportFormatSet(exportFormats, inputParsers: inputParsers);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of ITransactionScript<Context,IInput,Output>

        public IInput InitializeInput(IContext context)
        {
            var availableFormats = _formatSet.GetAvailableFormats(UIOperationContext.Current, context.EntityName);
            var input = _viewModelFactory.NewEntity<IInput>();

            input.AvailableFormats = availableFormats.Select(f => f.FormatName).ToArray();

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
            IEntityExportFormat importFormat;

            var uiContext = UIOperationContext.Current;
            var inputParser = _formatSet.GetInputParser(input.Format, out importFormat);
            var entityHandler = uiContext.EntityService.GetEntityHandler(uiContext.EntityName);
            
            return new Empty.Output();
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [ViewModelContract]
        public interface IContext
        {
            string EntityName { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [ViewModelContract]
        public interface IInput
        {
            [PropertyContract.Required]
            string Format { get; set; }

            ICollection<string> AvailableFormats { get; set; }

            [PropertyContract.Required, PropertyContract.Semantic.FileUpload]
            byte[] File { get; set; }
        }
    }
}
