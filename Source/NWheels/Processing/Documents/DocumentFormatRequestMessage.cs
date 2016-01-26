﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NWheels.Authorization;
using NWheels.Entities.Core;
using NWheels.Processing.Commands;
using NWheels.Processing.Messages;
using NWheels.UI;

namespace NWheels.Processing.Documents
{
    public class DocumentFormatRequestMessage : AbstractCommandMessage
    {
        public DocumentFormatRequestMessage(
            IFramework framework, 
            ISession session, 
            bool isSynchronous,
            ApplicationEntityService entityService,
            IDomainObject documentModel, 
            DocumentDesign documentDesign, 
            string outputFormatIdName)
            : base(framework, session, isSynchronous)
        {
            this.RequestType = DocumentFormatRequestType.FixedDocument;
            this.OutputFormatIdName = outputFormatIdName;
            this.EntityService = entityService;
            this.DocumentModel = documentModel;
            this.DocumentDesign = documentDesign;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public DocumentFormatRequestMessage(
            IFramework framework,
            ISession session,
            bool isSynchronous,
            ApplicationEntityService entityService,
            IDomainObject reportCriteria,
            IQueryable reportQuery,
            ApplicationEntityService.QueryOptions reportQueryOptions,
            DocumentDesign documentDesign,
            string outputFormatIdName)
            : base(framework, session, isSynchronous)
        {
            this.RequestType = DocumentFormatRequestType.Report;
            this.OutputFormatIdName = outputFormatIdName;
            this.EntityService = entityService;
            this.ReportCriteria = reportCriteria;
            this.ReportQuery = reportQuery;
            this.ReportQueryOptions = reportQueryOptions;
            this.DocumentDesign = documentDesign;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override bool CheckAuthorization(out bool authenticationRequired)
        {
            authenticationRequired = true;
            return Thread.CurrentPrincipal.Identity.IsAuthenticated;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public DocumentFormatRequestType RequestType { get; private set; }
        public ApplicationEntityService EntityService { get; private set; }
        public IDomainObject DocumentModel { get; private set; }
        public IDomainObject ReportCriteria { get; private set; }
        public IQueryable ReportQuery { get; private set; }
        public ApplicationEntityService.QueryOptions ReportQueryOptions { get; private set; }
        public DocumentDesign DocumentDesign { get; private set; }
        public string OutputFormatIdName { get; private set; }
    }
}