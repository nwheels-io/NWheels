using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Domains.DevOps.Alerts.Entities;
using NWheels.Domains.DevOps.SystemLogs.Entities;
using NWheels.Domains.DevOps.SystemLogs.Transactions;
using NWheels.Processing.Jobs;
using NWheels.Processing.Messages;
using NWheels.Processing.Messages.Impl;
using NWheels.UI;

namespace NWheels.Domains.DevOps.Alerts.Jobs
{
    public class SystemAlertJob : ApplicationJobBase
    {
        private readonly IFramework _framework;
        private readonly ServiceBus _serviceBus;
        private readonly AbstractLogMessageListTx _messageListTx;
        private ImmutableConfiguration _configuration;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public SystemAlertJob(IFramework framework, ServiceBus serviceBus, AbstractLogMessageListTx messageListTx)
        {
            _framework = framework;
            _serviceBus = serviceBus;
            _messageListTx = messageListTx;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of ApplicationJobBase

        public override void Execute(IApplicationJobContext context)
        {
            if (_configuration == null)
            {
                Refresh();
            }

            var operation = new DetectAndNotifyOperation(_configuration, _framework, _serviceBus, _messageListTx);
            operation.ExecuteOnce();
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Refresh()
        {
            using (var context = _framework.NewUnitOfWork<ISystemAlertConfigurationContext>())
            {
                var configuredAlerts = context.Alerts.AsQueryable().ToList();
                _configuration = new ImmutableConfiguration(configuredAlerts);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class ImmutableConfiguration
        {
            public ImmutableConfiguration(IEnumerable<ISystemAlertConfigurationEntity> alerts)
            {

            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public IEnumerable<AlertMessageRecipientPair> MultiplyAlertMessageByRecipients(ILogMessageEntity message)
            {
                throw new NotImplementedException();
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

        private class DetectAndNotifyOperation
        {
            private readonly ImmutableConfiguration _configuration;
            private readonly IFramework _framework;
            private readonly ServiceBus _serviceBus;
            private readonly AbstractLogMessageListTx _messageListTx;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public DetectAndNotifyOperation(
                ImmutableConfiguration configuration, 
                IFramework framework, 
                ServiceBus serviceBus, 
                AbstractLogMessageListTx messageListTx)
            {
                _configuration = configuration;
                _framework = framework;
                _serviceBus = serviceBus;
                _messageListTx = messageListTx;
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
                var criteria = new ExtendedLogTimeRangeCriteria() {
                    //TODO: build the right query
                };

                var results = _messageListTx.Execute(criteria).ToList();

                return results;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private IReadOnlyList<OutgoingEmailMessage> GroupAlertsIntoEmails(IReadOnlyList<ILogMessageEntity> newAlerts)
            {
                var emails = newAlerts
                    .SelectMany(message => _configuration.MultiplyAlertMessageByRecipients(message))
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
                throw new NotImplementedException();
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

            #region Implementation of IExtendedLogTimeRangeCriteria

            public ApplicationEntityService.QueryOptions Query { get; set; }

            #endregion
        }
    }
}
