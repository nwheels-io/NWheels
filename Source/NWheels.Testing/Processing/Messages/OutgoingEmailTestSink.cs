using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Processing.Messages;

namespace NWheels.Testing.Processing.Messages
{
    public class OutgoingEmailTestSink : IMessageHandler<OutgoingEmailMessage>
    {
        private readonly List<OutgoingEmailMessage> _sentEmails;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public OutgoingEmailTestSink()
        {
            _sentEmails = new List<OutgoingEmailMessage>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of IMessageHandler<OutgoingEmailMessage>

        void IMessageHandler<OutgoingEmailMessage>.HandleMessage(OutgoingEmailMessage message)
        {
            _sentEmails.Add(message);
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Clear()
        {
            _sentEmails.Clear();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IEnumerable<OutgoingEmailMessage> SentEmails
        {
            get
            {
                return _sentEmails;
            }
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public static class OutgoingEmailMessageEnumerableExtensions
    {
        public static IEnumerable<OutgoingEmailMessage> To(this IEnumerable<OutgoingEmailMessage> source, string recipientEmail)
        {
            return source.Where(e => e.To.Any(r => r.EmailAddress == recipientEmail));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static IEnumerable<OutgoingEmailMessage> To(this IEnumerable<OutgoingEmailMessage> source, string recipientEmail, string recipientName)
        {
            return source.Where(e => e.To.Any(r => r.EmailAddress == recipientEmail && r.PersonName == recipientName));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static IEnumerable<OutgoingEmailMessage> OfContentType(this IEnumerable<OutgoingEmailMessage> source, object contentType)
        {
            return source.Where(e => contentType.Equals(e.TemplateContentType));
        }
    }
}
