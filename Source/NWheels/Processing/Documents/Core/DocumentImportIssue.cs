using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Logging;
using NWheels.UI;

namespace NWheels.Processing.Documents.Core
{
    [ViewModelContract]
    public interface IDocumentImportIssue
    {
        string Id { get; set; }
        SeverityLevel Severity { get; }
        string Code { get; }
        string Text { get; }
        string Location { get; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class DocumentImportIssue : IDocumentImportIssue
    {
        public DocumentImportIssue(SeverityLevel severity, string code, string text = null, string location = null)
        {
            this.Severity = severity;
            this.Code = code;
            this.Text = text;
            this.Location = location;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string Id { get; set; }
        public SeverityLevel Severity { get; private set; }
        public string Code { get; private set; }
        public string Text { get; private set; }
        public string Location { get; private set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static DocumentImportIssue NoneReported()
        {
            return new DocumentImportIssue(SeverityLevel.Info, "NoneReported");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static DocumentImportIssue Done(int errors, int warnings)
        {
            return new DocumentImportIssue(SeverityLevel.Info, "Done", string.Format("DONE: {0} errors, {1} warnings", errors, warnings));
        }
    }
}
