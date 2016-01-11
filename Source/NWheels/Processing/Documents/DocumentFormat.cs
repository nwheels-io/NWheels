using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Processing.Documents
{
    public class DocumentFormat
    {
        public DocumentFormat(string idName, string contentType, string fileExtension, string defaultFileName)
        {
            IdName = idName;
            ContentType = contentType;
            FileExtension = fileExtension;
            DefaultFileName = defaultFileName;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string IdName { get; private set; }
        public string ContentType { get; private set; }
        public string FileExtension { get; private set; }
        public string DefaultFileName { get; private set; }
    }
}
