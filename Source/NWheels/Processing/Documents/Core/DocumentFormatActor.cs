using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using NWheels.Authorization;
using NWheels.Extensions;
using NWheels.Processing.Commands.Impl;
using NWheels.Processing.Messages;
using NWheels.UI;

namespace NWheels.Processing.Documents.Core
{
    public class DocumentFormatActor : 
        CommandActorBase,
        IMessageHandler<DocumentFormatRequestMessage>
    {
        private readonly IFramework _framework;
        private readonly IReadOnlyDictionary<string, IOutputDocumentFormatter> _formatterByFormatIdName;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public DocumentFormatActor(
            IComponentContext components, 
            IFramework framework, 
            ICommandActorLogger logger, 
            IServiceBus serviceBus, 
            ISessionManager sessionManager,
            IEnumerable<IOutputDocumentFormatter> formatters)
            : base(components, framework, logger, serviceBus, sessionManager)
        {
            _framework = framework;
            _formatterByFormatIdName = formatters.ToDictionary(f => f.MetaFormat.IdName, StringComparer.InvariantCultureIgnoreCase);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of IMessageHandler<DocumentFormatRequestMessage>

        public void HandleMessage(DocumentFormatRequestMessage message)
        {
            ExecuteCommand(message, ExecuteFormatDocument);
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private DocumentFormatReplyMessage ExecuteFormatDocument(DocumentFormatRequestMessage message)
        {
            var formatter = _formatterByFormatIdName.ReadOnlyGetOrThrow(
                message.OutputFormatIdName,
                "No IOutputMessageFormatter implementation was registered for document format id: '{0}'.");

            FormattedDocument document = null;

            switch ( message.RequestType )
            {
                case DocumentFormatRequestType.FixedDocument:
                    document = formatter.FormatFixedDocument(message.DocumentModel, message.DocumentDesign);
                    break;
                case DocumentFormatRequestType.Report:
                    message.EntityService.ProcessEntityCursor(
                        message.ReportQueryOptions.EntityName,
                        message.ReportQuery,
                        message.ReportQueryOptions,
                        cursor => {
                            document = formatter.FormatReportDocument(message.ReportCriteria, cursor, message.DocumentDesign);
                        });
                    break;
                default:
                    throw new ArgumentException("Request type not recognized: " + message.RequestType);
            }

            return new DocumentFormatReplyMessage(_framework, message.Session, message.MessageId, document);
        }
    }
}
