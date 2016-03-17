using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Processing.Documents;

namespace NWheels.Stacks.Formats.EPPlus
{
    public static class ExcelMetaFormat
    {
        private static readonly DocumentFormat _s_format = new DocumentFormat(
            idName: "EXCEL",
            contentType: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileExtension: "xslx",
            defaultFileName: "workbook.xlsx");

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static DocumentFormat GetFormat()
        {
            return _s_format;
        }
    }
}
