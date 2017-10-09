using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using System.Web;
using NWheels.Kernel.Api.Extensions;

namespace NWheels.Kernel.Api.Exceptions
{
    [Serializable]
    public abstract class ExplainableExceptionBase : Exception, IExplainableException
    {
        public readonly static string DefaultHelpLinkBaseUri = "https://nwheels.io/explain/";

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private readonly string _reason;
        private string _message = null;
        private string _helpLink = null;
        private string _explanationPath = null;
        private string _explanationQuery = null;
        private KeyValuePair<string, string>[] _keyValuePairs = null;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected ExplainableExceptionBase(string reason)
            : base(reason)
        {
            _reason = reason;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected ExplainableExceptionBase(string reason, Exception innerException)
            : base(reason, innerException)
        {
            _reason = reason;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected ExplainableExceptionBase(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string ExplanationPath
        {
            get
            {
                if (_explanationPath == null)
                {
                    _explanationPath = this.GetType().FriendlyFullName(fullNameGenericArgs: false);
                }

                return _explanationPath;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string ExplanationQuery
        {
            get
            {
                if (_explanationQuery == null)
                {
                    _explanationQuery = BuildExplanationQuery();
                }

                return _explanationQuery;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IEnumerable<KeyValuePair<string, string>> KeyValuePairs
        {
            get
            {
                EnsureKeyValuePairs();
                return _keyValuePairs;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string Reason => _reason;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override string Message
        {
            get
            {
                if (_message == null)
                {
                    _message = BuildMessage();
                }

                return _message;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override string HelpLink
        {
            get
            {
                if (_helpLink == null || _helpLink == _message)
                {
                    _helpLink = GetHelpLinkBaseUri() + ExplanationPath + '?' + ExplanationQuery;
                }

                return _helpLink;
            }   
            set
            {
                _helpLink = value;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected abstract IEnumerable<KeyValuePair<string, string>> BuildKeyValuePairs();

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual string BuildMessage()
        {
            EnsureKeyValuePairs();

            string messsageText;

            if (_keyValuePairs == null || _keyValuePairs.Length == 0)
            {
                messsageText = _reason.PascalCaseToHumanReadableText();
            }
            else
            {
                var keyValueStrings = _keyValuePairs.Select(kvp => kvp.Key + '=' + kvp.Value);
                messsageText =
                    _reason.PascalCaseToHumanReadableText() +
                    _s_messagsFirstParamSeparator +
                    string.Join(_s_messagsNextParamSeparator, keyValueStrings);
            }

            if (InnerException != null)
            {
                return messsageText + _s_messagsInnerExceptionSeparator + InnerException.Message + '}';
            }
            else
            {
                return messsageText;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected string BuildExplanationQuery()
        {
            EnsureKeyValuePairs();

            var minimalQuery = $"Reason={_s_urlEncoder.Encode(this.Reason)}";
            var keyValuePairs = this.KeyValuePairs;

            if (_keyValuePairs == null || _keyValuePairs.Length == 0)
            {
                return minimalQuery;
            }

            var builder = new StringBuilder(minimalQuery);

            for (int i = 0 ; i < _keyValuePairs.Length ; i++)
            {
                builder.Append($"&{_keyValuePairs[i].Key}={_s_urlEncoder.Encode(_keyValuePairs[i].Value ?? string.Empty)}");
            }

            return builder.ToString();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual string GetHelpLinkBaseUri()
        {
            return DefaultHelpLinkBaseUri;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void EnsureKeyValuePairs()
        {
            if (_keyValuePairs == null)
            {
                _keyValuePairs = BuildKeyValuePairs()?.ToArray();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static readonly UrlEncoder _s_urlEncoder = UrlEncoder.Default;
        private static readonly string _s_messagsFirstParamSeparator = ": ";
        private static readonly string _s_messagsNextParamSeparator = ", ";
        private static readonly string _s_messagsInnerExceptionSeparator = " {";
    }
}
