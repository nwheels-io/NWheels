using System;
using System.Text;
using NWheels.DataObjects;
using NWheels.Entities.Core;
using NWheels.Processing.Documents.Core;
using NWheels.UI;

namespace NWheels.Processing.Documents.Impl
{
    public class CsvOutputDocumentFormatter : IOutputDocumentFormatter
    {
        private readonly ITypeMetadataCache _metadataCache;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public CsvOutputDocumentFormatter(ITypeMetadataCache metadataCache)
        {
            _metadataCache = metadataCache;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of IOutputDocumentFormatter

        public FormattedDocument FormatReportDocument(IDomainObject criteria, ApplicationEntityService.EntityCursor queryResults, DocumentDesign design)
        {
            var output = new StringBuilder();

            for ( int i = 0 ; i < queryResults.ColumnCount ; i++ )
            {
                if ( i > 0 )
                {
                    output.Append(',');
                }

                output.Append(CsvEncode(queryResults.Columns[i].AliasName));
            }

            foreach ( var row in queryResults )
            {
                for ( int i = 0; i < queryResults.ColumnCount; i++ )
                {
                    if ( i > 0 )
                    {
                        output.Append(',');
                    }

                    output.Append(CsvEncode(row[i].ToString()));
                }
            }


            var metaDocument = new DocumentMetadata(_s_metaFormat, "report.csv");
            return new FormattedDocument(metaDocument, Encoding.UTF8.GetBytes(output.ToString()));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public FormattedDocument FormatFixedDocument(IDomainObject model, DocumentDesign design)
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public DocumentFormat MetaFormat
        {
            get
            {
                return _s_metaFormat;
            }
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static readonly DocumentFormat _s_metaFormat = new DocumentFormat("CSV", "text/csv", "csv", "data.csv");

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static string CsvEncode(string s)
        {
            s = s.Replace("\"", "\"\"");

            if ( s.Contains(",") || s.Contains(Environment.NewLine) )
            {
                s = "\"" + s.Replace("\"", "\\\"") + "\"";
            }

            return s;
        }
    }
}
