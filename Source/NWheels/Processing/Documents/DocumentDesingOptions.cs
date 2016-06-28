using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Processing.Documents
{
    public class DocumentDesingOptions
    {
        public DocumentDesingOptions(bool pagePerDataRow = false, Action<object> customImport = null, Action<object> customExport = null)
        {
            PagePerDataRow = pagePerDataRow;
            CustomImport = customImport;
            CustomExport = customExport;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool PagePerDataRow { get; private set; }
        public Action<object> CustomImport { get; private set; }
        public Action<object> CustomExport { get; private set; }
    }
}
