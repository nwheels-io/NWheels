using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Processing.Documents;

namespace NWheels.UI
{
    public interface IEntityExportFormat
    {
        Type EntityContract { get; }
        string DocumentFormatIdName { get; }
        DocumentDesign DocumentDesign { get; }
        string FormatTitle { get; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public abstract class EntityExportFormatBase<TEntity> : IEntityExportFormat
    {
        protected EntityExportFormatBase(string formatTitle, string documentFormatIdName)
        {
            this.FormatTitle = formatTitle;
            this.DocumentFormatIdName = documentFormatIdName;
            this.DocumentDesign = BuildDocumentDesign();
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public Type EntityContract
        {
            get { return typeof(TEntity); }
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public string FormatTitle { get; private set; }
        public string DocumentFormatIdName { get; private set; }
        public DocumentDesign DocumentDesign { get; private set; }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        protected abstract DocumentDesign BuildDocumentDesign();
    }
}
