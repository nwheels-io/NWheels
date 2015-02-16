using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using NWheels.Extensions;

namespace NWheels.Logging
{
    [DataContract(Namespace = "NWheels.Logging", Name = "Log")]
    public class FormattedLogNode : LogNode
    {
        public const string AdHocFormattedMessageId = "Formatted.AdHoc";

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private readonly string _singleLineText;
        private readonly string _fullDetailsText;
        private readonly Exception _exception;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public FormattedLogNode(
            LogLevel level, 
            string singleLineText,
            string fullDetailsText = null, 
            LogContentTypes contentTypes = LogContentTypes.Text, 
            Exception exception = null)
            : this(AdHocFormattedMessageId, level, singleLineText, fullDetailsText, contentTypes, exception)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public FormattedLogNode(
            string messageId,
            LogLevel level,
            string singleLineText,
            string fullDetailsText = null,
            LogContentTypes contentTypes = LogContentTypes.Text,
            Exception exception = null)
            : base(messageId, contentTypes, level)
        {
            _singleLineText = singleLineText;
            _fullDetailsText = fullDetailsText;
            _exception = exception;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override string FormatSingleLineText()
        {
            return _singleLineText;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override string FormatFullDetailsText()
        {
            return _fullDetailsText;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override Exception Exception
        {
            get
            {
                return _exception;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override string FormatNameValuePairsText(string delimiter)
        {
            return (
                base.FormatNameValuePairsText(delimiter) + delimiter +
                FormatNameValuePair("text", _singleLineText) + delimiter + 
                FormatNameValuePair("details", _fullDetailsText));
        }
    }
}
