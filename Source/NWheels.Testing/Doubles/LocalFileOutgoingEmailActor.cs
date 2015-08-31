using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NWheels.Processing.Messages;
using NWheels.Utilities;

namespace NWheels.Testing.Doubles
{
    public class LocalFileOutgoingEmailActor : IMessageHandler<OutgoingEmailMessage>
    {
        private readonly string _outputFolderPath;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public LocalFileOutgoingEmailActor()
        {
            _outputFolderPath = PathUtility.HostBinPath("Email.Out");

            if ( !Directory.Exists(_outputFolderPath) )
            {
                Directory.CreateDirectory(_outputFolderPath);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of IMessageHandler<OutgoingEmailMessage>

        public void HandleMessage(OutgoingEmailMessage message)
        {
            string subject;
            string body;
            message.FormatTemplates(out subject, out body);

            var output = new StringBuilder();

            output.AppendFormat("-- UTC DATE/TIME\r\n{0:yyyy-MM-dd HH:mm:ss}\r\n", DateTime.UtcNow);
            output.AppendFormat("-- TO\r\n{0}", string.Join("", message.To.Select(r => r.PersonName + " <" + r.EmailAddress + ">\r\n")));
            output.AppendFormat("-- CC\r\n{0}", string.Join("", message.Cc.Select(r => r.PersonName + " <" + r.EmailAddress + ">\r\n")));
            output.AppendFormat("-- BCC\r\n{0}", string.Join("", message.Bcc.Select(r => r.PersonName + " <" + r.EmailAddress + ">\r\n")));
            output.AppendFormat("-- SUBJECT\r\n{0}\r\n", subject);
            output.AppendFormat("-- BODY\r\n{0}\r\n", body);

            Thread.Sleep(10);

            var fileName = string.Format("{0:yyyy-MM-dd-HHmm-ssfff}.txt", DateTime.UtcNow);
            File.WriteAllText(Path.Combine(_outputFolderPath, fileName), output.ToString());
        }

        #endregion
    }
}
