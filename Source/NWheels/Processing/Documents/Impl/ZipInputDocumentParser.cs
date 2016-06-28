using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Concurrency;
using NWheels.Processing.Documents.Core;
using NWheels.UI;

namespace NWheels.Processing.Documents.Impl
{
    public class ZipInputDocumentParser : IInputDocumentParser
    {
        #region Implementation of IInputDocumentParser

        public void ImportDataFromReportDocument(
            FormattedDocument document, 
            DocumentDesign design, 
            ApplicationEntityService entityService, 
            IWriteOnlyCollection<DocumentImportIssue> issues)
        {
            if (design.Options.CustomImport == null)
            {
                throw new InvalidOperationException("Cannot import data from ZIP file with no custom import callback specfiied.");
            }

            using (var uploadStream = new MemoryStream(document.Contents))
            {
                using (ZipArchive archive = new ZipArchive(uploadStream, ZipArchiveMode.Read))
                {
                    var importContext = new ZipImportDataContext(document, design, entityService, issues, archive);
                    design.Options.CustomImport(importContext);
                }
            }
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

        private static readonly DocumentFormat _s_metaFormat = new DocumentFormat("ZIP", "application/octet-stream", "zip", "data.zip");
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class ZipImportDataContext
    {
        public ZipImportDataContext(
            FormattedDocument document, 
            DocumentDesign design, 
            ApplicationEntityService entityService, 
            IWriteOnlyCollection<DocumentImportIssue> issues, 
            ZipArchive archive)
        {
            Document = document;
            Design = design;
            EntityService = entityService;
            Issues = issues;
            Archive = archive;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public FormattedDocument Document { get; private set; }
        public DocumentDesign Design { get; private set; }
        public ApplicationEntityService EntityService { get; private set; }
        public IWriteOnlyCollection<DocumentImportIssue> Issues { get; private set; }
        public ZipArchive Archive { get; private set; }
    }
}
