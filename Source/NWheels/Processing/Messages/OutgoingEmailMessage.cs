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
        private readonly IContentTemplateProvider _templateProvider;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public OutgoingEmailMessage()
        {
            Initialize();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public OutgoingEmailMessage(IFramework framework)
            : base(framework)
        {
            Initialize();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public OutgoingEmailMessage(IFramework framework, IContentTemplateProvider templateProvider)
            : base(framework)
        {
            _templateProvider = templateProvider;
            Initialize();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void FormatTemplates(out string formattedSubject, out string formattedHtmlBody)
        {
            ValidateTemplateProvider();

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

            formattedSubject = _templateProvider.Format(subjectTemplate, this.TemplateData);
            formattedHtmlBody = _templateProvider.Format(bodyTemplate, this.TemplateData);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void LoadTemplate(object contentType, bool subjectAtFirstLine)
        {
            ValidateTemplateProvider();

            this.TemplateContentType = contentType;
            var template = _templateProvider.GetTemplate(contentType);

            if ( subjectAtFirstLine )
            {
                using ( var reader = new StringReader(template) )
                {
                    this.Subject = reader.ReadLine();
                    this.BodyHtmlTemplate = reader.ReadToEnd();
                }
            }
            else
            {
                this.BodyHtmlTemplate = template;
            }
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

        /// <summary>
        /// Content type value that was used when loading template.
        /// </summary>
        public object TemplateContentType { get; private set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Template provider which the current instance was initialized with (optional).
        /// </summary>
        public IContentTemplateProvider TemplateProvider 
        {
            get
            {
                return _templateProvider;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void Initialize()
        {
            this.To = new List<Recipient>();
            this.Cc = new List<Recipient>();
            this.Bcc = new List<Recipient>();
            this.Attachments = new List<Attachment>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void ValidateTemplateProvider()
        {
            if ( _templateProvider == null )
            {
                throw new InvalidOperationException("Current instance was not initialized with content provider.");
            }
        }

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

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public static class OutgoingEmailMessageExtensions
    {
        public static void AddRecipient(this List<OutgoingEmailMessage.Recipient> recipients, string personName, string emailAddress)
        {
            if ( !string.IsNullOrEmpty(emailAddress) )
            {
                recipients.Add(new OutgoingEmailMessage.Recipient(personName, emailAddress));
            }
        }
    }
}
