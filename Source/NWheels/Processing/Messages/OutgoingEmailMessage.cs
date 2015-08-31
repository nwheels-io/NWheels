using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartFormat;

namespace NWheels.Processing.Messages
{
    public class OutgoingEmailMessage : MessageObjectBase
    {
        public OutgoingEmailMessage()
        {
            this.To = new List<Recipient>();
            this.Cc = new List<Recipient>();
            this.Bcc = new List<Recipient>();
            this.Attachments = new List<Attachment>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void FormatTemplates(out string formattedSubject, out string formattedHtmlBody)
        {
            string subjectTemplate;
            string bodyTemplate;

            if ( SubjectAtBodyTemplateFirstLine )
            {
                using ( var reader = new StringReader(BodyHtmlTemplate) )
                {
                    subjectTemplate = reader.ReadLine();
                    bodyTemplate = reader.ReadToEnd();
                }
            }
            else
            {
                subjectTemplate = Subject;
                bodyTemplate = BodyHtmlTemplate;
            }

            formattedSubject = Smart.Format(subjectTemplate, this.TemplateData);
            formattedHtmlBody = Smart.Format(bodyTemplate, this.TemplateData);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public List<Recipient> To { get; private set; }
        public List<Recipient> Cc { get; private set; }
        public List<Recipient> Bcc { get; private set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Specifies the subject line of the email. Can contain value placeholders with expressions evaluated on the TemplateData property.
        /// Note: this property is ignored if SubjectAtBodyTemplateFirstLine is true.
        /// </summary>
        public string Subject { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Specifies that first line of the body template should be treated as subject line. If true, Subject property is ignored, and the body starts 
        /// on the second line of the template.
        /// </summary>
        public bool SubjectAtBodyTemplateFirstLine { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// The HTML contents of the email. Can contain value placeholders with expressions evaluated on the TemplateData property.
        /// </summary>
        public string BodyHtmlTemplate { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// An object that contains values for placeholders in the template.
        /// </summary>
        public object TemplateData { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Attachments to the email
        /// </summary>
        public List<Attachment> Attachments { get; private set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class Recipient
        {
            public Recipient(string personName, string emailAddress)
            {
                PersonName = personName;
                EmailAddress = emailAddress;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public string PersonName { get; private set; }
            public string EmailAddress { get; private set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class Attachment
        {
            public Attachment(string fileName, byte[] fileContents)
            {
                this.FileName = fileName;
                this.FileExtension = Path.GetExtension(fileName);
                this.FileContents = fileContents;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public Attachment(string fileName, Stream fileContents)
            {
                this.FileName = fileName;
                this.FileExtension = Path.GetExtension(fileName);
                
                using ( var buffer = new MemoryStream() )
                {
                    fileContents.CopyTo(buffer);
                    this.FileContents = buffer.ToArray();
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public string FileName { get; private set; }
            public string FileExtension { get; private set; }
            public byte[] FileContents { get; private set; }
        }
    }
}
