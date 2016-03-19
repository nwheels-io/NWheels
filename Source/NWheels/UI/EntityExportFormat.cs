using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.DataObjects;
using NWheels.Processing.Documents;

namespace NWheels.UI
{
    public interface IEntityExportFormat
    {
        Type EntityContract { get; }
        string DocumentFormatIdName { get; }
        DocumentDesign DocumentDesign { get; }
        string FormatName { get; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public abstract class EntityExportFormatBase<TEntity> : IEntityExportFormat
    {
        private DocumentDesign _documentDesign;

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        protected EntityExportFormatBase(ITypeMetadataCache metadataCache, string formatName, string documentFormatIdName)
        {
            this.MetadataCache = metadataCache;
            this.FormatName = formatName;
            this.DocumentFormatIdName = documentFormatIdName;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public Type EntityContract
        {
            get { return typeof(TEntity); }
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public string FormatName { get; private set; }
        public string DocumentFormatIdName { get; private set; }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public DocumentDesign DocumentDesign
        {
            get
            {
                if (_documentDesign == null)
                {
                    _documentDesign = BuildDocumentDesign();
                }

                return _documentDesign;
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        protected abstract DocumentDesign BuildDocumentDesign();

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        protected ITypeMetadataCache MetadataCache { get; private set; }
    }
}
