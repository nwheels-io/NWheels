using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Processing.Documents
{
    public class FormattedDocument
    {
        public FormattedDocument(DocumentMetadata metadata, byte[] contents)
        {
            Metadata = metadata;
            Contents = contents;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public DocumentMetadata Metadata { get; private set; }
        public byte[] Contents { get; private set; }
    }
}
