using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Processing.Documents
{
    public class DocumentMetadata
    {
        public DocumentMetadata(DocumentFormat format)
            : this(format, format.DefaultFileName)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public DocumentMetadata(DocumentFormat format, string fileName)
        {
            Format = format;
            FileName = fileName;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public DocumentFormat Format { get; private set; }
        public string FileName { get; private set; }
    }
}
