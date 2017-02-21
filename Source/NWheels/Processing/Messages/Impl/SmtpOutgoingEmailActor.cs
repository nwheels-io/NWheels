using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading;
using NWheels.Configuration;
using NWheels.DataObjects;
using NWheels.Extensions;
using NWheels.Logging;
using NWheels.Utilities;

namespace NWheels.Processing.Messages.Impl
{
    public class SmtpOutgoingEmailActor : IMessageHandler<OutgoingEmailMessage>
    {
        private readonly IConfigSection _configSection;
        private readonly ILogger _logger;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public SmtpOutgoingEmailActor(IConfigSection configSection, ILogger logger)
        {
            _configSection = configSection;
            _logger = logger;

            GlobalInitialize(configSection);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of IMessageHandler<OutgoingEmailMessage>

        public void HandleMessage(OutgoingEmailMessage message)
        {
            string logFrom = null;
            string logTo = null;
            string logCc = null;
            string logBcc = null;
            string subject = null;
            string body = null;

            try
            {
                logFrom = (message.From != null ? string.Format("{0} <{1}>", message.From.DisplayName, message.From.EmailAddress) : _configSection.From);
                logTo = GetRecipientsLogString(message.To);
                logCc = GetRecipientsLogString(message.Cc);
                logBcc = GetRecipientsLogString(message.Bcc);

                var smtp = new SmtpClient(_configSection.Host, _configSection.Port);

                if (!string.IsNullOrEmpty(_configSection.UserName))
                {
                    smtp.UseDefaultCredentials = false;
                    smtp.Credentials = new NetworkCredential(_configSection.UserName, _configSection.Password);
                }

                if (_configSection.UseSsl)
                {
                    smtp.EnableSsl = true;
                }

                if (message.From == null)
                {
                    message.From = new OutgoingEmailMessage.SenderRecipient(_configSection.From, _configSection.From);
                }

                message.FormatTemplates(out subject, out body);

                var smtpMessage = BuildMailMessage(message, subject, body);
                smtp.Send(smtpMessage);

                _logger.EmailSent(logFrom, logTo, logCc, logBcc, subject, body);
            }
            catch (Exception e)
            {
                _logger.EmailSendFailed(logFrom, logTo, logCc, logBcc, subject, body, e);
                throw;
            }
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private MailMessage BuildMailMessage(OutgoingEmailMessage source, string subject, string body)
        {
            var smtpMessage = new MailMessage();

            if (source.From != null)
            {
                smtpMessage.From = new MailAddress(
                    source.From.EmailAddress.OrDefaultIfNull("missing@sender.info"), 
                    source.From.DisplayName);
            }

            CopyRecipients(source.To, smtpMessage.To);
            CopyRecipients(source.Cc, smtpMessage.CC);
            CopyRecipients(source.Bcc, smtpMessage.Bcc);

            smtpMessage.Subject = subject;
            smtpMessage.SubjectEncoding = Encoding.UTF8;
            smtpMessage.Body = body;
            smtpMessage.BodyEncoding = Encoding.UTF8;
            smtpMessage.IsBodyHtml = (source.BodyHtmlTemplate != null);

            return smtpMessage;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        private void CopyRecipients(IEnumerable<OutgoingEmailMessage.SenderRecipient> source, MailAddressCollection destination)
        {
            if (source != null)
            {
                foreach (var recipient in source.Where(s => !string.IsNullOrEmpty(s.EmailAddress)))
                {
                    destination.Add(new MailAddress(recipient.EmailAddress, recipient.DisplayName));
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private string GetRecipientsLogString(IEnumerable<OutgoingEmailMessage.SenderRecipient> recipients)
        {
            if (recipients != null)
            {
                return string.Join(",", recipients.Select(r => string.Format("{0} <{1}>", r.DisplayName, r.EmailAddress)));
            }

            return null;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static bool _s_globalInitialized;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static void GlobalInitialize(IConfigSection configSection)
        {
            if (!_s_globalInitialized)
            {
                if (configSection.IgnoreCertificateErrors)
                {
                    ServicePointManager.ServerCertificateValidationCallback = (obj, cert, chain, errors) => {
                        return true;
                    };
                }

                _s_globalInitialized = true;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [ConfigurationSection(XmlName = "Framework.Smtp")]
        public interface IConfigSection : IConfigurationSection
        {
            [PropertyContract.Required]
            string Host { get; set; }

            [PropertyContract.DefaultValue(465)]
            int Port { get; set; } 

            [PropertyContract.DefaultValue(true)]
            bool UseSsl { get; set; }

            [PropertyContract.DefaultValue(false)]
            bool IgnoreCertificateErrors { get; set; }

            string UserName { get; set; }

            [PropertyContract.Semantic.Password]
            string Password { get; set; }

            [PropertyContract.Required]
            string From { get; set; }
        }
        
        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface ILogger : IApplicationEventLogger
        {
            [LogInfo]
            void EmailSent(
                string from, 
                string to, 
                [Detail] string cc, 
                [Detail] string bcc, 
                string subject, 
                [Detail] string body);

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            [LogError]
            void EmailSendFailed(
                string from,
                string to,
                [Detail] string cc,
                [Detail] string bcc,
                string subject,
                [Detail] string body,
                Exception error);
        }
    }
}
