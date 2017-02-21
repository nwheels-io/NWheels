using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using NWheels.DataObjects;
using NWheels.Domains.DevOps.Alerts.Entities;
using NWheels.Domains.DevOps.SystemLogs.Entities;
using NWheels.Domains.DevOps.SystemLogs.Transactions;
using NWheels.Extensions;
using NWheels.Processing.Jobs;
using NWheels.Processing.Messages;
using NWheels.Processing.Messages.Impl;
using NWheels.UI;
using NWheels.Utilities;

namespace NWheels.Domains.DevOps.Alerts.Jobs
{
    [ApplicationJob("SystemAlertJob")]
    public class SystemAlertJob : ApplicationJobBase
    {
        private readonly IFramework _framework;
        private readonly ITypeMetadataCache _metadataCache;
        private readonly IContentTemplateProvider _templateProvider;
        private readonly ServiceBus _serviceBus;
        private readonly AbstractLogMessageListTx _messageListTx;
        private readonly ISystemAlertConfigurationFeatureSection _systemAlertConfiguration;
        private CrossInvocationJobState _crossInvocationState;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public SystemAlertJob(
            IFramework framework, 
            ITypeMetadataCache metadataCache, 
            IContentTemplateProvider templateProvider,
            ServiceBus serviceBus, 
            AbstractLogMessageListTx messageListTx,
            ISystemAlertConfigurationFeatureSection systemAlertConfiguration)
        {
            _framework = framework;
            _metadataCache = metadataCache;
            _templateProvider = templateProvider;
            _serviceBus = serviceBus;
            _messageListTx = messageListTx;
            _systemAlertConfiguration = systemAlertConfiguration;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of ApplicationJobBase

        public override void Execute(IApplicationJobContext context)
        {
            if (_crossInvocationState == null)
            {
                _crossInvocationState = new CrossInvocationJobState(this);
            }

            var operation = new DetectAndNotifyOperation(_crossInvocationState, _systemAlertConfiguration);
            operation.ExecuteOnce();
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Refresh()
        {
            _crossInvocationState.RefreshConfiguration();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class ImmutableConfiguration
        {
            private readonly Dictionary<string, IEntityPartEmailRecipient[]> _emailRecipientPerAlertMessageId;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ImmutableConfiguration(IEnumerable<ISystemAlertConfigurationEntity> alerts)
            {
                _emailRecipientPerAlertMessageId = new Dictionary<string, IEntityPartEmailRecipient[]>();

                foreach (var alert in alerts)
                {
                    var recipients = alert
                        .AlertActions
                        .OfType<IEntityPartByEmailAlertAction>()
                        .Select(action => action.Recipients)
                        .SelectMany(x => x)
                        .ToArray();

                    _emailRecipientPerAlertMessageId[alert.Id] = recipients;
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public IEnumerable<string> GetConfiguredAlertIds()
            {
                return _emailRecipientPerAlertMessageId.Keys;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public IEnumerable<AlertMessageRecipientPair> MultiplyAlertMessageByRecipients(ILogMessageEntity message)
            {
                IEntityPartEmailRecipient[] recipients;

                if (message.AlertId == null || !_emailRecipientPerAlertMessageId.TryGetValue(message.AlertId, out recipients))
                {
                    return Enumerable.Empty<AlertMessageRecipientPair>();
                }

                return recipients.Select(r => new AlertMessageRecipientPair(message, r));
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class AlertMessageRecipientPair
        {
            public AlertMessageRecipientPair(ILogMessageEntity alert, IEntityPartEmailRecipient recipient)
            {
                Alert = alert;
                Recipient = recipient;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ILogMessageEntity Alert { get; private set; }
            public IEntityPartEmailRecipient Recipient { get; private set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class CrossInvocationJobState
        {
            private readonly SystemAlertJob _owner;
            private DateTime _lastTimestamp;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public CrossInvocationJobState(SystemAlertJob owner)
            {
                _owner = owner;
                _lastTimestamp = owner._framework.UtcNow.AddMinutes(-10);

                this.LogMessageMetaType = owner._metadataCache.GetTypeMetadata(typeof(ILogMessageEntity));

                RefreshConfiguration();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void RefreshConfiguration()
            {
                using (var context = _owner._framework.NewUnitOfWork<ISystemAlertConfigurationContext>())
                {
                    var configuredAlerts = context.Alerts.AsQueryable().ToList();

                    //work around lazy loading
                    foreach (var alert in configuredAlerts)
                    {
                        foreach (var emailAction in alert.AlertActions.OfType<IEntityPartByEmailAlertAction>())
                        {
                            foreach (var recipient in emailAction.Recipients)
                            {
                                recipient.ToOutgoingEmailMessageRecipient();
                            }
                        }
                    }

                    this.Configuration = new ImmutableConfiguration(configuredAlerts);
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public DateTime TakeLastTimestamp()
            {
                var value = _lastTimestamp;
                _lastTimestamp = _owner._framework.UtcNow;
                return value;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ITypeMetadata LogMessageMetaType { get; private set; }
            public ImmutableConfiguration Configuration { get; private set; }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public IFramework Framework
            {
                get { return _owner._framework; }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ServiceBus ServiceBus
            {
                get { return _owner._serviceBus; }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public AbstractLogMessageListTx MessageListTx
            {
                get { return _owner._messageListTx; }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public IContentTemplateProvider TemplateProvider
            {
                get { return _owner._templateProvider; }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class DetectAndNotifyOperation
        {
            private static readonly PropertyInfo _s_alertIdPropertyInfo = 
                ExpressionUtility.GetPropertyInfoFrom<ILogMessageEntity>(x => x.AlertId);

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private readonly CrossInvocationJobState _jobState;
            private readonly ISystemAlertConfigurationFeatureSection _systemAlertConfiguration;
            private readonly IFramework _framework;
            private readonly ServiceBus _serviceBus;
            private readonly AbstractLogMessageListTx _messageListTx;
            private readonly char _splitChar = char.MinValue;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public DetectAndNotifyOperation(CrossInvocationJobState jobState, ISystemAlertConfigurationFeatureSection systemAlertConfiguration)
            {
                _jobState = jobState;
                _systemAlertConfiguration = systemAlertConfiguration;
                _framework = jobState.Framework;
                _serviceBus = jobState.ServiceBus;
                _messageListTx = jobState.MessageListTx;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void ExecuteOnce()
            {
                var alerts = FindNewAlerts();
                var emails = GroupAlertsIntoEmails(alerts);
                SendEmails(emails);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private IReadOnlyList<ILogMessageEntity> FindNewAlerts()
            {
                var query = new ApplicationEntityService.QueryOptions(_jobState.LogMessageMetaType.Name, new Dictionary<string, string>());

                query.Filter.Add(new ApplicationEntityService.QueryFilterItem(
                    propertyName: _s_alertIdPropertyInfo.Name,
                    @operator: ApplicationEntityService.QueryOptions.IsInOperator,
                    stringValue: string.Join(",", _jobState.Configuration.GetConfiguredAlertIds())));

                var criteria = new ExtendedLogTimeRangeCriteria() {
                    From = _jobState.TakeLastTimestamp(),
                    Until = _framework.UtcNow.AddMinutes(1),
                    Query = query
                };

                var results = _messageListTx.Execute(criteria).ToList();

                return results;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private IReadOnlyList<OutgoingEmailMessage> GroupAlertsIntoEmails(IReadOnlyList<ILogMessageEntity> newAlerts)
            {
                var emails = newAlerts
                    .SelectMany(message => _jobState.Configuration.MultiplyAlertMessageByRecipients(message))
                    .GroupBy(pair => pair.Recipient)
                    .Select(group => CreateEmailMessage(group.Key, group.Select(pair => pair.Alert)))
                    .ToList();

                return emails;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void SendEmails(IEnumerable<OutgoingEmailMessage> emails)
            {
                foreach (var email in emails)
                {
                    _serviceBus.EnqueueMessage(email);
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private OutgoingEmailMessage CreateEmailMessage(IEntityPartEmailRecipient recipient, IEnumerable<ILogMessageEntity> alerts)
            {
                var message = new OutgoingEmailMessage(_framework, _jobState.TemplateProvider);
                var messageRecipient = recipient.ToOutgoingEmailMessageRecipient();

                message.To.Add(messageRecipient);

                //message.LoadTemplate(SystemAlertReportTemplate.TemplateType.AlertsGroupedPerRecipient, subjectAtFirstLine: true);

                /*var reportData = new SystemAlertReportTemplate.AlertsGroupedPerRecipient() {
                    Recipient = messageRecipient
                };

                reportData.Alerts = alerts.Select(
                    a => new SystemAlertReportTemplate.AlertDetails() {
                        AlertId = a.AlertId,
                        Problem = a.MessageId.SplitPascalCase(),
                        Description = "", //_systemAlertConfiguration,
                        RequiredAction = "TODO: configure required action text"
                    }).ToList();
                */
                //message.TemplateData = reportData;

                string subject;
                int numOfAlerts = alerts.Count();
                if (numOfAlerts == 1)
                {
                    subject = string.Format("MS Alert: {0}", alerts.First().MessageId.SplitPascalCase());
                }
                else
                {
                    subject = string.Format("MS Alerts: {0} different alerts awaiting for you", numOfAlerts);
                }
                message.Subject = subject;
                message.BodyHtmlTemplate = string.Empty;

                var body = new StringBuilder();

                foreach (var groupedAlerts in alerts.GroupBy(alert => GetGroupByKeyByAlert(alert)))
                {
                    var alertConfig = _systemAlertConfiguration.AlertList[groupedAlerts.First().AlertId];
                    body.AppendLine(string.Format("<h3>{0}</h3> - <h1>{1}</h1>", groupedAlerts.Key.Replace(_splitChar, ' '), groupedAlerts.Count()));
                    body.AppendLine(string.Format("<p>Description: {0}</p>", alertConfig.Description));
                    body.AppendLine(string.Format("<p>Possible reasons: {0}</p>", alertConfig.PossibleReason));
                    body.AppendLine(string.Format("<p>Suggested Actions: {0}</p>", alertConfig.SuggestedAction));
                    body.AppendLine();
                }

                message.BodyHtmlTemplate = body.ToString();
                return message;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private string GetGroupByKeyByAlert(ILogMessageEntity logMessage)
            {
                var groupByConfig = _systemAlertConfiguration.AlertList[logMessage.AlertId];

                var groupByResult = logMessage.AlertId;
                foreach (var keyValue in logMessage.KeyValues)
                {
                    foreach (var groupItem in groupByConfig.GroupBy.Split(','))
                    {
                        if (keyValue.StartsWith(groupItem.Trim() + "="))
                        {
                            groupByResult += _splitChar + keyValue;
                        }
                    }
                }
                return groupByResult;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class ExtendedLogTimeRangeCriteria : IExtendedLogTimeRangeCriteria
        {
            #region Implementation of ILogTimeRangeCriteria

            public DateTime From { get; set; }
            public DateTime Until { get; set; }
            public string MessageId { get; set; }
            public int? SeriesIndex { get; set; }

            #endregion

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            #region Implementation of IExtendedLogTimeRangeCriteria

            public ApplicationEntityService.QueryOptions Query { get; set; }

            #endregion
        }
    }
}
