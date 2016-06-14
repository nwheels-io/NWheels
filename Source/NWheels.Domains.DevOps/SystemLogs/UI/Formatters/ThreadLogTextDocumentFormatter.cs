using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.DataObjects.Core;
using NWheels.Domains.DevOps.SystemLogs.Entities;
using NWheels.Processing.Documents;
using NWheels.Processing.Documents.Core;
using NWheels.UI;

namespace NWheels.Domains.DevOps.SystemLogs.UI.Formatters
{
    public class ThreadLogTextDocumentFormatter : IOutputDocumentFormatter
    {
        #region Implementation of IOutputDocumentFormatter

        public FormattedDocument FormatReportDocument(IObject criteria, ApplicationEntityService.EntityCursor queryResults, DocumentDesign design)
        {
            var output = new MemoryStream();
            string firstLogId = null;

            foreach (var row in queryResults)
            {
                var rootNode = (IRootThreadLogUINodeEntity)row.Record;

                if (firstLogId == null)
                {
                    firstLogId = rootNode.LogId;
                }

                WriteThreadLogText(rootNode, output);
            }

            var fileName = (firstLogId != null ? firstLogId + ".threadlog.txt" : "threadlog.txt");

            return new FormattedDocument(
                new DocumentMetadata(_s_format, fileName), 
                output.ToArray());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        public FormattedDocument FormatFixedDocument(IObject model, DocumentDesign design)
        {
            throw new NotSupportedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public DocumentFormat MetaFormat
        {
            get { return _s_format; }
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void WriteThreadLogText(IRootThreadLogUINodeEntity rootNode, MemoryStream output)
        {
            using (var writer = new StreamWriter(output))
            {
                WriteLogNodeText(0, rootNode, writer);
                writer.Flush();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void WriteLogNodeText(int depth, IThreadLogUINodeEntity logNode, StreamWriter writer)
        {
            var depthIndent = MakeFirstRowDepthIndent(depth);
            var subRowIndent = MakeSubRowIndent(depth);

            writer.WriteLine(
                "{0:yyyy-MM-dd HH:mm:sss} {1}{2,-16} {3}", 
                logNode.Timestamp, depthIndent, logNode.NodeType, IdnentMultiLineText(logNode.Text, subRowIndent));

            if (logNode.Exception != null)
            {
                writer.WriteLine(
                    "{0}EXCEPTION! {1}", 
                    _s_subRowIndent, IdnentMultiLineText(logNode.Exception, subRowIndent));
            }

            if (logNode.SubNodes != null)
            {
                foreach (var subNode in logNode.SubNodes)
                {
                    WriteLogNodeText(depth + 1, subNode, writer);
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private string IdnentMultiLineText(string text, string subRowIndent)
        {
            return text.Replace("\r\n", "\r\n" + subRowIndent);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private string MakeFirstRowDepthIndent(int depth)
        {
            var indent = string.Empty;

            for (int i = 0 ; i < depth ; i++)
            {
                indent += _s_firstRowLevelIndent;
            }

            return indent;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private string MakeSubRowIndent(int depth)
        {
            var indent = _s_subRowIndent;

            for (int i = 0; i < depth; i++)
            {
                indent += _s_subRowLevelIndent;
            }

            return indent;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static readonly string _s_subRowIndent = new string(' ', 20);
        private static readonly string _s_firstRowLevelIndent = " . ";
        private static readonly string _s_subRowLevelIndent = "   ";

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static readonly DocumentFormat _s_format = new DocumentFormat(
            idName: "THREAD-LOG-TEXT", 
            contentType: "text/plain", 
            fileExtension: "threadlog.txt", 
            defaultFileName: "thread");

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static DocumentFormat Format
        {
            get { return _s_format;  }
        }
    }
}
